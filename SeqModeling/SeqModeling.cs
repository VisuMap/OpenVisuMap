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

        public List<IBody> ClusterContract(List<IBody> bList, double factor) {
            Dictionary<short, double[]> centers = new Dictionary<short, double[]>();
            foreach (var b in bList) {
                if (!centers.ContainsKey(b.Type)) 
                    centers[b.Type] = new double[4];
                double[] v = centers[b.Type];
                v[0] += b.X;
                v[1] += b.Y;
                v[2] += b.Z;
                v[3] += 1.0;
            }

            foreach (var c in centers.Values) {
                c[0] /= c[3];
                c[1] /= c[3];
                c[2] /= c[3];
            }

            foreach (var b in bList) {
                double[] c = centers[b.Type];
                b.X = c[0] + factor * (b.X - c[0]);
                b.Y = c[1] + factor * (b.Y - c[1]);
                b.Z = c[2] + factor * (b.Z - c[2]);
            }
            return bList;
        }

        public List<IBody> Interpolate3D(List<IBody> bList, int repeats, double convexcity, int bIdx0) {
            if( (bList.Count <= 1) || (repeats==0) )
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
                    P[1] += 0.35f * eps * (P[1] - P[4]);
                    P[K-2] += 0.35f * eps * (P[K - 2] - P[K - 5]);
                }
                D = P;
            }

            // Insert the interpolating data points.
            int secL = 1 << repeats;
            int L2 = secL / 2;
            Body b0 = null;
            List<IBody> bs = new List<IBody>();
            for (int k=0; k<D.Length; k+=secL) {                
                b0 = bList[k/secL] as Body;
                for (int i=k-L2; i<k+L2; i++) {
                    if ( i == k) {
                        bs.Add(b0);
                    } else if ( (i>=0) && (i<D.Length) ) {
                        Body b = new Body("i" + (bIdx0 + bs.Count));
                        b.Name = b0.Name;
                        b.Type = b0.Type;
                        b.Flags = b0.Flags;
                        b.SetXYZ(D[i].X, D[i].Y, D[i].Z);
                        bs.Add(b);
                    }
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
