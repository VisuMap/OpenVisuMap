﻿// Copyright (C) 2020 VisuMap Technologies Inc.
using System;
using System.Linq;
using Buffer = SharpDX.Direct3D11.Buffer;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TsneDx {    
    public class FastPca {
        [StructLayout(LayoutKind.Explicit)]
        public struct PcaConstants {
            [FieldOffset(0)] public int rows;
            [FieldOffset(4)] public int columns;
            [FieldOffset(8)] public int iBlock;
            [FieldOffset(12)] public int eigenCount;
            [FieldOffset(16)] public int eigenIdx;
            [FieldOffset(20)] public int groupNumber;
            [FieldOffset(24)] public float covFactor;
        };

        const double epsilon = 1e-6;
        const double stopEpsilon = 1e-5;
        const int MAX_ITERATION = 30;

        // Reduce give matrix to top PC components.
        public float[][] DoPca(float[][] A, int eigenCount) {
            GpuDevice gpu = new GpuDevice();
            var cc = gpu.CreateConstBuffer<PcaConstants>(0);

            bool transposing = (A.Length > A[0].Length);
            cc.c.eigenCount = eigenCount;
            cc.c.rows = transposing ? A[0].Length : A.Length;
            cc.c.columns = (!transposing) ? A[0].Length : A.Length;

            var resultBuf = gpu.CreateBufferRW(3, 4, 1);  // to receive the total changes.
            var resultStaging = gpu.CreateStagingBuffer(resultBuf);

            Buffer tableBuf = gpu.CreateBufferRO(cc.c.rows * cc.c.columns, 4, 0);
            double[] colMean = new double[A[0].Length];
            Parallel.For(0, A[0].Length, col => {
                colMean[col] = 0.0;
                for (int row = 0; row < A.Length; row++) {
                    colMean[col] += A[row][col];
                }
                colMean[col] /= A.Length;
            });

            using (var ds = gpu.NewWriteStream(tableBuf)) {
                float[] buf = new float[cc.c.rows * cc.c.columns];
                if (transposing) {
                    Parallel.For(0, cc.c.columns, col => {
                        int offset = col * cc.c.rows;
                        for (int row = 0; row < cc.c.rows; row++)
                            buf[offset + row] = (float)(A[col][row] - colMean[row]);
                    });
                } else {
                    Parallel.For(0, cc.c.columns, col => {
                        int offset = col * cc.c.rows;
                        for (int row = 0; row < cc.c.rows; row++)
                            buf[offset+row] = (float)(A[row][col] - colMean[col]);
                    });
                }
                ds.WriteRange(buf);
            }

            cc.c.covFactor = transposing ? 1.0f / (cc.c.columns - 1) : 1.0f / (cc.c.rows - 1);
            Buffer covBuf = gpu.CreateBufferRW(cc.c.rows * cc.c.rows, 4, 0);

            using (var shader = gpu.LoadShader("TsneDx.PcaCreateCovMatrix.cso")) {
                gpu.SetShader(shader);
                cc.c.groupNumber = 256;
                for (int iBlock = 0; iBlock < cc.c.rows; iBlock += cc.c.groupNumber) {
                    cc.c.iBlock = iBlock;
                    cc.Upload();
                    gpu.Run(cc.c.groupNumber);
                }
            }

            var eVectorBuf = gpu.CreateBufferRW(cc.c.rows, 4, 2);
            var eVectorStaging = gpu.CreateStagingBuffer(eVectorBuf);
            var eVector2Buf = gpu.CreateBufferRW(cc.c.rows, 4, 3);

            var sdInit = gpu.LoadShader("TsneDx.PcaInitIteration.cso");
            var sdStep = gpu.LoadShader("TsneDx.PcaIterateOneStep.cso");
            var sdNorm = gpu.LoadShader("TsneDx.PcaCalculateNormal.cso");
            var sdAdjCov = gpu.LoadShader("TsneDx.PcaAdjustCovMatrix.cso");

            gpu.SetShader(sdInit);
            cc.c.eigenIdx = 0;
            cc.Upload();
            gpu.Run();

            float preEigen = 1e30f;
            float newEigen = 0;
            float[][] eVectors = new float[eigenCount][];
            double[] eValues = new double[eigenCount];

            for (int eigenIdx = 0; eigenIdx < eigenCount; eigenIdx++) {
                cc.c.groupNumber = 256;
                cc.Upload();

                for (int repeat = 0; repeat < MAX_ITERATION; repeat++) {
                    gpu.SetShader(sdStep);
                    gpu.Run(cc.c.groupNumber);
                    gpu.SetShader(sdNorm);
                    gpu.Run(1);
                    newEigen = gpu.ReadFloat(resultStaging, resultBuf);
                    double delta = Math.Abs((newEigen - preEigen) / preEigen);
                    if (delta < epsilon)
                        break;
                    preEigen = newEigen;
                }

                eValues[eigenIdx] = (double) newEigen;

                // Eigenvector with extrem small eigenvalue (i.e. 0.0) will be ignored and stop the calculation.
                if (Math.Abs(eValues[eigenIdx] / eValues[0]) < stopEpsilon) {
                    Array.Resize(ref eValues, eigenIdx);
                    Array.Resize(ref eVectors, eigenIdx);
                    break;
                }

                eVectors[eigenIdx] = new float[cc.c.rows];
                Array.Copy(gpu.ReadRange<float>(eVectorStaging, eVectorBuf, cc.c.rows), eVectors[eigenIdx], cc.c.rows);

                if (eigenIdx == (eigenCount - 1)) break;

                // Adjust the covariance matrix.
                gpu.SetShader(sdAdjCov);
                cc.c.groupNumber = 128;
                cc.Upload();
                gpu.Run(cc.c.groupNumber);

                // Initialize the iteration loop for the next eigen-vector.
                gpu.SetShader(sdInit);
                cc.c.eigenIdx = eigenIdx + 1;
                cc.Upload();
                gpu.Run();
                //CmdSynchronize();
            }

            if (!transposing) {
                using (var shader = gpu.LoadShader("TsneDx.PcaTransposeEigenvectors.cso")) {
                        int eRows = eVectors.Length;
                        int eColumns = eVectors[0].Length;
                    Buffer eigenList1 = gpu.CreateBufferRO(eRows * eColumns, 4, 1);
                    double[] S = eValues.Select(x => 1.0 / Math.Sqrt(Math.Abs(x * (eVectors[0].Length - 1)))).ToArray();
                    float[] eVector1 = new float[eRows * eColumns];
                    for(int row=0; row<eRows; row++)
                    for (int col = 0; col < eColumns; col++)
                            eVector1[row * eColumns + col] = (float)(S[row] * eVectors[row][col]);

                    using (var ds = gpu.NewWriteStream(eigenList1))
                        ds.WriteRange(eVector1);

                    Buffer eigenList2 = gpu.CreateBufferRW(eVectors.Length * cc.c.columns, 4, 4);
                    gpu.SetShader(shader);
                    cc.c.groupNumber = 128;
                    cc.c.eigenCount = eVectors.Length;
                    cc.Upload();
                    gpu.Run(cc.c.groupNumber);

                    float[] eVectors2 = gpu.ReadRange<float>(eigenList2, eVectors.Length * cc.c.columns);
                    eVectors = new float[eVectors.Length][];
                    for(int row=0; row< eVectors.Length; row++)
                        eVectors[row] = new float[cc.c.columns];
                    Parallel.For(0, eVectors.Length, row => {
                        Array.Copy(eVectors2, row * cc.c.columns, eVectors[row], 0, cc.c.columns);
                    });

                    TsneDx.SafeDispose(eigenList1, eigenList2);
                }
            }

            float[][] B = null;
            cc.c.rows = A.Length;
            cc.c.columns = A[0].Length;
            cc.c.eigenCount = eVectors.Length;
            cc.Upload();

            if (transposing) {
                // The tableBuf on GPU is in wrong matrix order. We need to upload the tableBuf in needed order here.
                TsneDx.SafeDispose(tableBuf);
                tableBuf = gpu.CreateBufferRO(cc.c.rows * cc.c.columns, 4, 0);
                Parallel.For(0, cc.c.rows, row => {
                    for (int col = 0; col < cc.c.columns; col++)
                        A[row][col] -= (float)colMean[col];
                });
                gpu.WriteMarix(tableBuf, A);
            }

            Buffer eigenTable = gpu.CreateBufferRO(cc.c.eigenCount * cc.c.columns, 4, 1);
            gpu.WriteMarix(eigenTable, eVectors);

            TsneDx.SafeDispose(resultBuf);
            resultBuf = gpu.CreateBufferRW(cc.c.rows * cc.c.eigenCount, 4, 1);

            using (var shader = gpu.LoadShader("TsneDx.PcaReduceMatrix.cso")) {
                try {
                    gpu.SetShader(shader);
                    const int GROUP_NR = 256;
                    gpu.Run(GROUP_NR);

                    float[] buf = gpu.ReadRange<float>(resultBuf, cc.c.rows * cc.c.eigenCount);
                    B = new float[cc.c.rows][];
                    for (int row = 0; row < cc.c.rows; row++)
                        B[row] = new float[cc.c.eigenCount];
                    Parallel.For(0, cc.c.rows, row => {
                        Array.Copy(buf, row * cc.c.eigenCount, B[row], 0, cc.c.eigenCount);
                    });
                } catch (SharpDX.SharpDXException ex) {
                    string msg = ex.Message;
                    Console.WriteLine("GPU operation timeouted: Please try to enlarge the TDR value");
                }
            }

            TsneDx.SafeDispose(eigenTable, sdInit, sdStep, sdNorm, sdAdjCov, eVectorBuf, 
                eVectorStaging, eVector2Buf, resultBuf, resultStaging, covBuf, tableBuf, cc, gpu);
            return B;
        }

        public float[] DoPcaNumpyFile(string fileName, int eigenCount) {
            return TsneMap.Flatten(DoPca(TsneMap.ReadNumpyFile(fileName), eigenCount));
        }

        public float[] DoPcaBuffer(long ptr, int rows, int columns, int eigenCount) {
            var matrix = TsneMap.NumpyArrayToMatrix(ptr, rows, columns);
            return TsneMap.Flatten(DoPca(matrix, eigenCount));
        }

        public float[] DoPcaNumpy(float[][] X, int eigenCount) {
            return TsneMap.Flatten(DoPca(X, eigenCount));
        }
    }
}
