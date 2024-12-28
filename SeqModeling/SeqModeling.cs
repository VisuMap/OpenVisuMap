using System;
using System.Linq;
using System.Collections.Generic;
using VisuMap.Script;
using Vector3 = System.Numerics.Vector3;
using System.IO;

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

        public void LocalSmoothen2(double[][] M, double smoothenRatio, int repeats = 8) {
            if ((M == null) || (M.Length < 3) || (M[0].Length != 3) || (repeats <= 0))
                return;

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
        }

        public INumberTable PcaNormalize(INumberTable nt, int waveLen) {
            // perform PCA rotation
            nt = nt.DoPcaReduction(3);

            // shifting origine to the first point
            double x0 = nt.Matrix[0][0];
            double y0 = nt.Matrix[0][1];
            double z0 = nt.Matrix[0][2];
            for (int k = 0; k < nt.Rows; k++) {
                var R = nt.Matrix[k];
                R[0] -= x0;
                R[1] -= y0;
                R[2] -= z0;
            }
            // collapsing nt to {waveLen} rows
            if (nt.Rows > waveLen) {
                for(int row=waveLen; row<nt.Rows; row++) {
                    var R1 = nt.Matrix[row];
                    int cirIdx = row % waveLen;
                    if ((row / waveLen) % 2 == 1)
                        cirIdx = waveLen - 1 - cirIdx;
                    var R2 = nt.Matrix[cirIdx];
                    R2[0] += R1[0];
                    R2[1] += R1[1];
                    R2[2] += R1[2];
                }
                nt.RemoveRows(Enumerable.Range(waveLen, nt.Rows).ToList());
            }
            return nt;
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
                        Body b = new Body("i" + b0.Type + '.' + i);
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
                string[] fs = L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
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

        List<IBody> LoadAtoms(TextReader tr, HashSet<int> helixSet, HashSet<int> betaSet, List<string> chainNames) {
            Dictionary<string, int> ch2idx = new Dictionary<string, int>() {
                { "HOH", 72 + 3 },
                { "NAG", 72 + 11 } } ;
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
                    } else if (RNA_set.Contains(rsName) && (atName == "C1")) {
                        p1 = "r";
                    } else if (DNA_set.Contains(rsName) && (atName == "C1")) {
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
                        b.Type = 72 + 25;
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
