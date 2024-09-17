using System;
using System.Linq;
using System.Collections.Generic;
using VisuMap.Script;
using VisuMap.LinearAlgebra;
using Vector3 = SharpDX.Vector3;

namespace VisuMap
{
    public class SeqModeling {
        IVisuMap vv;
        INew New;

        public SeqModeling() {
            this.vv = VisuMapImp.GetVisuMapImp();
            this.New = vv.New;
        }

        public IBody MeanPoint(IList<IBody> bList) {
            if (bList.Count == 0)
                return null;
            double x = 0;
            double y = 0;
            foreach (IBody b in bList) {
                x += b.X;
                y += b.Y;
            }
            x /= bList.Count;
            y /= bList.Count;
            IBody mBody = null;
            double mDist = 1.0e10;
            foreach (IBody b in bList) {
                double d2 = (b.X - x) * (b.X - x) + (b.Y - y) * (b.Y - y);
                if (d2 < mDist) {
                    mBody = b;
                    mDist = d2;
                }
            }
            return mBody;
        }

        public List<IBody> Interpolate3D(List<IBody> bList, int repeats, double convexcity, int bIdx0) {
            if (bList.Count <= 1)
                return bList;
            Vector3[] D = new Vector3[bList.Count];
            for (int k = 0; k < D.Length; k++) {
                D[k].X = (float) bList[k].X;
                D[k].Y = (float) bList[k].Y;
                D[k].Z = (float) bList[k].Z;
            }
            float eps = (float) convexcity;
            for(int n=0; n<repeats; n++) {
                int L = D.Length;
                int K = 2 * L - 1;
                Vector3[] P = new Vector3[K];
                P[0] = D[0];
                for(int k=1; k<L; k++) {
                    P[2 * k - 1] = 0.5f * (D[k - 1] + D[k]);
                    P[2 * k] = D[k];
                }
                for (int k = 3; k < (K - 2); k += 2)
                    P[k] += eps * (2 * P[k] - P[k - 3] - P[k + 3]);
                if ( K > 4 ) {
                    P[1] += eps * (P[1] - P[4]);
                    P[K - 2] += eps * (P[K - 2] - P[K - 5]);
                }
                D = P;
            }
            int secL = 1 << repeats;
            List<IBody> bs = new List<IBody>();
            for(int k=0; k<D.Length; k++) {
                Body b0 = bList[k / secL] as Body;
                if (k % secL == 0) {
                    bs.Add(b0);
                } else {
                    Body b = new Body("i" + (bIdx0 + k));
                    b.Name = b0.Name;
                    b.Type = b0.Type;
                    b.Flags = b0.Flags;
                    b.SetXYZ(D[k].X, D[k].Y, D[k].Z);
                    bs.Add(b);
                }
            }
            return bs;
        }

        public INumberTable VectorizeProtein1(string alphabet, int M, IList<string> pList, VisuMap.Script.IDataset pTable) {
            int L = alphabet.Length;
            Dictionary<char, int> P = Enumerable.Range(0, alphabet.Length).ToDictionary(k => alphabet[k], k => k);
            int[][] aaSize = new int[L][];
            int[] aaPos = new int[L];
            double[] weight = new double[M];
            for (int k = 0; k < L; k++)
                aaSize[k] = new int[M];
            for (int k = 0; k < M; k++)
                weight[k] = 1.0 / (k + 0.1);
            List<double[]> vList = new List<double[]>();
            foreach (string pId in pList) {
                int rowIdx = pTable.IndexOfRow(pId);
                if (rowIdx < 0)
                    continue;
                string pSeq = pTable.GetDataAt(rowIdx, 2);
                for (int k = 0; k < L; k++) {
                    aaPos[k] = -1;
                    for (int i = 0; i < M; i++)
                        aaSize[k][i] = 0;
                }
                for (int k = 0; k < pSeq.Length; k++) {
                    if (P.ContainsKey(pSeq[k])) {
                        int aaIdx = P[pSeq[k]];
                        int sz = Math.Min(M, k - aaPos[aaIdx]);
                        aaSize[aaIdx][sz - 1] += 1;
                        aaPos[aaIdx] = k;
                    }
                }
                double[] pV = new double[L];
                for (int k = 0; k < L; k++)
                    for (int i = 0; i < M; i++)
                        pV[k] += aaSize[k][i] * weight[i];
                vList.Add(pV);
            }
            return New.NumberTable(vList.ToArray());
        }

