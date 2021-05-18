using System;
using System.Xml;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using VisuMap.Script;

namespace VisuMap.SingleCell {
    using IMetric = VisuMap.Plugin.IMetric;
    using IDataset = VisuMap.Script.IDataset;
    using IFilterEditor = VisuMap.Plugin.IFilterEditor;
    using ComputeShader = SharpDX.Direct3D11.ComputeShader;
    using CBuf = DxShader.ConstBuffer<ShaderConstant>;
    using GBuf = SharpDX.Direct3D11.Buffer;

    [StructLayout(LayoutKind.Explicit)]
    struct ShaderConstant {
        [FieldOffset(0)] public int N;
        [FieldOffset(4)] public int columns;
        [FieldOffset(8)] public int iBlock;
    };

    public enum MetricMode { CorCor, EucEuc, CorEuc, EucCor };

    public class DualAffinity : IMetric {
        IDataset dataset;
        float[][] P, dtP, dtQ;
        int[] toIdx = null;  // map a dataset wide index into the range of [0, enabled_bodies-1].
        const int MaxGpuFloats = (1 << 29);    // maximal number of floats in a single GPU buffer.
        MetricMode metricMode = MetricMode.CorCor;

        public DualAffinity(MetricMode mtrMode) {
            this.metricMode = mtrMode;
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            if (!ValidateDataset(dataset))
                return;
            this.dataset = dataset;
            dtP = dtQ = null;
        }

        public void Initialize(float[][] table) {
            P = table;
            dataset = null;
            dtP = dtQ = null;
        }

        public double Distance(int i, int j){
            if (i == j) {
                if (i == 0) {
                    if (dtP == null)
                        PreCalculate();
                    else if ( (dataset != null) && (dataset.BodyList.Count(b=>!b.Disabled) != (dtP.Length+dtQ.Length)) )
                        PreCalculate();
                } else if (i == 1)
                    dtP = dtQ = null;
                return 0;
            }
            i = toIdx[i];
            j = toIdx[j];
            int N = P.Length;
            if ((i < N) && (j < N)) {
                return (j < i) ? dtP[i][j] : dtP[j][i];
            } else if ((i >= N) && (j >= N)) {
                i -= N;
                j -= N;
                return (j < i) ? dtQ[i][j] : dtQ[j][i];
            } else
                return (i < N) ? P[i][j - N] : P[j][i - N];
        }

        #region Calculate the dot product on GPU.
        public static float[][] DotProduct(DxShader.GpuDevice gpu, float[][] M, bool isCorrelation) {
            using (var cc = gpu.CreateConstantBuffer<ShaderConstant>(0))
            using (var sd = gpu.LoadShader("SingleCellAnalysis.DotProduct.cso", Assembly.GetExecutingAssembly())) {
                return CallShadere(gpu, cc, sd, M, isCorrelation);
            }
        }

        static float[][] CallShadere(DxShader.GpuDevice gpu, CBuf cc, ComputeShader sd, float[][] M, bool isCorrelation) {
            cc.c.N = M.Length;
            int columns = M[0].Length;
            const int groupSize = 256;
            int distSize = groupSize * cc.c.N;
            float[][] dMatrix = new float[M.Length][];
            for (int row = 0; row < M.Length; row++)
                dMatrix[row] = new float[row];
            int maxColumns = MaxGpuFloats / cc.c.N;
            int secSize = Math.Min(columns, (maxColumns > 4096) ? 4096 : (maxColumns - 32));
            float[] uploadBuf = new float[cc.c.N * secSize];

            using (var dataBuf = gpu.CreateBufferRO(cc.c.N * secSize, 4, 0))
            using (var distBuf = gpu.CreateBufferRW(distSize, 4, 0))
            using (var distStaging = gpu.CreateStagingBuffer(distBuf)) {
                gpu.SetShader(sd);
                for (int s0 = 0; s0 < columns; s0 += secSize) {
                    int s1 = Math.Min(s0 + secSize, columns);
                    WriteMarix(gpu, dataBuf, M, s0, s1, uploadBuf);
                    float[] blockDist = new float[distSize];
                    for (cc.c.iBlock = 1; cc.c.iBlock < cc.c.N; cc.c.iBlock += groupSize) {
                        cc.c.columns = s1 - s0;
                        cc.Upload();
                        gpu.Run(groupSize);
                        int iBlock2 = Math.Min(cc.c.iBlock + groupSize, cc.c.N);
                        int bSize = (iBlock2 - cc.c.iBlock) * (iBlock2 + cc.c.iBlock - 1) / 2;
                        gpu.ReadRange<float>(blockDist, 0, distStaging, distBuf, bSize);

                        int offset = 0;
                        for (int row = cc.c.iBlock; row < iBlock2; row++) {
                            float[] R = dMatrix[row];
                            for (int k = 0; k < row; k++)
                                R[k] += blockDist[offset + k];
                            offset += row;
                        }
                        Application.DoEvents();
                    }
                }
            }

            if (isCorrelation) {
                MT.ForEach(dMatrix, R => {
                    for (int col = 0; col < R.Length; col++)
                        R[col] = 1.0f - R[col];
                });
            } else { // Euclidean distance is wanted.
                float[] norm2 = new float[M.Length];
                Array.Clear(norm2, 0, M.Length);
                int L = M[0].Length;
                MT.ForEach(M, (R, row) => {
                    float sumSquared = 0.0f;
                    for (int col = 0; col < L; col++)
                        sumSquared += R[col] * R[col];
                    norm2[row] = sumSquared;
                });
                MT.Loop(1, M.Length, row => {
                    float[] R = dMatrix[row];
                    for (int col = 0; col < row; col++) {
                        R[col] = (float)Math.Sqrt(Math.Abs(norm2[row] + norm2[col] - 2 * R[col]));
                    }
                });
            }
            return dMatrix;
        }

