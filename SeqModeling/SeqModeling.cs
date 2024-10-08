using System;
using System.Linq;
using System.Collections.Generic;
using VisuMap.Script;
//using Vector3 = SharpDX.Vector3;
using Vector3 = System.Numerics.Vector3;

namespace VisuMap {

    public class VectorN {
        float[] v;

        public VectorN(float[] v) : this(v.Length) {
            Array.Copy(v, this.v, v.Length);
        }

        public VectorN(int vDim) {
            this.v = new float[vDim];
        }

        public float[] Vector { get => v; }

        public float this[int index]
        {
            get => this.v[index];
            set => this.v[index] = value;
        }

        public static VectorN[] NewVectorN(int length, int vDim) {
            VectorN[] vn = new VectorN[length];
            for (int k = 0; k < length; k++)
                vn[k] = new VectorN(vDim);
            return vn;
        }
    }


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
            if ((bList.Count <= 1) || (repeats == 0))
                return bList;

            Vector3[] D = new Vector3[bList.Count];
            for (int k = 0; k < D.Length; k++) {
                D[k].X = (float)bList[k].X;
                D[k].Y = (float)bList[k].Y;
                D[k].Z = (float)bList[k].Z;
            }
            float eps = (float)convexcity;
            for (int n = 0; n < repeats; n++) {
                int L = D.Length;
                int K = 2 * L - 1;
                Vector3[] P = new Vector3[K];
                P[0] = D[0];
                for (int k = 1; k < L; k++) {
                    P[2 * k - 1] = 0.5f * (D[k - 1] + D[k]);
                    P[2 * k] = D[k];
                }
                for (int k = 3; k < (K - 2); k += 2)
                    P[k] += eps * (2 * P[k] - P[k - 3] - P[k + 3]);
                if (K > 4) {
                    P[1] += 0.35f * eps * (P[1] - P[4]);
                    P[K - 2] += 0.35f * eps * (P[K - 2] - P[K - 5]);
                }
                D = P;
            }