        public INumberTable VectorizeProtein2(IList<string> ppList, int M, IList<string> pList, VisuMap.Script.IDataset pTable) {
            int L = ppList.Count;
            var P = new Dictionary<string, int>();
            ushort[][] aaSize = new ushort[L][];
            int[] aaPos = new int[L];
            for (int k = 0; k < L; k++) {
                P[ppList[k]] = k;
                aaSize[k] = new ushort[M];
            }
            double[] weight = new double[M];
            for (int k = 0; k < M; k++)
                weight[k] = 1.0 / (k + 0.5);
            List<double[]> vList = new List<double[]>();
            foreach (string pId in pList) {
                int rowIdx = pTable.IndexOfRow(pId);
                if (rowIdx < 0)
                    continue;
                string pSeq = pTable.GetDataAt(rowIdx, 2);
                for (int k = 0; k < L; k++) {
                    aaPos[k] = -1;
                    for (int i = 0; i < M; i++)
                        aaSize[k][i] = 0;
                }
                for (int k = 0; k < (pSeq.Length - 1); k++) {
                    string aaKey = pSeq.Substring(k, 2);
                    if (P.ContainsKey(aaKey)) {
                        int aaIdx = P[aaKey];
                        int sz = Math.Min(M, k - aaPos[aaIdx]);
                        aaSize[aaIdx][sz - 1] += 1;
                        aaPos[aaIdx] = k;
                    }
                }
                double[] pV = new double[L];
                for (int k = 0; k < L; k++)
                    for (int i = 0; i < M; i++)
                        pV[k] += aaSize[k][i] * weight[i];
                vList.Add(pV);
            }

            var nt = New.NumberTable(vList.ToArray());
            var P1 = Enumerable.Range(0, AB.Length).ToDictionary(k => AB[k], k => k);
            for (int col = 0; col < L; col++) {
                string id = ppList[col];
                nt.ColumnSpecList[col].Id = id;
                nt.ColumnSpecList[col].Type = (short)P1[id[0]];
            }
            var bList = vv.Dataset.BodyListForId(pList);
            for (int row = 0; row < pList.Count; row++)
                nt.RowSpecList[row].CopyFromBody(bList[row]);
            return nt;
        }

        public INumberTable ToWaveTable(string pSeq, IList<string> ppList, int M) {
            int L = ppList.Count;
            var P = new Dictionary<string, int>();
            double[][] aaSize = new double[L][];
            int[] aaPos = new int[L];
            int kSize = ppList[0].Length;
            for (int k = 0; k < L; k++) {
                P[ppList[k]] = k;
                aaSize[k] = new double[M];
                aaPos[k] = -1;
            }
            int kEnd = pSeq.Length - kSize + 1;
            for (int k = 0; k < kEnd; k += 1) {
                string aaPair = pSeq.Substring(k, kSize);
                if (P.ContainsKey(aaPair)) {
                    int aaIdx = P[aaPair];
                    int sz = Math.Min(M, k - aaPos[aaIdx]);
                    aaSize[aaIdx][sz - 1] += 1.0;
                    aaPos[aaIdx] = k;
                }
            }
            var nt = New.NumberTable(aaSize);
            for (int row = 0; row < L; row++)
                nt.RowSpecList[row].Id = ppList[row];
            return nt;
        }

        const string AB = "ARNDCEQGHILKMFPSTWYV"; // "WCMYHFNIDQTRVKGPAESL";
        List<string> GetAAPairs() {
            List<string> aaPairList = new List<string>();
            foreach (char a in AB)
                foreach (char b in AB)
                    aaPairList.Add(new string(new char[] { a, b }));
            return aaPairList;
        }