        static void WriteMarix(DxShader.GpuDevice gpu, GBuf buffer, float[][] matrix, int col0, int col1, float[] uploadBuf) {
            int rows = matrix.Length;
            Array.Clear(uploadBuf, 0, uploadBuf.Length);
            MT.Loop(col0, col1, col => {
                int offset = (col - col0) * rows;
                for (int row = 0; row < rows; row++)
                    uploadBuf[offset + row] = matrix[row][col];
            });
            gpu.WriteArray(uploadBuf, buffer);
        }
        #endregion

        void PreCalculate() {
            if (dataset != null) {
                var bs = dataset.BodyList;
                INumberTable nt = dataset.GetNumberTableView();
                int nrows = nt.Rows - nt.Columns;
                toIdx = Enumerable.Range(0, bs.Count).Where(i => !bs[i].Disabled).ToArray();    // Indexes of enabled bodies.
                int[] selectedRows = toIdx.Where(i => i < nrows).ToArray();
                int[] selectedColumns = toIdx.Where(i => i >= nrows).Select(i=>i-nrows).ToArray();

                if ( (selectedColumns.Length==0) || (selectedRows.Length==0) ) {
                    throw new TException("No-zero number of genes and cells must be selected!");
                }
                P = new float[selectedRows.Length][];
                MT.Loop(0, P.Length, row => {
                    float[] R = P[row] = new float[selectedColumns.Length];
                    double[] dsR = nt.Matrix[selectedRows[row]] as double[];
                    for (int col = 0; col < selectedColumns.Length; col++)
                        R[col] = (float)dsR[selectedColumns[col]];
                });

                // Reverse toIdx;
                int[] rIdx = Enumerable.Repeat(-1, bs.Count).ToArray();
                for(int i=0; i<toIdx.Length; i++) rIdx[toIdx[i]] = i;
                toIdx = rIdx;
            }
            float[][] Q = Transpose(P);   // Q is the transpose of P.

            // Calculate the distance tables for P and Q.
            bool isPCor = (metricMode == MetricMode.CorCor) || (metricMode == MetricMode.CorEuc);
            bool isQCor = (metricMode == MetricMode.CorCor) || (metricMode == MetricMode.EucCor);
            if ( isPCor )
                Normalize(P);
            if ( isQCor )
                Normalize(Q);

            using (var gpu = new VisuMap.DxShader.GpuDevice())
            using (var cc = gpu.CreateConstantBuffer<ShaderConstant>(0))
            using (var sd = gpu.LoadShader("SingleCellAnalysis.DotProduct.cso", Assembly.GetExecutingAssembly())) {
                dtP = CallShadere(gpu, cc, sd, P, isPCor);
                dtQ = CallShadere(gpu, cc, sd, Q, isQCor);
            }

            /*
             * affinity-propagation enhances structure if colums or rows within clusters are
             * light occupied with high randomness. AP however dilutes clusters with few members.
             * For instance, singlton gene cluster (with only gene) will suppressed by AP due to
             * its aggregation with neighboring genes (which should be consdiered as separate
             * clusters.)
             * 
            ProbagateAffinity(dtQ, P); // Column<->Column affinity => Column->Row affinity
            ProbagateAffinity(dtP, Q);  // Row<->Row affinity => Row->Column affinity.
            */

            // Calculates the distance between rows and columns into P.
            P = AffinityToDistance(P, Q);
            Q = null;
            var app = SingleCellPlugin.App.ScriptApp;
            double linkBias = app.GetPropertyAsDouble("SingleCell.Separation", 1.0);
            double pScale = app.GetPropertyAsDouble("SingleCell.CellScale", 1.0);
            double qScale = app.GetPropertyAsDouble("SingleCell.GeneScale", 1.0);
            //PowerMatrix(P, linkBias);

            // Scaling dtP, dtQ to the range of P.
            Func<float[][], float> aFct = AverageSqrt;
            double av = aFct(P);
            pScale *= av / aFct(dtP);
            qScale *= av / aFct(dtQ);
            ScaleMatrix(dtP, pScale);
            ScaleMatrix(dtQ, qScale);

            //ScaleMatrix(P, linkBias);
            ShiftMatrix(P, (float)(linkBias * av));
        }

