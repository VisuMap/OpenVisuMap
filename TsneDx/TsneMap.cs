// Copyright (C) 2020 VisuMap Technologies Inc.
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Buffer = SharpDX.Direct3D11.Buffer;
using ComputeShader = SharpDX.Direct3D11.ComputeShader;
using System.IO;
using SysBuffer = System.Buffer;
using System.Threading.Tasks;
using System.Linq;

namespace TsneDx {
    using ConstBuffer = GpuDevice.ConstBuffer<TsneMapConstants>;

    [StructLayout(LayoutKind.Explicit)]
    struct TsneMapConstants { 
        [FieldOffset(0)] public float targetH;  // The target entropy for all data points.
        [FieldOffset(4)] public int outDim;    // The output dimension.
        [FieldOffset(8)] public float PFactor;  // The exaggeration factor for the P matrix 
        [FieldOffset(12)] public float mom;     // The momentum.
        [FieldOffset(16)] public bool chacedP;  // Is the matrix P chached?
        [FieldOffset(20)] public int blockIdx;  // Help variable to split the calculation of P in multiple batches.
        [FieldOffset(24)] public int cmd;       // optional shader command flag.
        [FieldOffset(28)] public int groupNumber; // optional parameter for the number of dispatched thread groups.
        [FieldOffset(32)] public int columns;   // columns of input data table.
        [FieldOffset(36)] public int N;         // Rows of the input data table.
        [FieldOffset(40)] public int metricType;  // metric type: 0: Euclidean; 1: Correlation.
    };

    [ComVisible(true)]
    public class TsneMap : IDisposable {
        double momentum = 0.5;
        double finalMomentum = 0.8;
        double currentVariation = 0;
        double stopVariation = 0;
        double momentumSwitch = 0.33;

        const int GpuGroupSize = 1024;   // the GPU thread group size, must match GROUP_SIZE defined in TsneMap.hlsl.
        const int GroupSize = 128;       // Must match GROUP_SZ in TsneMap.hlsl; Used only for IteratOneStep()
        const int MaxGroupNumber = 128;

        GpuDevice gpu;
        Buffer PBuf;
        ConstBuffer cc;
        Buffer groupMaxBuf;
        Buffer resultBuf;
        Buffer resultStaging;
        Buffer tableBuf;
        Buffer distanceBuf;

        void CmdSynchronize() { gpu.ReadFloat(resultStaging, resultBuf); }

        public TsneMap(
            double PerplexityRatio = 0.05, 
            int MaxEpochs = 500, 
            int OutDim=2,
            double ExaggerationRatio = 0.2,
            int CacheLimit = 23000,
            double ExaggerationFactor = 4.0,
            int MetricType = 0
            )  {
            this.PerplexityRatio = PerplexityRatio;
            this.MaxEpochs = MaxEpochs;
            this.OutDim = OutDim;
            this.ExaggerationRatio = ExaggerationRatio;
            this.CacheLimit = CacheLimit;
            this.ExaggerationFactor = ExaggerationFactor;
            this.MetricType = MetricType;
        }

        public void Dispose() {
        }

        #region Properties
        public static string ErrorMsg { get; set; } = "";

        public int CacheLimit { get; set; } = 23000;

        public double PerplexityRatio { get; set; } = 0.05;

        public int OutDim { get; set; } = 2;

        public int MaxEpochs { get; set; } = 500;

        public int MetricType { get; set; } = 0;

        public double ExaggerationFactor { get; set; } = 4.0;

        public double ExaggerationRatio { get; set; } = 0.2;

        public int ExaggerationLength { 
            get { return (int) (MaxEpochs* ExaggerationRatio); }
        }
        #endregion        

        #region FitNumpy
        public static float[] Flatten(float[][] Y) {
            int columns = Y[0].Length;
            float[] Y1 = new float[Y.Length * columns];
            for (int row = 0; row < Y.Length; row++)
                Array.Copy(Y[row], 0, Y1, row * columns, columns);
            return Y1;
        }

