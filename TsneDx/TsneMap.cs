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
using MT = System.Threading.Tasks.Parallel;

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
        const int GroupSizeHyp = 32;       // Must match GROUP_SZ_HYP in TsneMap.hlsl; Used only for OneStepCpuCache()
        const int MaxGroupNumberHyp = 32;

        GpuDevice gpu;
        Buffer PBuf;
        Buffer P2Buf;
        ConstBuffer cc;
        Buffer groupMaxBuf;
        Buffer resultBuf;
        Buffer resultStaging;
        Buffer tableBuf;
        Buffer distanceBuf;

        float[][] cpuP;

        public TsneMap(
            double PerplexityRatio = 0.05, 
            int MaxEpochs = 500, 
            int OutDim=2,
            double ExaggerationRatio = 0.7,
            int CacheLimit = 23000,
            double ExaggerationFactor = 12.0,
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

        public int MaxCpuCacheSize { get; set; } = 26000;

        public double PerplexityRatio { get; set; } = 0.15;

        public int OutDim { get; set; } = 2;

        public int MaxEpochs { get; set; } = 500;

        public int MetricType { get; set; } = 0;

        public double ExaggerationFactor { get; set; } = 4.0;

        public double ExaggerationRatio { get; set; } = 0.99;

        public int ExaggerationLength { 
            get { return (int) (MaxEpochs* ExaggerationRatio); }
        }

        public bool ExaggerationSmoothen { get; set; } = true;

        public bool AutoNormalize { get; set; } = true;

        public bool StagedTraining { get; set; } = true;
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

        unsafe static public float[][] NumpyArrayToMatrix(long ptr, int rows, int columns) {
            float[][] matrix = new float[rows][];
            long L = 4 * columns;
            for (int row = 0; row < rows; row++) {
                matrix[row] = new float[columns];
                fixed (void* src = &(matrix[row][0]))
                    System.Buffer.MemoryCopy((void*)(ptr + row * L), src, L, L);
            }
            return matrix;
        }

        unsafe public float[] FitBuffer(long ptr, int rows, int columns) {
            float[][] matrix = NumpyArrayToMatrix(ptr, rows, columns);
            return Flatten(Fit(matrix));
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

                if ((dtype == NumpyDtype.DT_Unknown) || (dims.Count != 2) ) {
                    TsneMap.ErrorMsg = "Invalid Format";
                    return null;
                }

                float[][] X = new float[dims[0]][];
                if (isFortranOrder) {
                    float[][] Xtr = new float[dims[1]][];
                    for (int row = 0; row < dims[1]; row++)
                        Xtr[row] = ReadRow(dims[0], br, dtype);
                    for (int row = 0; row < dims[0]; row++) {
                        X[row] = new float[dims[1]];
                        for (int col = 0; col < dims[1]; col++)
                            X[row][col] = Xtr[col][row];
                    }
                } else {
                    for (int row = 0; row < dims[0]; row++)
                        X[row] = ReadRow(dims[1], br, dtype);
                }
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

                // Calculates the betaList[].
                gpu.SetShader(sd3);
                cc.c.cmd = 3;
                cc.c.groupNumber = INIT_THREAD_GROUPS;
                for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber) {
                    cc.c.blockIdx = bIdx;
                    cc.Upload();
                    gpu.Run(cc.c.groupNumber);
                }

                // calculates the affinityFactor[]
                gpu.SetShader(sd3);
                cc.c.cmd = 4;
                cc.c.groupNumber = 128;  // groupNumber must be smaller than GroupSize as required by next command.
                for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber) {
                    cc.c.blockIdx = bIdx;
                    cc.Upload();
                    gpu.Run(cc.c.groupNumber);
                }

                gpu.SetShader(sd);
                // cc.c.groupNumber must contains the number of partial sumes in groupMax[].
                cc.Upload();
                gpu.Run();
            }
        }

        void InitializePCpu() {
            int N = cc.c.N;
            const float DistanceScale = 100.0f;
            const float eps = 2.22e-16f;
            int bandSize = Math.Min(N, MaxGroupNumberHyp * GroupSizeHyp);
            PBuf = gpu.CreateBufferRW(bandSize * N, 4, 1);
            P2Buf = gpu.CreateBufferDynamic(bandSize * N, 4, 7); // dynamic buffer for fast uploading. Linked to Pcpu[] on HLSL.
            int blockSize = 128; // Calculate so many rows per dispatch.
            cpuP = new float[N][];
            for (int i = 0; i < N; i++) cpuP[i] = new float[N];

            using (var distanceBuf = gpu.CreateBufferRW(blockSize * N, 4, 0))
            using (var stagingBuf = gpu.CreateStagingBuffer(distanceBuf))
            using (var sd = gpu.LoadShader("TsneDx.PartialDistance2.cso")) {
                gpu.SetShader(sd);
                for (int iBlock = 0; iBlock < N; iBlock += blockSize) {
                    cc.c.blockIdx = iBlock;
                    cc.Upload();
                    gpu.Run(blockSize);

                    int iBlock2 = Math.Min(iBlock + blockSize, N);
                    int blockLen = (iBlock2 * (iBlock2 - 1) - iBlock * (iBlock - 1)) / 2;
                    float[] ret = gpu.ReadRange<float>(stagingBuf, distanceBuf, blockLen);
                    int idx = 0;
                    for (int row = iBlock; row < iBlock2; row++) {
                        Array.Copy(ret, idx, cpuP[row], 0, row);
                        idx += row;
                    }
                }
            }

            double distanceFactor = double.MinValue;
            MT.For(1, N, i => {
                float maxV = cpuP[i].Max();
                lock (this)
                    distanceFactor = Math.Max(distanceFactor, maxV);
            });

            if (distanceFactor == 0)
                throw new System.Exception("Distance metric degenerated: all components are zero.");

            // Scale the distance to managable range [0, 100.0] to avoid degredation 
            // with exp function.
            distanceFactor = DistanceScale / distanceFactor;
            MT.For(1, N, i => {
                for (int j = 0; j < i; j++)
                    cpuP[i][j] = (float)(cpuP[i][j] * distanceFactor);
            });
        
            MT.For(0, N, i => {
                for (int j = 0; j<i; j++)
                    cpuP[j][i] = cpuP[i][j];
                cpuP[i][i] = 0;
            });

            int bSize = MaxGroupNumberHyp * GroupSizeHyp;
            using (var sd = gpu.LoadShader("TsneDx.Dist2Affinity.cso"))
            using (var stagingBuf = gpu.CreateStagingBuffer(PBuf)) {
                gpu.SetShader(sd);
                for (int iBlock = 0; iBlock < N; iBlock += bSize) {
                    cc.c.blockIdx = iBlock;
                    cc.Upload();
                    int iBlock2 = Math.Min(N, iBlock + bSize);
                    using (var ws = gpu.NewWriteStream(PBuf))
                        for (int row = iBlock; row < iBlock2; row++)
                            ws.WriteRange(cpuP[row]);
                    gpu.Run(MaxGroupNumberHyp);
                    using (var rs = gpu.NewReadStream(stagingBuf, PBuf))
                        for (int row = iBlock; row < iBlock2; row++)
                            rs.ReadRange(cpuP[row], 0, N);
                }
            }

            double sum = 0;
            MT.For(0, N, i => {
                double sum2 = 0.0;
                for (int j = i + 1; j < N; j++) {
                    cpuP[i][j] += cpuP[j][i];
                    sum2 += cpuP[i][j];
                }
                lock (this)
                    sum += sum2;
            });

            if (sum == 0) 
                throw new System.Exception("Perplexity too small!");

            sum *= 2;
            MT.For(0, N, i => {
                for (int j = i + 1; j < N; j++) {
                    cpuP[i][j] = (float)Math.Max(cpuP[i][j] / sum, eps);
                    cpuP[j][i] = cpuP[i][j];
                }
                cpuP[i][i] = 1.0f;
            });
        }

        enum CachingMode {  // Caching mode for affinity matrix P.
            OnGpu,   // fully cached on GPU.
            OnFly,   // no-caching, calculated on-fly on GPU
            OnCpu,   // cached on CPU in cpuP[]
            OnFlySm, // Calculated on-fly with help of GPU shared memory.
            OnFlySmS,// Calculated on-fly with help of small set of GPU shared memory.
        }
        CachingMode cachingMode = CachingMode.OnGpu;

        void ReInitializeP(double perplexity) {
            cc.c.targetH = (float)Math.Log(perplexity);
            if (cachingMode == CachingMode.OnGpu) {
                CalculateP();
            } else if (cachingMode == CachingMode.OnCpu) {
                InitializePCpu();
            } else { // (cachingMode == CachingMode.OnFly[Sm,SmS])
                InitializeP();
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

            if (cc.c.outDim <= 2) {
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

            if (cc.c.outDim <= 2) {
                using (var ws = gpu.NewWriteStream(v2Buf)) {
                    for (int row = 0; row < N; row++)
                        ws.Write<float>(0, 1, 0, 1);
                }
                
                using (var ws = gpu.NewWriteStream(Y2Buf)) {
                    for (int row = 0; row < N; row++) {
                        for (int col = 0; col < cc.c.outDim; col++)
                            ws.Write((float)(rang * rGenerator.NextDouble() - rang / 2));
                        if (cc.c.outDim == 1)
                            ws.Write(0.0f);
                    }
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
            int gpSize = Math.Max(GpuGroupSize, MaxGroupNumber * GroupSize);
            gpSize = Math.Max(gpSize, MaxGroupNumberHyp * GroupSizeHyp);
            groupMaxBuf = gpu.CreateBufferRW(gpSize, 4, 7);

            resultBuf = gpu.CreateBufferRW(3, 4, 2);  // to receive the total changes.
            resultStaging = gpu.CreateStagingBuffer(resultBuf);            

            tableBuf = gpu.CreateBufferRO(N * cc.c.columns, 4, 0);
            if (MetricType == 1)
                NormalizeTable(X);
            gpu.WriteMarix(tableBuf, X, true);

            const int MinCpuDimension = 100; // minimal dimension to trigger CPU caching.
            const int MaxDimension = 64;     // maximal dimension (table columns) for fast EuclideanNoCache shader. Must be the same as MAX_DIMENSION.
            const int MaxDimensionS = 32;    // maximal dimension (table columns) for fast EuclideanNoCache shader. Must be the same as MAX_DIMENSIONs.
            if (N <= CacheLimit) {
                cachingMode = CachingMode.OnGpu;
            } else {
                if( (cc.c.columns > MinCpuDimension) && ((double) N * N * 4) < ((double)MaxCpuCacheSize * 1024.0 * 1024.0)) {
                    cachingMode = CachingMode.OnCpu;
                } else {
                    if (cc.c.columns < MaxDimensionS)
                        cachingMode = CachingMode.OnFlySmS;
                    else if (cc.c.columns < MaxDimension)
                        cachingMode = CachingMode.OnFlySm;
                    else
                        cachingMode = CachingMode.OnFly;
                }
            }
            #endregion

            ReInitializeP(PerplexityRatio * N);

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
            
            var sdNames = new Dictionary<CachingMode, string>() {
                {CachingMode.OnGpu, "TsneDx.OneStep.cso"},
                {CachingMode.OnCpu,  "TsneDx.OneStepCpuCache.cso"},
                {CachingMode.OnFly,  "TsneDx.OneStepNoCache.cso"},
                {CachingMode.OnFlySm, "TsneDx.FastStep.cso" },
                {CachingMode.OnFlySmS, "TsneDx.FastStepS.cso"},
            };

            ComputeShader csOneStep = gpu.LoadShader(sdNames[cachingMode]);
            ComputeShader csSumUp = gpu.LoadShader("TsneDx.OneStepSumUp.cso");
            int stepCounter = 0;
            List<double> stages = StagedTraining ? new List<double>() { 0.5, 0.75, 0.9 } : new List<double>();

            while (true) {
                if (stepCounter < exaggerationLength) {
                    if (ExaggerationSmoothen) {
                        int len = (int)(0.9 * MaxEpochs);
                        if (stepCounter < len) {
                            double t = (double)stepCounter / len;
                            cc.c.PFactor = (float)((1 - t) * ExaggerationFactor + t);
                        } else
                            cc.c.PFactor = 1.0f;
                    } else
                        cc.c.PFactor = (float)ExaggerationFactor;
                } else
                    cc.c.PFactor = 1.0f;

                if (stages.Count > 0) {
                    double r = stages[0];
                    if (stepCounter == (int)(r * MaxEpochs)) {
                        double newPP = PerplexityRatio * N * (1 - r);
                        if (newPP > 20) {
                            ReInitializeP(newPP);
                            stages.RemoveAt(0);
                        }
                    }
                }

                gpu.SetShader(csOneStep);

                if (cachingMode == CachingMode.OnGpu) {
                    cc.c.groupNumber = MaxGroupNumber;
                    // Notice: cc.c.groupNumber*GroupSize must fit into groupMax[].
                    for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber * GroupSize) {
                        cc.c.blockIdx = bIdx;
                        cc.Upload();
                        gpu.Run(cc.c.groupNumber);
                    }
                    cc.c.groupNumber = MaxGroupNumber * GroupSize;
                } else if (cachingMode == CachingMode.OnCpu) {
                    int bSize = MaxGroupNumberHyp * GroupSizeHyp;
                    cc.c.groupNumber = MaxGroupNumberHyp;
                    for (int bIdx = 0; bIdx < N; bIdx += bSize) {
                        gpu.WriteArray(cpuP, bIdx, Math.Min(N, bIdx + bSize), P2Buf);
                        cc.c.blockIdx = bIdx;
                        cc.Upload();
                        gpu.Run(cc.c.groupNumber);
                    }
                    cc.c.groupNumber = Math.Min(N, bSize);
                } else if ( (cachingMode==CachingMode.OnFlySm) || (cachingMode == CachingMode.OnFlySmS) ) {
                    const int GrSize = 64;  // This value must match that of GR_SIZE in TsneMap.hlsl.
                    cc.c.groupNumber = MaxGroupNumber;
                    for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber * GrSize) {
                        cc.c.blockIdx = bIdx;
                        cc.Upload();
                        gpu.Run(cc.c.groupNumber);
                    }
                    cc.c.groupNumber = cc.c.groupNumber * GrSize;
                } else { // cachingMode==CachingMode.OnFly
                    cc.c.groupNumber = 128;
                    for (int bIdx = 0; bIdx < N; bIdx += cc.c.groupNumber) {
                        cc.c.blockIdx = bIdx;
                        cc.Upload();
                        gpu.Run(cc.c.groupNumber);
                    }
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

            float[][] Y = new float[N][];
            using (var rs = gpu.NewReadStream((cc.c.outDim == 3) ? Y3StagingBuf : Y2StagingBuf, (cc.c.outDim == 3) ? Y3Buf : Y2Buf)) {
                int outVDim = (cc.c.outDim == 3) ? 3 : 2;
                for (int row = 0; row < N; row++) {
                    Y[row] = rs.ReadRange<float>(outVDim);
                }
            }

            if ( cc.c.outDim == 1) 
                for (int i = 0; i < N; i++)
                    Y[i] = new float[] { Y[i][0] };

            TsneDx.SafeDispose(csSumUp, csOneStep, PBuf, P2Buf, distanceBuf, tableBuf, resultBuf, 
                resultStaging, groupMaxBuf, Y3Buf, Y3StagingBuf, v3Buf, Y2Buf, Y2StagingBuf, v2Buf, cc, gpu);

            return AutoNormalize ? PcaNormalize.DoNormalize(Y) : Y;
        }
    }
}