        public INumberTable GetPairLinkage(bool toDistances = true, bool normalizing=true) {
            var ppList = GetAAPairs();
            var PP = new Dictionary<string, int>();
            int L = ppList.Count;
            for (int k = 0; k < L; k++)
                PP[ppList[k]] = k;
            var T = New.NumberTable(L, L);
            for (int row = 21; row < vv.Dataset.Rows; row++)  {
                string S = vv.Dataset.GetDataAt(row, 2);
                for (int k = 0; k < (S.Length - 3); k++) {
                    int rIdx = PP[S.Substring(k, 2)];
                    double[] R = T.Matrix[rIdx] as double[];
                    R[PP[S.Substring(k + 2, 2)]] += 1.0;
                }
            }

            var P = Enumerable.Range(0, AB.Length).ToDictionary(k => AB[k], k => k);
            for (int k = 0; k < L; k++) {
                string id = ppList[k];
                short ty = (short)P[id[0]];
                T.ColumnSpecList[k].Id = id;
                T.RowSpecList[k].Id = id;
                T.ColumnSpecList[k].Group = ty;
                T.RowSpecList[k].Type = ty;
            }

            var M = T.Matrix;

            if (normalizing) {
                foreach (double[] R in M) {
                    double rowSum = vv.Math.Sum(R);
                    if (rowSum != 0)
                        for (int col = 0; col < L; col++)
                            R[col] /= rowSum;
                }
            }

            if (toDistances) {
                for (int row = 0; row < L; row++)
                    for (int col = 0; col < row; col++) {
                        double v = 0.5 * (M[row][col] + M[col][row]);
                        v = -Math.Log(Math.Max(0.000001, v));
                        M[row][col] = M[col][row] = v;
                    }
            }
            return T;
        }

        public INumberTable GetMarkovCoding1(Script.IDataset ds, List<string> pList) {
            int L = AB.Length;
            var P = Enumerable.Range(0, L).ToDictionary(k => AB[k], k => k);
            var T = New.NumberTable(ds.Rows, L);

            for (int row = 0; row < pList.Count; row++) {
                double[][] M = VisuMap.MathUtil.NewMatrix(L, L);
                int pIdx = ds.IndexOfRow(pList[row]);
                string S = ds.GetDataAt(pIdx, 2);
                int rIdx = P[S[0]];
                for (int k = 1; k < S.Length; k++) {
                    int cIdx = P[S[k]];
                    M[rIdx][cIdx] += 1.0;
                    rIdx = cIdx;
                }

                foreach (double[] R in M) {
                    double rowSum = vv.Math.Sum(R);
                    if (rowSum > 0)
                        for (int col = 0; col < L; col++)
                            R[col] /= rowSum;
                }

                IterateMarkovian(M, L, 16, T.Matrix[row] as double[]);
                T.RowSpecList[row].CopyFromBody(ds.BodyList[pIdx]);
            }

            for (int col = 0; col < L; col++)
                T.ColumnSpecList[col].Id = AB[col].ToString();
            return T;
        }

        // Iterate the markovian process by given steps and store the equalibrium/stationary distribution into 
        // given vector.
        public void IterateMarkovian(double[][] M, int L, int steps, double[] stDistribution) {
            double[] D = new double[L];
            Array.Copy(M[0], D, L);
            M = Matrix.Transpose(M);
            for (int n = 0; n < steps; n++)
                D = Matrix.MultVector(M, D);
            Array.Copy(D, stDistribution, L);
        }