        public float[] FitNumpyFile(string fileName) {
            return Flatten( Fit(ReadNumpyFile(fileName)) );
        }

        public float[] FitNumpy(float[][] X) {
            return Flatten( Fit(X) );
        }

        enum NumpyDtype {
            DT_Float32, DT_Int32, DT_Float64, DT_Unknown
        }

        public static float[][] ReadNumpyFile(string fileName) {        

        float[] ReadRow(int columns, BinaryReader br, NumpyDtype dtype) {
                float[] R = new float[columns];
                switch (dtype) {
                    case NumpyDtype.DT_Float32:
                        SysBuffer.BlockCopy(br.ReadBytes(R.Length * 4), 0, R, 0, R.Length * 4);
                        break;
                    case NumpyDtype.DT_Float64:
                        double[] buf = new double[R.Length];
                        SysBuffer.BlockCopy(br.ReadBytes(R.Length * 8), 0, buf, 0, R.Length * 8);
                        for (int i = 0; i < R.Length; i++) R[i] = (float) buf[i];
                        break;
                    default:
                        int[] buff = new int[R.Length];
                        SysBuffer.BlockCopy(br.ReadBytes(R.Length * 4), 0, buff, 0, R.Length * 4);
                        for (int i = 0; i < R.Length; i++) R[i] = buff[i];
                        break;
                }
                return R;
            }

            using (FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(f)) {
                byte[] magic = br.ReadBytes(6);
                if (magic[0] != 0x93) {
                    TsneMap.ErrorMsg = "No numpy file";
                    return null;            
                }
                byte[] version = br.ReadBytes(2);
                int headLen = 0;
                if (version[0] == 1) {
                    headLen = br.ReadInt16();
                } else {
                    headLen = br.ReadInt32();
                }
                byte[] bHead = br.ReadBytes(headLen);
                string sHead = Encoding.UTF8.GetString(bHead);
                sHead = sHead.Trim(new char[] { '{', ',', '}' });
                string[] fs = sHead.Split(new char[] { ' ', ',', ':', '\'', ')', '(' }, StringSplitOptions.RemoveEmptyEntries);
                NumpyDtype dtype = NumpyDtype.DT_Unknown;

                bool isFortranOrder = false;
                List<int> dims = new List<int>();
                for (int i = 0; i < fs.Length; i += 2) {
                    switch (fs[i]) {
                        case "descr":
                            switch (fs[i + 1]) {
                                case "<f4":
                                    dtype = NumpyDtype.DT_Float32;
                                    break;
                                case "<f8":
                                    dtype = NumpyDtype.DT_Float64;
                                    break;
                                case "<i4":
                                    dtype = NumpyDtype.DT_Int32;
                                    break;
                                default:
                                    dtype = NumpyDtype.DT_Unknown;
                                    break;
                            }
                            break;
                        case "fortran_order":
                            isFortranOrder = (fs[i + 1] == "True");
                            break;
                        case "shape":
                            dims.Add(int.Parse(fs[i + 1]));
                            for (int j = i + 2; j < fs.Length; j++)
                                if (char.IsDigit(fs[j][0]))
                                    dims.Add(int.Parse(fs[j]));
                            break;
                    }
                }

                if ((dtype == NumpyDtype.DT_Unknown) || (dims.Count != 2) || isFortranOrder ) {
                    TsneMap.ErrorMsg = "Invalid Format";
                    return null;
                }

                float[][] X = new float[dims[0]][];
                for (int row = 0; row < dims[0]; row++)
                    X[row] = ReadRow(dims[1], br, dtype);
                return X;
            }
        }
        #endregion

        static void NormalizeTable(float[][] matrix) {
            int rows = matrix.Length;
            int columns = matrix[0].Length;
            Parallel.For(0, rows, row => {
                float[] R = matrix[row];
                float av = R.Average();
                double sum = 0.0;
                for (int col = 0; col < columns; col++) {
                    R[col] -= av;
                    sum += R[col] * R[col];
                }
                double norm = Math.Sqrt(sum);
                if (norm != 0) {
                    for (int col = 0; col < columns; col++)
                        R[col] = (float)(R[col]/norm);
                }
            });
        }