            // Insert the interpolating data points.
            int secL = 1 << repeats;
            int L2 = secL / 2;
            Body b0 = null;
            List<IBody> bs = new List<IBody>();
            for (int k = 0; k < D.Length; k += secL) {
                b0 = bList[k / secL] as Body;
                for (int i = k - L2; i < k + L2; i++) {
                    if (i == k) {
                        bs.Add(b0);
                    } else if ((i >= 0) && (i < D.Length)) {
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

        public INumberTable InterpolateVector(float[][] vList, int repeats, double convexcity) {
            if ((vList.Length <= 1) || (repeats == 0))
                return null;
            VectorN[] D = new VectorN[vList.Length];
            for (int k = 0; k < vList.Length; k++)
                D[k] = new VectorN(vList[k]);
            int vDim = vList[0].Length;
            float eps = (float)convexcity;

            for (int n = 0; n < repeats; n++) {
                int L = D.Length;
                int K = 2 * L - 1;
                VectorN[] P = VectorN.NewVectorN(K, vDim);
                for (int i = 0; i < vDim; i++)
                    P[0][i] = D[0][i];
                for (int k = 1; k < L; k++) {
                    for (int i = 0; i < vDim; i++) {
                        P[2 * k - 1][i] = 0.5f * (D[k - 1][i] + D[k][i]);
                        P[2 * k][i] = D[k][i];
                    }
                }

                for (int k = 3; k < (K - 2); k += 2)
                    for (int i = 0; i < vDim; i++) {
                        P[k][i] += eps * (2 * P[k][i] - P[k - 3][i] - P[k + 3][i]);
                    }

                if (K > 4) {
                    for (int i = 0; i<vDim; i++) {
                        P[1][i] += 0.35f * eps * (P[1][i] - P[4][i]);
                        P[K - 2][i] += 0.35f * eps * (P[K - 2][i] - P[K - 5][i]) ;
                    }
                }
                D = P;
            }

            return New.NumberTable(D.Select(v => v.Vector).ToArray());            
        }


        static double[] ToVector(string pSeq, Dictionary<char, int> P, int[] aaPos, int[][] aaSize, 
                double[] clusterWeight, double[] waveWeight, int secLen, int secCount) {
            int clusters = aaPos.Length;
            int wLen = waveWeight.Length;
            List<double[]> vSec = new List<double[]>();
            for (int s = 0; s < pSeq.Length; s += secLen) {
                for (int k = 0; k < clusters; k++) {
                    aaPos[k] = -1;
                    for (int i = 0; i < wLen; i++)
                        aaSize[k][i] = 0;
                }

                int secEnd = Math.Min(s + secLen, pSeq.Length);
                if (vSec.Count == (secCount - 1))  // The last section will include all the rest.
                    secEnd = pSeq.Length;

                for (int k = s; k < secEnd; k++) {
                    if (P.ContainsKey(pSeq[k])) {
                        int aaIdx = P[pSeq[k]];
                        int sz = Math.Min(wLen, k - aaPos[aaIdx]);
                        aaSize[aaIdx][sz - 1] += 1;
                        aaPos[aaIdx] = k;
                    }
                }
                double[] pV = new double[clusters];
                for (int k = 0; k < clusters; k++) {
                    double cw = clusterWeight[k];
                    for (int i = 0; i < wLen; i++)
                        pV[k] += cw * aaSize[k][i] * waveWeight[i];
                }
                vSec.Add(pV);

                if (vSec.Count == secCount)
                    break;
            }

            double[] pRow = new double[secCount * clusters];
            for (int k = 0; k < vSec.Count; k++)
                Array.Copy(vSec[k], 0, pRow, k * clusters, clusters);
            return pRow;
        }

        // Returns a dictionary that maps aa to their aa-cluster indexes.
        Dictionary<char, int> Cluster2Index(string aaGroups) {
            string[] cList = aaGroups.Split('|');
            Dictionary<char, int> P = new Dictionary<char, int>();
            for (int cIdx = 0; cIdx < cList.Length; cIdx++)
                foreach (char c in cList[cIdx])
                    P[c] = cIdx;
            return P;
        }

        public INumberTable VectorizeProtein(IList<string> pList, VisuMap.Script.IDataset pTable, string aaGroups, int sections) {
            var P = Cluster2Index(aaGroups);
            int clusters = P.Values.Max() + 1;
            int wLen = 50; // maximal gape or wave length
            int[][] aaSize = new int[clusters][];
            int[] aaPos = new int[clusters];
            double[] waveWeight = new double[wLen];   // weight for different wave-length (gape size).
            for (int k = 0; k < clusters; k++)
                aaSize[k] = new int[wLen];
            for (int k = 0; k < wLen; k++)
                waveWeight[k] = 1.0 / (k + 0.1);
            List<int> cSize = Enumerable.Range(0, clusters).Select(cIdx => P.Count(aa => aa.Value == cIdx)).ToList();
            double[] clusterWeight = cSize.Select(sz => 1.0 / sz ).ToArray();

            List<double[]> vList = new List<double[]>();
            foreach (string pId in pList) {
                int rowIdx = pTable.IndexOfRow(pId);
                if (rowIdx < 0)
                    continue;
                string pSeq = pTable.GetDataAt(rowIdx, 2);
                int secLen = Math.Max(50, pSeq.Length / sections + 1);
                double[] pRow = ToVector(pSeq, P, aaPos, aaSize, clusterWeight, waveWeight, secLen, sections);
                vList.Add(pRow);
            }
            return New.NumberTable(vList.ToArray());
        }

        public INumberTable VectorizeProtein2(IList<string> pList, VisuMap.Script.IDataset pTable, string aaGroups, INumberTable transMatrix) {
            var P = Cluster2Index(aaGroups);
            int clusters = P.Values.Max() + 1;
            int L = transMatrix.Rows;
            int N = transMatrix.Columns;
            double[][] tM = transMatrix.Matrix as double[][];

            double[][] vList = new double[pList.Count][];
            for(int pIdx=0; pIdx<pList.Count; pIdx++) { 
                vList[pIdx] = new double[clusters * N];
                int rowIdx = pTable.IndexOfRow(pList[pIdx]);
                if (rowIdx < 0)
                    continue;
                double[] pVector = vList[pIdx];
                string pSeq = pTable.GetDataAt(rowIdx, 2);
                for (int k = 0; k < pSeq.Length; k++) {
                    char c = pSeq[k];
                    if (!P.ContainsKey(c))
                        continue;
                    int col0 = P[c] * N;
                    double[] R_k = tM[k % L];
                    for (int col = 0; col < N; col++)
                        pVector[col0 + col] += R_k[col];
                }
            }
            return New.NumberTable(vList);
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