        public static float Average(float[][] matrix) {
            float sum = 0.0f;
            MT.ForEach(matrix, R => {
                float s = 0;
                for (int i = 0; i < R.Length; i++)
                    s += R[i];
                lock (SingleCellPlugin.App)
                    sum += s;
            });
            return (float)(sum / matrix.Sum(R => (double)R.Length));
        }

        public static float Average2(float[][] matrix) {
            float sum = 0.0f;
            MT.ForEach(matrix, R => {
                float s = 0;
                for (int i = 0; i < R.Length; i++)
                    s += R[i] * R[i];
                lock (SingleCellPlugin.App)
                    sum += s;
            });
            return (float)Math.Sqrt(sum / matrix.Sum(R => (double)R.Length));
        }

        public static float AverageSqrt(float[][] matrix) {
            double sum = 0.0f;
            MT.ForEach(matrix, R => {
                double s = 0;
                for (int i = 0; i < R.Length; i++)
                    s += Math.Sqrt(Math.Abs(R[i]));
                lock (SingleCellPlugin.App)
                    sum += s;
            });
            double mean = sum / matrix.Sum(R => (double)R.Length);
            return (float)(mean*mean);
        }

        public static void ScaleMatrix(float[][] matrix, double fct) {
            if (fct == 1.0) return;
            MT.ForEach(matrix, R => {
                for (int j = 0; j < R.Length; j++)
                    R[j] = (float)(R[j] * fct);
            });
        }

        public static Tuple<float, float, float> MinMaxMean(float[][] matrix) {
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;
            float meanValue = 0;
            int cnt = 0;
            MT.ForEach(matrix, R => {
                float minV = float.MaxValue;
                float maxV = float.MinValue;
                float meanV = 0;
                for (int j = 0; j < R.Length; j++) {
                    float v = R[j];
                    minV = Math.Min(minV, v);
                    maxV = Math.Max(maxV, v);
                    meanV += v;
                }
                lock (matrix) {
                    minValue = Math.Min(minValue, minV);
                    maxValue = Math.Max(maxValue, maxV);
                    meanValue += meanV;
                    cnt += R.Length;
                }
            });
            meanValue /= cnt;
            return Tuple.Create(minValue, maxValue, meanValue);
        }

        public static void ShiftMatrix(float[][] matrix, float delta) {
            if (delta == 0.0f) return;
            MT.ForEach(matrix, R => {
                for (int j = 0; j < R.Length; j++)
                    R[j] = Math.Max(0, R[j]+delta);
            });
        }

        public static void PowerMatrix(float[][] matrix, double exp) {
            if (exp == 1.0) return;
            if (exp == 0.0) {
                foreach (var R in matrix)
                    for (int j = 0; j < R.Length; j++)
                        R[j] = 1.0f;
                return;
            }
            MT.ForEach(matrix, R => {
                for (int j = 0; j < R.Length; j++)
                    R[j] = (float)Math.Pow(R[j], exp);
            });
        }

        void ShowMatrix(float[][] M) {
            double[][] m = MathUtil.NewMatrix(M.Length, M[0].Length);
            for (int row = 0; row < M.Length; row++)
                Array.Copy(M[row], m[row], M[0].Length);
            var nt = NumberTable.CreateFromMatrix(m);
            var hm = new HeatMap(nt);
            Root.FormMgr.ShowForm(hm);
        }