        void CalculateP() {
            int N = cc.c.N;
            distanceBuf = gpu.CreateBufferRW((N * N - N) / 2, 4, 0);
            using (var shader = gpu.LoadShader("TsneDx.CreateDistanceCache.cso")) {
                gpu.SetShader(shader);
                int groupNr = 256;
                for (int i = 0; i < N; i += groupNr) {
                    cc.c.blockIdx = i;
                    cc.Upload();
                    gpu.Run(groupNr);
                }
            }

            PBuf = gpu.CreateBufferRW(N * N, 4, 1);
            cc.c.chacedP = true;
            using (var sd = gpu.LoadShader("TsneDx.CalculateP.cso")) {
                // Calculate the squared distance matrix in to P
                using (var sd2 = gpu.LoadShader("TsneDx.CalculatePFromCache.cso")) {
                    gpu.SetShader(sd2);
                    gpu.Run(64);
                }
                gpu.SetShader(sd);

                // Normalize and symmetrizing the distance matrix
                cc.c.cmd = 4;
                cc.Upload();
                gpu.Run();

                // Convert the matrix to affinities.
                cc.c.cmd = 2;
                cc.c.groupNumber = 4;
                for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber * GpuGroupSize) {
                    cc.c.blockIdx = bIdx;
                    cc.Upload();
                    gpu.Run(cc.c.groupNumber);
                }

                // Normalize and symmetrizing the affinity matrix
                gpu.SetShader(sd);
                cc.c.cmd = 3;
                cc.Upload();
                gpu.Run();
            }
        }

        void InitializeP() {
            int N = cc.c.N;
            const int INIT_THREAD_GROUPS = 256;
            PBuf = gpu.CreateBufferRW((2 + INIT_THREAD_GROUPS) * N, 4, 1); // for betaList[] and affinityFactor[]
            cc.c.chacedP = false;

            using (var sd = gpu.LoadShader("TsneDx.InitializeP.cso"))
            using (var sd3 = gpu.LoadShader("TsneDx.InitializeP3.cso")) {
                // Calcualtes the distanceFactor[].
                gpu.SetShader(sd3);
                cc.c.cmd = 1;
                cc.c.groupNumber = 64;
                for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber) {
                    cc.c.blockIdx = bIdx;
                    cc.Upload();
                    gpu.Run(cc.c.groupNumber);
                }

                cc.c.cmd = 2;
                // cc.c.groupNumber, groupMax[0..groupNumber] are the maximal values of matrix sub-section.
                cc.Upload();
                gpu.Run();
                CmdSynchronize();

                // Calculates the betaList[].
                gpu.SetShader(sd3);
                cc.c.cmd = 3;
                cc.c.groupNumber = INIT_THREAD_GROUPS;
                for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber) {
                    cc.c.blockIdx = bIdx;
                    cc.Upload();
                    gpu.Run(cc.c.groupNumber);
                    CmdSynchronize();
                }
                CmdSynchronize();

