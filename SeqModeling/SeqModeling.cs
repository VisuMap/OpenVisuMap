using System;
using System.Linq;
using System.Collections.Generic;
using VisuMap.Script;
using VisuMap.LinearAlgebra;

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
            pList = new List<string>(pList);
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
            pList = new List<string>(pList);
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
            for (int col = 0; col < L; col++)
                nt.ColumnSpecList[col].Id = ppList[col];
            var bList = vv.Dataset.BodyListForId(pList);
            for (int row = 0; row < pList.Count; row++)
                nt.RowSpecList[row].CopyFromBody(bList[row]);
            return nt;
        }

        public INumberTable ToWaveTable(string pSeq, IList<string> ppList, int M) {
            ppList = new List<string>(ppList);
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

        public INumberTable GetPairLinkage(bool toDistances = true) {
            var ppList = vv.GroupManager.GetGroupLabels("KeyPairs400");
            var PP = new Dictionary<string, int>();
            int L = ppList.Count;
            for (int k = 0; k < L; k++)
                PP[ppList[k]] = k;
            var T = New.NumberTable(L, L);
            for (int row = 21; row < vv.Dataset.Rows; row++)
           //for(int row=1; row<21; row++)
           {
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
            foreach (double[] R in M) {
                double rowSum = vv.Math.Sum(R);
                if (rowSum != 0)
                    for (int col = 0; col < L; col++)
                        R[col] /= rowSum;
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
            var ppList = vv.GroupManager.GetGroupLabels("KeyPairs400");
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

        public INumberTable MarkovianMatrix1(Script.IDataset ds) {
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

    }
}