        public float[][] AffinityToDistance(float[][] P, float[][] Q) {
            // Symmetrize P, Q to P
            const float eps = 2.22e-16f;
            int rows = P.Length;
            int columns = Q.Length;

            MT.Loop(0, rows, row => {
                for (int col = 0; col < columns; col++)
                    P[row][col] = P[row][col] + Q[col][row];
            });

            // Linearly map affinity to distance  in to the range [0, 1.0].
            var mmm = MinMaxMean(P);
            float range = Math.Max(eps, mmm.Item2 - mmm.Item1);
            float maxValue = mmm.Item2;
            MT.Loop(0, rows, row => {
                for (int col = 0; col < columns; col++)
                    P[row][col] = (maxValue - P[row][col]) / range;
            });
            return P;
        }

        static float[][] Normalize(float[][] matrix) {
            int rows = matrix.Length;
            int columns = matrix[0].Length;
            MT.ForEach(matrix, R => {
                float av = R.Average();
                double sum = 0.0;
                for (int col = 0; col < columns; col++) {
                    R[col] -= av;
                    sum += R[col] * R[col];
                }
                if (sum <= 0) {
                    float c = (float)(1.0 / Math.Sqrt(columns));
                    for (int col = 0; col < columns; col++)
                        R[col] = c;
                } else {
                    double norm = Math.Sqrt(sum);
                    for (int col = 0; col < columns; col++)
                        R[col] = (float)(R[col] / norm);
                }
            });
            return matrix;
        }

        static float[][] Transpose(float[][] matrix) {
            int rows = matrix.Length;
            int columns = matrix[0].Length;
            float[][] m = new float[columns][];
            MT.Loop(0, columns, row => {
                m[row] = new float[rows];
                for (int col = 0; col < rows; col++)
                    m[row][col] = matrix[col][row];
            });
            return m;
        }
        public static string GlyphSets = "36 Clusters|Ordered Glyphs|Colored Particles|Red Green|Colored Balls";

        static bool ValidateDataset(IDataset ds) {
            int i0 = ds.Rows - ds.Columns;
            var csList = ds.ColumnSpecList;
            if ((ds.Rows > ds.Columns) && Enumerable.Range(0, ds.Columns).All(i => ds.BodyList[i0 + i].Id == csList[i].Id)) {
                return true;
            } else {
                var ret = MsgBox.YesNoTimed("Current dataset is invalid for dual metric. Do you want to adjust the dataset for dual metric?", "Dual Metric", 5);                
                if ( ret == DialogResult.Yes ) { 
                    var rg = new Random();               
                    for (int col = 0; col < ds.Columns; col++) {
                        var cs = ds.ColumnSpecList[col];
                        var b = ds.AddRow(cs.Id, cs.Name, (short)16, null);
                        b.X = rg.Next((int)ds.CurrentMap.Width);
                        b.Y = rg.Next((int)ds.CurrentMap.Height);
                        b.IsDummy = true;
                    }
                    ds.CurrentMap.GlyphType = GlyphSets;
                    ds.CurrentMap.RedrawAll();
                    SingleCellPlugin.App.ScriptApp.Folder.DataChanged = true;
                    return true;
                } else {
                    return false;
                }
            }
        }

        struct Neighbor {
            public Neighbor(int idx, float dist) {
                this.index = idx;
                this.distance = dist;
            }
            public int index;
            public float distance;
        }

        #region Various propagation algorithms.
        void PropagateByNeighborsSimple(Neighbor[][] nbList, float[][] a) {
            float avAff = 0.0f;
            MT.ForEach(nbList, nbs => {
                float s = nbs.Sum(nb => nb.distance * nb.distance);
                lock (this)
                    avAff += s;
            });
            avAff = (float)Math.Sqrt(avAff / (nbList.Length * nbList[0].Length));

            int rows = a.Length;
            int columns = a[0].Length;

            MT.Loop(0, rows, row => {
                float[] newAff = new float[columns];
                float[] aR = a[row];
                for (int col = 0; col < columns; col++) {
                    float aff = aR[col];
                    foreach (var nb in nbList[col]) {
                        if (nb.distance >= avAff)
                            break;
                        aff += aR[nb.index];
                    }
                    newAff[col] = aff;
                }
                a[row] = newAff;
            });
        }