                // calculates the affinityFactor[]
                gpu.SetShader(sd3);
                cc.c.cmd = 4;
                cc.c.groupNumber = 128;  // groupNumber must be smaller than GroupSize as required by next command.
                for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber) {
                    cc.c.blockIdx = bIdx;
                    cc.Upload();
                    gpu.Run(cc.c.groupNumber);
                    CmdSynchronize();
                }

                gpu.SetShader(sd);
                // cc.c.groupNumber must contains the number of partial sumes in groupMax[].
                cc.Upload();
                gpu.Run();
                CmdSynchronize();
            }
        }

        public float[][] Fit(float[][] X) {
            int exaggerationLength = (int)(MaxEpochs * ExaggerationRatio);

            gpu = new GpuDevice();
            cc = gpu.CreateConstantBuffer<TsneMapConstants>(0);

            int N = X.Length;
            cc.c.columns = X[0].Length;
            cc.c.N = N;
            cc.c.outDim = OutDim;
            cc.c.metricType = MetricType;

            #region Initialize Y
            Buffer Y2Buf = null;
            Buffer Y3Buf = null;
            Buffer Y3StagingBuf = null;
            Buffer Y2StagingBuf = null;
            Buffer v2Buf = null;
            Buffer v3Buf = null;

            if (cc.c.outDim == 2) {
                Y2Buf = gpu.CreateBufferRW(N, 8, 3);
                Y2StagingBuf = gpu.CreateStagingBuffer(Y2Buf);
                v2Buf = gpu.CreateBufferRW(N, 2 * 8, 5);
            } else {
                Y3Buf = gpu.CreateBufferRW(N, 12, 4);
                Y3StagingBuf = gpu.CreateStagingBuffer(Y3Buf);
                v3Buf = gpu.CreateBufferRW(N, 2 * 12, 6);
            }

            float rang = 0.05f;
            Random rGenerator = new Random(435243);

            if (cc.c.outDim == 2) {
                using (var ws = gpu.NewWriteStream(v2Buf)) {
                    for (int row = 0; row < N; row++)
                        ws.Write<float>(0, 1, 0, 1);
                }
                using (var ws = gpu.NewWriteStream(Y2Buf)) {
                    for (int row = 0; row < N; row++)
                        for (int col = 0; col < cc.c.outDim; col++)
                            ws.Write((float)(rang * rGenerator.NextDouble() - rang / 2));
                }
            } else {
                using (var ws = gpu.NewWriteStream(v3Buf)) {
                    for (int row = 0; row < N; row++)
                        ws.Write<float>(0, 1, 0, 1, 0, 1);
                }
                using (var ws = gpu.NewWriteStream(Y3Buf)) {
                    for (int row = 0; row < N; row++)
                        for (int col = 0; col < cc.c.outDim; col++)
                            ws.Write((float)(rang * rGenerator.NextDouble() - rang / 2));
                }
            }
            #endregion

            #region Upload data table and initialize the distance matrix

            // Used to aggregate values created by parallel threads.
            // the size of of groupMaxBuf must be large enoght to hold a float value for each thread started in parallel.
            // Notice: gpu.Run(k) will start k*GROUP_SIZE threads.
            groupMaxBuf = gpu.CreateBufferRW(Math.Max(GpuGroupSize, MaxGroupNumber * GroupSize), 4, 7);

            resultBuf = gpu.CreateBufferRW(3, 4, 2);  // to receive the total changes.
            resultStaging = gpu.CreateStagingBuffer(resultBuf);            

            tableBuf = gpu.CreateBufferRO(N * cc.c.columns, 4, 0);
            if (MetricType == 1)
                NormalizeTable(X);
            gpu.WriteMarix(tableBuf, X, true);

            bool CachingDistance() { return (N <= CacheLimit); }
            #endregion

            #region Calculate or Initialize P.
            
            cc.c.targetH = (float)Math.Log(PerplexityRatio * N);
            if (CachingDistance()) { // CalculateP()
                CalculateP();
            } else { // InitializeP()
                InitializeP();
            }
            #endregion

            #region Calculate the initial sume of matrix Q.
            using (var sd = gpu.LoadShader("TsneDx.CalculateSumQ.cso")) {
                gpu.SetShader(sd);
                cc.c.groupNumber = 256;
                for (int i = 0; i < N; i += cc.c.groupNumber) {
                    cc.c.blockIdx = i;
                    cc.Upload();
                    gpu.Run(cc.c.groupNumber);
                }
                cc.c.blockIdx = -1;
                cc.Upload();
                gpu.Run();
            }
            CmdSynchronize();
            #endregion

            #region The training loop
            bool fastEuclidean = false;
            const int MaxDimension = 64; // maximal dimension (table columns) for fast EuclideanNoCache shader. Must be the same as MAX_DIMENSION.
            const int MaxDimensionS = 32; // maximal dimension (table columns) for fast EuclideanNoCache shader. Must be the same as MAX_DIMENSIONs.
            const int GrSize = 64;  // This value must match that of GR_SIZE in TsneMap.hlsl.

            ComputeShader csOneStep = null;
            if (CachingDistance()) {
                csOneStep = gpu.LoadShader("TsneDx.OneStep.cso");
            } else {
                if (cc.c.columns <= MaxDimension) {
                    // Using fast implementation by caching data in the group-shared memory.
                    string sdName = (cc.c.columns <= MaxDimensionS) ? "TsneDx.FastStepS.cso" : "TsneDx.FastStep.cso";
                    csOneStep = gpu.LoadShader(sdName);
                    fastEuclidean = true;
                } else {
                    csOneStep = gpu.LoadShader("TsneDx.OneStepNoCache.cso");
                }
            }
            ComputeShader csSumUp = gpu.LoadShader("TsneDx.OneStepSumUp.cso");
            int stepCounter = 0;

            while (true) {
                cc.c.PFactor = (stepCounter < exaggerationLength) ? (float)ExaggerationFactor : 1.0f;
                gpu.SetShader(csOneStep);

                if (CachingDistance()) {
                    cc.c.groupNumber = MaxGroupNumber;
                    // Notice: cc.c.groupNumber*GroupSize must fit into groupMax[].
                    for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber * GroupSize) {
                        cc.c.blockIdx = bIdx;
                        cc.Upload();
                        gpu.Run(cc.c.groupNumber);
                    }
                    cc.c.groupNumber = MaxGroupNumber * GroupSize;
                } else if (fastEuclidean) {
                    cc.c.groupNumber = MaxGroupNumber;
                    for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber * GrSize) {
                        cc.c.blockIdx = bIdx;
                        cc.Upload();
                        gpu.Run(cc.c.groupNumber);
                    }
                    cc.c.groupNumber = cc.c.groupNumber * GrSize;
                } else {
                    // using the IterateOneStep shader. A whole thread group will be assigned
                    // to calculate a row of PP(i,j) and Q(i,j).
                    cc.c.groupNumber = 128;
                    for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber) {
                        cc.c.blockIdx = bIdx;
                        cc.Upload();
                        gpu.Run(cc.c.groupNumber);
                    }
                    CmdSynchronize();
                }

                //Notice: cc.c.groupNumber must be number of partial sumQ_next, which add up to sumQ for the next step.
                gpu.SetShader(csSumUp);
                cc.Upload();
                gpu.Run();

                currentVariation = gpu.ReadRange<float>(resultStaging, resultBuf, 3)[2]/N;

                cc.c.mom = (float)((stepCounter < (MaxEpochs * momentumSwitch)) ? momentum : finalMomentum);
                stepCounter++;
                if (stepCounter % 10 == 0)  Console.Write('.');
                if (stepCounter % 500 == 0) Console.WriteLine();
                if ((stepCounter >= MaxEpochs) || ((stepCounter >= (2 + exaggerationLength)) && (currentVariation < stopVariation))) {
                    break;
                }
            }
            Console.WriteLine();
            #endregion

            float[][] Y = new float[N][];
            using (var rs = gpu.NewReadStream((cc.c.outDim == 3) ? Y3StagingBuf : Y2StagingBuf, (cc.c.outDim == 3) ? Y3Buf : Y2Buf)) {
                for (int row = 0; row < N; row++) {
                    Y[row] = rs.ReadRange<float>(cc.c.outDim);
                }
            }

            TsneDx.SafeDispose(csSumUp, csOneStep, PBuf, distanceBuf, tableBuf, resultBuf, 
                resultStaging, groupMaxBuf, Y3Buf, Y3StagingBuf, v3Buf, Y2Buf, Y2StagingBuf, v2Buf, cc, gpu);

            return Y;
        }
    }
}