        public INumberTable GetMarkovCoding2(Script.IDataset ds, List<string> pList) {
            var ppList = GetAAPairs();
            int L = ppList.Count;
            var PP = Enumerable.Range(0, L).ToDictionary(k => ppList[k], k => k);
            var T = New.NumberTable(pList.Count, L);
            
            for (int row = 0; row < pList.Count; row++) {
                double[][] M = MathUtil.NewMatrix(L, L);
                int pIdx = ds.IndexOfRow(pList[row]);
                string S = ds.GetDataAt(pIdx, 2);
                int rIdx = PP[S.Substring(0, 2)];
                for (int k = 2; k <S.Length-2; k+=2) {
                    int cIdx = PP[S.Substring(k, 2)];
                    M[rIdx][cIdx] += 1.0f;
                    rIdx = cIdx;
                }
                rIdx = PP[S.Substring(1, 2)];
                for (int k = 3; k < S.Length - 2; k += 2) {
                    int cIdx = PP[S.Substring(k, 2)];
                    M[rIdx][cIdx] += 1.0;
                    rIdx = cIdx;
                }
                foreach (double[] R in M) {
                    double rowSum = R.Sum();
                    if (rowSum > 0)
                        for (int col = 0; col < L; col++)
                            R[col] /= rowSum;
                }

                IterateMarkovian(M, L, 16, T.Matrix[row] as double[]);
                T.RowSpecList[row].CopyFromBody(ds.BodyList[pIdx]);
            }

            var P = Enumerable.Range(0, 20).ToDictionary(k => AB[k], k => k);
            for (int col = 0; col < L; col++) {
                var cs = T.ColumnSpecList[col];
                cs.Id = ppList[col];
                cs.Group = (short)P[cs.Id[0]];
            }
            return T;
        }

        public INumberTable MarkovianMatrix1(Script.IDataset ds, bool normalizing=true) {
            int L = AB.Length;
            var P = Enumerable.Range(0, L).ToDictionary(k => AB[k], k => k);
            double[][] M = VisuMap.MathUtil.NewMatrix(L, L);

            for (int row = 21; row < ds.Rows; row++) {
                string S = ds.GetDataAt(row, 2);
                int rIdx = P[S[0]];
                for (int k = 1; k < S.Length; k++) {
                    int cIdx = P[S[k]];
                    M[rIdx][cIdx] += 1.0;
                    rIdx = cIdx;
                }
            }

            if ( normalizing ) 
                foreach (double[] R in M) {
                    double rowSum = vv.Math.Sum(R);
                    if (rowSum > 0)
                        for (int col = 0; col < L; col++)
                            R[col] /= rowSum;
            }
            var T = New.NumberTable(M);
            for (int k = 0; k < L; k++) 
                T.RowSpecList[k].Id = T.ColumnSpecList[k].Id = AB[k].ToString();
            return T;
        }

        public double[][] MarkovianMatrix0(string pSeq) {
            int L = AB.Length;
            double[][] M = VisuMap.MathUtil.NewMatrix(L, L); 
            Dictionary<char, int> P = Enumerable.Range(0, L).ToDictionary(k => AB[k], k => k);

            int rIdx = P[pSeq[0]];
            for (int k = 1; k < pSeq.Length; k++) {
                int cIdx = P[pSeq[k]];
                M[rIdx][cIdx] += 1.0;
                rIdx = cIdx;
            }
            foreach (double[] R in M) {
                double rowSum = vv.Math.Sum(R);
                if (rowSum > 0)
                    for (int col = 0; col<L; col++)
                        R[col] /= rowSum;
            }
            return M;
        }

        public void SmoothenBodyList(IList<IBody> bs) {
            var B = New.NumberTable(bs, 3).Matrix;
            for (int k = 1; k < (bs.Count - 1); k++) {
                var b = bs[k];
                if ((bs[k - 1].Type == b.Type) && (b.Type == bs[k + 1].Type)) {
                    var T = B[k];
                    var P = B[k - 1];
                    var Q = B[k + 1];
                    b.X = 0.5 * T[0] + 0.25 * (P[0] + Q[0]);
                    b.Y = 0.5 * T[1] + 0.25 * (P[1] + Q[1]);
                    b.Z = 0.5 * T[2] + 0.25 * (P[2] + Q[2]);
                }
            }
        }
    }
}
