using System;
using System.Linq;
using System.Collections.Generic;
using VisuMap.Script;
using Vector3 = SharpDX.Vector3;
using RMatrix = SharpDX.Matrix3x3;
using Quaternion = SharpDX.Quaternion;
using System.IO;

namespace VisuMap {
    public static class MyExtensions {
        public static Vector3 ToV3(this IBody b) {
            return new Vector3((float)b.X, (float)b.Y, (float)b.Z);
        }

        public static void SetXYZ(this IBody b, Vector3 p) {
            b.SetXYZ(p.X, p.Y, p.Z);
        }
    }

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

        public List<IBody> LocalExpand(List<IBody> bList, double factor) {
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
        public void LocalSmoothen(List<IBody> bList, double smoothenRatio, int repeats=8) {
            if ((bList==null) || (bList.Count < 3) || (repeats <= 0))
                return;
            int N = bList.Count;
            Vector3[] P = new Vector3[N];
            for (int k = 0; k < N; k++) {
                P[k].X = (float)bList[k].X;
                P[k].Y = (float)bList[k].Y;
                P[k].Z = (float)bList[k].Z;
            }
            Vector3[] Mean = new Vector3[N - 2];
            float c = -(float)smoothenRatio;
            for (int rp=0; rp<repeats; rp++) {
                for(int k=0; k<Mean.Length; k++) 
                    Mean[k] = 0.5f * (P[k] + P[k + 2]);
                for (int k = 0; k < Mean.Length; k++)
                    P[k+1] += c * (P[k+1] - Mean[k]);
            }
            for(int k=1; k<(N-1); k++)
                bList[k].SetXYZ(P[k].X, P[k].Y, P[k].Z);
        }

        public bool LocalSmoothen2(double[][] M, double smoothenRatio, int repeats = 8) {
            if ((M == null) || (M.Length < 3) || (M[0].Length != 3) || (repeats <= 0))
                return false;

            int N = M.Length;
            Vector3[] P = new Vector3[N];
            for (int k = 0; k < N; k++) {
                double[] R = M[k];
                P[k].X = (float)R[0];
                P[k].Y = (float)R[1];
                P[k].Z = (float)R[2];
            }
            Vector3[] Mean = new Vector3[N - 2];
            float c = -(float)smoothenRatio;
            for (int rp = 0; rp < repeats; rp++) {
                for (int k = 0; k < Mean.Length; k++)
                    Mean[k] = 0.5f * (P[k] + P[k + 2]);
                for (int k = 0; k < Mean.Length; k++)
                    P[k + 1] += c * (P[k + 1] - Mean[k]);
            }

            for (int k = 1; k < (N - 1); k++) {
                double[] R = M[k];
                R[0] = P[k].X;
                R[1] = P[k].Y;
                R[2] = P[k].Z;
            }
            return true;
        }

        public void PcaNormalize2(List<IBody> bodyList) {
            var nt = New.NumberTable(bodyList, 3);
            PcaNormalize(nt);
            var M = nt.Matrix;
            for (int k = 0; k < bodyList.Count; k++) {
                var b = bodyList[k];
                b.X = M[k][0];
                b.Y = M[k][1];
                b.Z = M[k][2];
            }
        }

        public void PcaNormalize(INumberTable nt) {
            if (nt.Rows <= 3) 
                return;        
            double[][] M = nt.Matrix as double[][];
            int rows = M.Length;

            MathUtil.CenteringInPlace(M);

            // Since the helix AA's position have less flactuation, we try to give them large weight
            // in the pca-normalization steps.
            double[] weights = nt.RowSpecList.Select(s => s.Name.EndsWith("h") ? 5.0 : 1.0).ToArray();
            for (int row = 1; row < rows; row++)
                weights[row] = 0.25 * weights[row] + 0.75 * weights[row - 1];
            for (int row = 0; row < rows; row++) 
            for (int col = 0; col < 3; col++)
                M[row][col] *= weights[row];

            double[][] E = MathUtil.DoPca(M, 3);
            double dt = E[0][0] * E[1][1] * E[2][2] + E[1][0] * E[2][1] * E[0][2] + E[0][1] * E[1][2] * E[2][0]
                - E[0][2] * E[1][1] * E[2][0] - E[0][1] * E[1][0] * E[2][2] - E[0][0] * E[2][1] * E[1][2];

            bool flipped = (dt < 0);  // Does the PCA transformation include a flipping/mirroring?

            MT.ForEach(M, R => {
                double x = R[0] * E[0][0] + R[1] * E[0][1] + R[2] * E[0][2];
                double y = R[0] * E[1][0] + R[1] * E[1][1] + R[2] * E[1][2];
                double z = R[0] * E[2][0] + R[1] * E[2][1] + R[2] * E[2][2];
                R[0] = x;
                R[1] = y;
                R[2] = z;
            });

            int N = Math.Min(100, rows / 2);
            bool[] flip = new bool[3];
            for (int col = 0; col < 3; col++) {
                flip[col] = M.Take(N).Select(R => R[col]).Sum() > 0;
                if (flip[col])
                    flipped = !flipped;
            }

            if (flipped) flip[2] = !flip[2];
            MT.ForEach(M, R => {
                for(int col = 0; col < 3; col++)
                    if (flip[col])
                        R[col] = -R[col];
            });

            // Weighted centering
            //MathUtil.CenteringInPlace(M);

            for (int row = 0; row < rows; row++)
            for (int col = 0; col < 3; col++)
                M[row][col] /= weights[row];
        }

        public List<IBody> Interpolate3D(List<IBody> bList, int repeats, double convexcity, int bIdx0, int chIdx) {
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
                        Body b = new Body("i", b0.Name, b0.Type);
                        b.Flags = b0.Flags;
                        b.SetXYZ(D[i].X, D[i].Y, D[i].Z);
                        bs.Add(b);
                    }
                }
            }
            string secPrefix = "";
            int secIdx = 0;
            for(int k=0; k<bs.Count; k++) {
                IBody b = bs[k];
                if ( b.Id[0] == 'i' ) {
                    b.Id = secPrefix + secIdx;
                    secIdx++;
                } else {
                    int rsIdx = 0;
                    if( (b.Id[0] == 'A') && (char.IsDigit(b.Id[1])) )
                        rsIdx = int.Parse(b.Id.Split('.')[0].Substring(1));
                    secPrefix = "i" + chIdx + "."+ rsIdx + ".";
                    secIdx = 0;
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


        // Returns a dictionary that maps aa to their aa-cluster indexes.
        Dictionary<char, int> Cluster2Index(string aaGroups) {
            string[] cList = aaGroups.Split('|');
            Dictionary<char, int> P = new Dictionary<char, int>();
            for (int cIdx = 0; cIdx < cList.Length; cIdx++)
                foreach (char c in cList[cIdx])
                    P[c] = cIdx;
            return P;
        }

 
        Dictionary<char, List<int>> Cluster2IdxList(string aaGroups) {
            string[] cList = aaGroups.Split('|');
            var P = new Dictionary<char, List<int>>();
            for (int cIdx = 0; cIdx < cList.Length; cIdx++) {
                foreach (char c in cList[cIdx]) {
                    if (!P.ContainsKey(c))
                        P[c] = new List<int>();
                    P[c].Add(cIdx);
                }
            }
            return P;
        }

        public INumberTable VectorizeProtein(IList<string> seqList, string aaGroups, INumberTable transMatrix) {
            var P = Cluster2IdxList(aaGroups);
            int clusters = P.Values.Max(vs=>vs.Max()) + 1;
            int L = transMatrix.Rows;
            int N = transMatrix.Columns;
            double[][] tM = transMatrix.Matrix as double[][];

            double[][] vList = new double[seqList.Count][];
            for (int pIdx = 0; pIdx < seqList.Count; pIdx++) {
                vList[pIdx] = new double[clusters * N];
                double[] pVector = vList[pIdx];
                string pSeq = seqList[pIdx];
                for (int k = 0; k < pSeq.Length; k++) {
                    char c = pSeq[k];
                    if (!P.ContainsKey(c))
                        continue;
                    int cirIdx = k % L;
                    if ((k / L) % 2 == 1)
                        cirIdx = L - 1 - cirIdx;
                    double[] R_k = tM[cirIdx];
                    foreach (int idx in P[c]) {
                        int col0 = idx * N;
                        for (int col = 0; col < N; col++)
                            pVector[col0 + col] += R_k[col];
                    }
                }
            }
            return New.NumberTable(vList);
        }


        public void FourierTrans(INumberTable tm, INumberTable dt, double[] R) {
            // DO matrix multiplication dt * tm where dt and tm are 
            // both column-set potentially with different number of rows.
            double[][] dtM = dt.Matrix as double[][];
            double[][] tmM = tm.Matrix as double[][];
            int L = dtM.Length;
            int Columns1 = dtM[0].Length;
            int Columns2 = tmM[0].Length;
            MT.Loop(0, Columns1, c1 => {
                for (int c2 = 0; c2 < Columns2; c2++) {
                    double v = 0.0;
                    for (int k = 0; k < L; k++) {
                        int k1 = (k < L) ? k : (2 * L - 1 - k);
                        int k2 = k % tmM.Length;
                        v += dtM[k1][c1] * tmM[k2][c2];
                    }
                    R[c1 * Columns2 + c2] = v;
                }
            });
        }

        public void RowDifferentiation(INumberTable dt) {
            if (dt.Rows == 1) {
                var R = dt.Matrix[0];
                R[0] = R[1] = R[2] = 0;
                return;
            }
            double[][] M = dt.Matrix as double[][];
            int rows = M.Length;
            for (int r = 1; r < rows; r++)
                for (int c = 0; c < 3; c++)
                    M[r - 1][c] = M[r][c] - M[r - 1][c];
            Array.Resize(ref M, M.Length - 1);
            dt.RowSpecList.RemoveAt(M.Length - 1);
        }

        public INumberTable MFVectorize(IList<string> seqList, string aaGroups, int L) {
            var P = Cluster2IdxList(aaGroups);
            int clusters = P.Values.Max(vs => vs.Max()) + 1;
            double[][] vList = new double[seqList.Count][];
            MT.Loop(0, seqList.Count, pIdx => {
                string pSeq = seqList[pIdx];
                double[] pVector = vList[pIdx] = new double[clusters * L];
                int secLen = pSeq.Length / L;
                int tailIdx = pSeq.Length % L;   // where the tail sections begins. Tail sections are shorter by one point.
                int headSize = tailIdx * L;      // The size in aa of the head section, where section size is L+1.
                if (pSeq.Length < L)
                    headSize = pSeq.Length;
                for (int k = 0; k < pSeq.Length; k++) {
                    char c = pSeq[k];
                    if (P.ContainsKey(c)) {
                        // secIdx is the index of section where k-th aa is in.
                        int secIdx = (k < headSize) ? k / (secLen + 1) : (k - tailIdx) / secLen;
                        //int secIdx = (7919*k) % L;
                        //int secIdx = k % L;
                        secIdx *= clusters;
                        foreach (int cIdx in P[c])
                            pVector[secIdx + cIdx] += 1.0;
                    }
                }
            });
            return New.NumberTable(vList);
        }

        public void MeanFieldTrans(INumberTable dt, double[] R) {
            int L = R.Length / 3; // number of sections
            int secLen = dt.Rows / L;  // section length
            int tailIdx = dt.Rows % L;   // where the tail sections begins. Tail sections are shorter by one point.
            int headSize = tailIdx * L;      // The size in aa of the head section, where section size is L+1.
            if (dt.Rows < L)
                headSize = dt.Rows;
            Array.Clear(R, 0, R.Length);
            for (int k=0; k<dt.Rows; k++) {
                double[] Mrow = dt.Matrix[k] as double[];
                // secIdx is the index of section where k-th aa is in.
                int secIdx = (k < headSize) ? k / (secLen + 1) : (k - tailIdx) / secLen;
                //int secIdx = k % L;
                secIdx *= 3;
                R[secIdx]     += Mrow[0];
                R[secIdx + 1] += Mrow[1];
                R[secIdx + 2] += Mrow[2];
            }
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

        public unsafe double NWDistance(string sA, string sB, short gapCost = 2, short matchCost = 0, short mismatchCost = 1) {
            int rows = sA.Length;
            int cols = sB.Length;
            if (rows == 0) return cols * gapCost;
            if (cols == 0) return rows * gapCost;
            int* A = stackalloc int[cols + 1];   // this is much faster than the new short[] call.
            int* B = stackalloc int[cols + 1];

            for (int col = 0; col <= cols; col++) A[col] = col * gapCost;
            for (int row = 1; row <= rows; row++) {
                B[0] = row * gapCost;
                for (int col = 1; col <= cols; col++) {
                    B[col] = Math.Min(Math.Min(
                        A[col] + gapCost, B[col - 1] + gapCost),
                        A[col - 1] + ((sA[row - 1] == sB[col - 1]) ? matchCost : mismatchCost));
                }
                int* tmp = A; A = B; B = tmp;
            }
            return A[cols];
        }

        public List<IBody> ToSphere(List<IBody> bList, double fct=1.0) {
            for (int k = 0; k < (bList.Count - 1); k++) {
                var b = bList[k];
                var b1 = bList[k + 1];
                b.SetXYZ(b1.X - b.X, b1.Y - b.Y, b1.Z - b.Z);
            }
            bList.RemoveAt(bList.Count - 1);

            if (fct != 0)
                ShrikSphere(bList, (float)fct);

            return bList;
        }

        public void ShrikSphere(List<IBody> bList, float fct) {
            Vector3[] P = new Vector3[bList.Count];
            for (int k = 0; k < bList.Count; k++) {
                P[k] = bList[k].ToV3();
                P[k].Normalize();
            }

            Quaternion T = Quaternion.Identity;
            for (int k = 1; k < bList.Count; k++) {
                Vector3 axis = Vector3.Cross(P[k], P[k-1]);
                double angle = Math.Acos(Vector3.Dot(P[k - 1], P[k]));
                var Q = Quaternion.RotationAxis(axis, (float)(fct * angle));
                T = T * Q;
                var p = bList[k].ToV3();
                bList[k].SetXYZ(Vector3.Transform(p, T));
            }
        }

        #region LoadCif() method
        string pdbTitle = null;
        List<IBody> heteroChains = null;
        Dictionary<string, string> acc2chain = null;

        public List<IBody> LoadCif(string fileName, List<string> chainNames) {
            List<IBody> bList = null;
            HashSet<int> betaSet = new HashSet<int>();
            HashSet<int> helixSet = new HashSet<int>();
            using (TextReader tr = new StreamReader(fileName)) {
                string L = tr.ReadLine();
                if (!L.StartsWith("data_"))
                    return null;
                while (true) {
                    L = tr.ReadLine();
                    if (L == null)
                        break;
                    if (L.StartsWith("_struct_sheet_range.end_auth_seq_id")) {
                        LoadBetaSheet(tr, betaSet);
                    } else if (L.StartsWith("_struct_conf.pdbx_PDB_helix_length")) {
                        LoadHelix(tr, helixSet);
                    } else if (L.StartsWith("_struct_conf.conf_type_id")) {
                        if (L.TrimEnd().EndsWith("HELX_P"))
                            LoadHelix2(tr, helixSet);
                    } else if (L.StartsWith("_struct.title")) {
                        pdbTitle = GetPDBTitle(L, tr);
                    } else if (L.StartsWith("_struct_ref_seq.align_id")) {
                        acc2chain = GetAcc2Chain(tr);
                    } else if (L.StartsWith("_atom_site.")) {
                        bList = LoadAtoms(tr, helixSet, betaSet, chainNames);
                        break;
                    }
                }
            }
            return bList;
        }

        void LoadBetaSheet(TextReader tr, HashSet<int> betaSet) {
            while(true) {
                string L = tr.ReadLine();
                if (L[0] == '#')
                    break;
                string[] fs = L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                int idx0 = int.Parse(fs[4]) - 1;
                int idx1 = int.Parse(fs[8]) + 1;
                for (int i = idx0; i < idx1; i++)
                    betaSet.Add(i);
            }
        }

        void LoadHelix(TextReader tr, HashSet<int> helixSet) {
            while (true) {
                string L = tr.ReadLine();
                if (L[0] == '#')
                    break;
                if (L[0] == ';')
                    continue;
                string[] fs = L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (fs.Length < 10)
                    continue;
                int idx0 = int.Parse(fs[5]);
                int idx1 = int.Parse(fs[9]) + 1;
                for (int i = idx0; i < idx1; i++)
                    helixSet.Add(i);
            }
        }

        void LoadHelix2(TextReader tr, HashSet<int> helixSet) {
            int idx0 = -1;
            int idx1 = -1;
            while (true) {
                string L = tr.ReadLine();
                if (L[0] == '#')
                    break;
                string[] fs = L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (L.StartsWith("_struct_conf.beg_label_seq_id"))
                    idx0 = int.Parse(fs[1]);
                if (L.StartsWith("_struct_conf.end_label_seq_id"))
                    idx1 = int.Parse(fs[1]) + 1;
            }
            if ( (idx0>=0) && (idx1>=0) ) {
                for (int i = idx0; i < idx1; i++)
                    helixSet.Add(i);
            }

        }

        Dictionary<string, string> GetAcc2Chain(TextReader tr) {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            while (true) {
                string L = tr.ReadLine();
                if (L[0] == '#')
                    break;
                if (L[0] == '_')
                    continue;
                string[] fs = L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (!dict.ContainsKey(fs[8]))
                    dict[fs[8]] = fs[3];
            }
            return dict;
        }

        string GetPDBTitle(string L, TextReader tr) {
            int idx = L.IndexOf('\'');
            if (idx<0) {
                L = tr.ReadLine();
                if (L[0] == ';')
                    return L.Substring(1).Trim();
                else if (L[0] == '\'')
                    return L.Trim().Trim('\'');
                else
                    return "";
            }
            string s = L.Substring(idx).Trim().Trim('\'');
            return s;
        }

        public string GetTitle() {
            return this.pdbTitle;
        }

        public List<IBody> GetHeteroChains() {
            return heteroChains;
        }

        public Dictionary<string, string> GetAccession2ChainTable() {
            return acc2chain;
        }

        Dictionary<string, char> P3 = new Dictionary<string, char>()
        {
            {"ALA", 'A' },
            {"ARG", 'R' },
            {"ASN", 'N' },
            {"ASP", 'D' },
            {"CYS", 'C' },
            {"GLU", 'E' },
            {"GLN", 'Q' },
            {"GLY", 'G' },
            {"HIS", 'H' },
            {"ILE", 'I' },
            {"LEU", 'L' },
            {"LYS", 'K' },
            {"MET", 'M' },
            {"PHE", 'F' },
            {"PRO", 'P' },
            {"SER", 'S' },
            {"THR", 'T' },
            {"TRP", 'W' },
            {"TYR", 'Y' },
            {"VAL", 'V'}
        };

        const int maxChainIndex = 144; // 4x36 of "36 Clusters" 
        List<IBody> LoadAtoms(TextReader tr, HashSet<int> helixSet, HashSet<int> betaSet, List<string> chainNames) {
            Dictionary<string, int> ch2idx = new Dictionary<string, int>() {
                { "HOH", maxChainIndex + 3 },
                { "NAG", maxChainIndex + 11 } } ;
            int headIndex = 0;
            int Lookup(string chName) {
                if (!ch2idx.ContainsKey(chName)) {
                    for (int k = headIndex; k < 200; k++) {
                        if (!ch2idx.ContainsValue(k)) {
                            ch2idx[chName] = k;
                            headIndex = k + 1;
                            break;
                        }
                    }
                    if (!ch2idx.ContainsKey(chName))
                        ch2idx[chName] = 200;
                }
                return ch2idx[chName];
            }

            List<IBody> bsList = vv.New.BodyList();
            List<IBody> bsList2 = vv.New.BodyList();
            int rsIdxPre = -1;
            var RNA_set = new HashSet<string>() { "A", "U", "G", "C" };
            var DNA_set = new HashSet<string>() { "DA", "DT", "DG", "DC" };
            char[] fSeparator = new char[] { ' ' };
            char[] dbQuoats = new char[] { '"' };

            while (true) {
                string L = tr.ReadLine();
                if ( (L == null) || (L[0] == '#') )
                    break;
                if (L[0] == '_')
                    continue;
                string[] fs = L.Split(fSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (fs.Length < 21) {
                    vv.Message("Invalid record: " + fs.Length + ": |" + L + "|");
                    return null;
                }
                string chName = fs[18] + "_" + fs[20];
                string atName = fs[3].Trim(dbQuoats);
                string rsName = fs[5];
                string secType = "x";
                string p1 = "x";
                string bId = null;

                if (fs[0] == "ATOM") {
                    int rsIdx = int.Parse(fs[8]) - 1;
                    if (rsIdx == rsIdxPre)
                        continue;
                    if (P3.ContainsKey(rsName) && ((atName == "CA") || (atName == "C2"))) {
                        p1 = P3[rsName].ToString();
                        if (helixSet.Contains(rsIdx))
                            secType = "h";
                        else if (betaSet.Contains(rsIdx))
                            secType = "b";
                    } else if (RNA_set.Contains(rsName) && atName.StartsWith("P") ) {
                        p1 = "r";
                    } else if (DNA_set.Contains(rsName) && atName.StartsWith("C1") ) {
                        //
                        // DNA strands normally exists in pair in form of a double-helix. The atom C1' is 
                        // located more towards the center of the helix compared to the P atom. So, using C1'
                        // atoms as chain-sampling-points makes the two helix strands close to another; and
                        // further and therefor less entangled with the sourranding atoms.
                        //
                        rsName = rsName[1].ToString();
                        p1 = "d";
                    } else
                        continue;
                    bId = $"A{rsIdx}.{bsList.Count}";
                    rsIdxPre = rsIdx;
                } else if (fs[0] == "HETATM") {
                    bId = $"H.{fs[3]}.{bsList2.Count}";
                    p1 = fs[3];
                } else
                    continue;

                IBody b = vv.New.Body(bId);
                b.X = float.Parse(fs[10]);
                b.Y = float.Parse(fs[11]);
                b.Z = float.Parse(fs[12]);

                b.Name = p1 + '.' + rsName + '.' + chName + '.' + secType;
                b.Type = (short)Lookup(chName);

                if (b.Id[0] == 'H') {
                    if (ch2idx.ContainsKey(rsName))
                        b.Type = (short)Lookup(rsName);
                    else
                        b.Type = maxChainIndex + 25;
                    bsList2.Add(b);
                } else {
                    if ( (b.Name[0] == 'r') || (b.Name[0] == 'd') )
                        b.Hidden = true;
                    bsList.Add(b);
                }
            }

            if ( chainNames != null) {
                HashSet<string> selectedChains = new HashSet<string>(chainNames);
                bsList = bsList.Where(b => selectedChains.Contains(b.Name.Split('.')[2])).ToList();
                bsList2 = bsList2.Where(b => selectedChains.Contains(b.Name.Split('.')[2])).ToList();
            }

            heteroChains = bsList2;
            return bsList;
        }
#endregion
    }
}