        void PropagateByNeighbors(Neighbor[][] nbList, float[][] a) {
            int kNN = nbList[0].Length;
            int rows = a.Length;
            int columns = a[0].Length;

            // Calculate the average distance to kNN neighbors.
            float avAff = 0.0f;
            MT.ForEach(nbList, nbs => {
                float s = nbs.Sum(nb => nb.distance);
                lock (this)
                    avAff += s;
            });
            avAff /= nbList.Length * kNN;
            
            // Prepare the exponentially descreasing coefficent to propgate neighboring affinity.
            MT.ForEach(nbList, nbs => {
                for (int k = 0; k < kNN; k++) {
                    double e = nbs[k].distance / avAff;
                    nbs[k].distance = (float)Math.Exp(-0.5 * e * e);
                }
            });

            // Propagate affinity from neibhoring columns.
            MT.Loop(0, rows, row => {
                float[] newAff = new float[columns];
                float[] aR = a[row];
                for (int col = 0; col < columns; col++) {
                    var nbs = nbList[col];
                    float aff = aR[col];
                    foreach (var nb in nbs)
                        aff += aR[nb.index] * nb.distance;
                    newAff[col] = aff;
                }
                a[row] = newAff;
            });
        }

        void ShowNeighbors(Neighbor[][] nbList) {
            var M = MathUtil.NewMatrix(nbList.Length, nbList[0].Length, (i,j) => nbList[i][j].distance);
            var nt = NumberTable.CreateFromMatrix(M);
            var csList = Root.Data.ColumnSpecList;

            if ( M.Length == csList.Count ) {
                for (int row = 0; row < M.Length; row++)
                    nt.RowSpecList[row].Id = csList[row].Id;
            } else { // M.Length == Root.Data.Columns.
                for (int row = 0; row < M.Length; row++)
                    nt.RowSpecList[row].Id = Root.Data.BodyList[row].Id;
            }
            Root.FormMgr.ShowForm(new HeatMap(nt));
        }
        #endregion

        // Find the kNN neighbors of each node based on distance matrix cDist[].
        Neighbor[][] FindNeighbors(float[][] cDist, int maxKnn) {
            int columns = cDist.Length;
            int kNN = Math.Min(columns - 1, maxKnn);
            Neighbor[][] nbList = new Neighbor[columns][]; // each row stores the kNN closet columns of a column.

            MT.Loop(0, columns, col => {
                // find the kNN nearest columns/neighbors of col-the column.
                Neighbor[] nbs = nbList[col] = new Neighbor[kNN];
                int kNN2 = 0; // The actually number of neighbors in nbs[]
                for (int c = 0; c < columns; c++) {
                    if (c == col)
                        continue;
                    float dist = (c < col) ? cDist[col][c] : cDist[c][col];
                    if (dist <= 0)
                        continue;
                    int i = 0;
                    for (i = kNN2 - 1; i >= 0; i--)
                        if (nbs[i].distance <= dist)
                            break;
                    // at here we have nbs[i].distance <= dist < nbs[i+1].distance

                    kNN2 = Math.Min(kNN, kNN2 + 1);
                    i++;
                    // Copy nbs[i:*] to nbs[i+1:*]
                    int len = kNN2 - i - 1;
                    if (len > 0)
                        Array.Copy(nbs, i, nbs, i + 1, len);
                    if (i < kNN2) {
                        nbs[i].distance = dist;
                        nbs[i].index = c;
                    }
                }
            });
            return nbList;
        }

        // probagate affinity between columns of cDist to rows of a[][].
        void ProbagateAffinity(float[][] cDist, float[][] a) {
            Neighbor[][] nbList = FindNeighbors(cDist, 16);            
            PropagateByNeighbors(nbList, a);
        }

        #region default methods/properties
        public string Name
        {
            get {
                switch (metricMode) {
                    case MetricMode.CorCor: return "Dual.Correlation";
                    case MetricMode.CorEuc: return "Dual.CorEuc";
                    case MetricMode.EucCor: return "Dual.EucCor";
                    case MetricMode.EucEuc: return "Dual.Euclidean";
                    default:
                        return "Undefined";
                }
            }
            set {; }
        }
        public IFilterEditor FilterEditor
        {
            get => null;
        }
        public bool IsApplicable(IDataset dataset) {
            return true;
        }
        public bool IsApplicable(IDataset dataset, XmlElement filterNode) {
            return false;
        }
        #endregion
    }
}
