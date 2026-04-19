using System;
using System.Linq;
using System.Collections.Generic;
using Vector3 = SharpDX.Vector3;
using Quaternion = SharpDX.Quaternion;
using MathNet.Numerics.Interpolation;
using VisuMap.Clustering;
using VisuMap.Script;

namespace VisuMap {
    public partial class SeqModeling {
        IVisuMap vv;
        INew New;
        const double BOND_LENGTH = 3.8015;  // Average bond length. with std ca 0.1

        public SeqModeling() {
            this.vv = VisuMapImp.GetVisuMapImp();
            this.New = vv.New;
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

        public void NormalizeByFlipping(List<IBody> bodyList) {
            int N = bodyList.Count;
            if (N < 2)
                return;
            double xm = bodyList.Select(b => b.X).Average();
            double ym = bodyList.Select(b => b.Y).Average();
            double zm = bodyList.Select(b => b.Z).Average();

            double x = 0;
            double y = 0;
            double z = 0;
            foreach (var b in bodyList.Take(N / 2)) {
                x += b.X - xm;
                y += b.Y - ym;
                z += b.Z - zm;
            }
            bool flipX = (x > 0);
            bool flipY = (y > 0);
            bool flipZ = (z > 0);

            foreach (var b in bodyList) {
                if (flipX)
                    b.X = 2 * xm - b.X;
                if (flipY)
                    b.Y = 2 * ym - b.Y;
                if (flipZ)
                    b.Z = 2 * zm - b.Z;
            }
        }

        public List<IBody> NormalizeChain(List<IBody> bList, int chType = 1, bool hidIntp = false) {
            foreach (var b in bList) {
                if (chType >= 0)
                    b.Type = (short)chType;
                if (hidIntp)
                    b.Hidden = b.Id[0] == 'i';
            }
            const short SeqMap_HEAD = 158;
            const short SeqMap_TAIL = 171;
            bList[0].Type = SeqMap_HEAD;
            for(int k=0; k<bList.Count-1; k++) {
                IBody b0 = bList[k];
                IBody b1 = bList[k + 1];
                if (b0.Name.Split('.')[2] != b1.Name.Split('.')[2]) {
                    b0.Type = SeqMap_TAIL;
                    b1.Type = SeqMap_HEAD;
                }
            }
            bList[bList.Count - 1].Type = SeqMap_TAIL;
            return bList;
        }
     

        public void CenteringBodyList(IList<IBody> bList, double cx, double cy, double cz) {
            if (bList.Count < 1)
                return;
            double xm = bList.Select(b => b.X).Average();
            double ym = bList.Select(b => b.Y).Average();
            double zm = (cz != 0) ? bList.Select(b => b.Z).Average() : 0;
                
            double dx = cx - xm;
            double dy = cy - ym;
            double dz = cz - zm;

            foreach (var b in bList) {
                b.X += dx;
                b.Y += dy;
                if (cz != 0)
                    b.Z += dz;
            }
        }

        // Flipping centralized matrix.
        void FlipNormalize(double[][] M) {
            int rows = M.Length;
            int cols = M[0].Length;
            double[] colSum = new double[cols];
            for(int row=0; row<(rows/2); row++) {
                var R = M[row];
                for(int col=0; col<cols; col++)
                    colSum[col] += R[col];
            }
            /*
            foreach(int row in new int[] { 0, rows-1 }) {
                var R = M[row];
                for (int col = 0; col < cols; col++)
                    colSum[col] += 10*R[col];
            }
            */
            bool[] flipColumn = colSum.Select(c => c > 0).ToArray();

            foreach (var R in M) { 
                for (int col = 0; col < cols; col++)
                    if ( flipColumn[col] )
                            R[col] = -R[col];
            }
        }

        // Unlike PcaNormalize2D() and PcaNormalize3D(), this function only allows even number of flippings
        // So that the rotation matrix is always positive, e.g. with positive determinantes. This method
        // is intended to normalize raw 3D information from PDB files, whereas PcaNormalize2/3D() are intended
        // to normalize t-SNE outputs.
        public INumberTable PcaNormalizePositive(INumberTable nt) {
            if (nt.Rows <= 3) 
                return nt;
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

            // Flipping X, Y and Z in SU(3), i.e. with a positive rotation matrix.
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

            return nt;
        }

        public IMapSnapshot FitByPCA(IMapSnapshot map, double scale) {
            if (map == null)
                return map;
            List<IBody> bs = map.BodyListEnabled() as List<IBody>;
            if (bs.Count <= 1)
                return map;

            double[][] M = bs.Select(b => new double[] { b.X, b.Y }).ToArray();
            MathUtil.CenteringInPlace(M);
            double[][] E = MathUtil.DoPca(M, 2);

            if ((E.Length < 2) || (E[0].Length < 2))
                return map;

            MT.ForEach(M, R => {
                double x = R[0] * E[0][0] + R[1] * E[0][1];
                double y = R[0] * E[1][0] + R[1] * E[1][1];
                R[0] = x;
                R[1] = y;
            });

            FlipNormalize(M);

            // Scale and shift the map
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = -minX;
            double maxY = -minY;
            for (int k = 0; k < bs.Count; k++) {
                var b = bs[k];
                b.X = M[k][0];
                b.Y = M[k][1];
                minX = Math.Min(minX, b.X);
                maxX = Math.Max(maxX, b.X);
                minY = Math.Min(minY, b.Y);
                maxY = Math.Max(maxY, b.Y);
            }
            const int margin = 5;
            MT.ForEach(bs, b => {
                b.X = scale * (b.X - minX) + margin;
                b.Y = scale * (b.Y - minY) + margin;
            });
            var cSz = new System.Drawing.Size(
                (int)(scale * (maxX - minX) + 2 * margin), 
                (int)(scale * (maxY - minY) + 2 * margin));
            map.TheForm.ClientSize = cSz;
            map.MapLayout.Width = cSz.Width;
            map.MapLayout.Height = cSz.Height;
            return map;
        }

        public double[] SmoothenSeries(double[] vs) {
            int L = vs.Length;
            double pv = vs[0];
            vs[0] = 0.5 * (pv + vs[0]);
            for (int k = 1; k < L - 1; k++) {
                double vk = (pv + vs[k] + vs[k + 1]);
                pv = vs[k];
                vs[k] = vk;
            }
            vs[L - 1] = 0.5 * (pv + vs[L - 1]);
            return vs;
        }

        public INumberTable MovingWindowFT(IList<string> pList, IList<int> wsList, INumberTable tm, int intRp = 0, string cacheDir=null) {
            List<IBody> bList = vv.Dataset.BodyListForId(pList) as List<IBody>;
            INumberTable D = New.NumberTable(bList, tm.Columns * wsList.Count);
            const double EPS = 0.085;
            const double rRNA_AA = 44 / 14.0;
            MT.LoopNoblocking(0, pList.Count, k => {
                string pId = pList[k];
                var bs = LoadChain3D($"{cacheDir}/{pList[k]}.pmc");
                if (intRp > 0)
                    bs = Interpolate3D(bs, intRp, EPS, bs.Count, 0);
                int idx0 = 0;
                bool isNucleotide = vv.Dataset.StringAt(pId, 0)[0] != 'A';
                double[] Rk = (double[])D.Matrix[k];
                double[] bDist = new double[bs.Count];
                foreach (int n in wsList) { 
                    int ws = isNucleotide ? (int)(n * rRNA_AA) : n;
                    if (ws < bs.Count) {
                        MovingWindowVariance(bs, ws, bDist);
                        VectorizeChainFT(bDist, tm, Rk, idx0);
                        idx0 += tm.Columns;
                    }
                }
                if ((k > 0) && (k % 500 == 0)) {
                    vv.Title = $"Reading chains: {k} of {pList.Count}";
                }
            });
            return D;
        }

        public INumberTable BondGapeSpetrum(IList<string> pList, INumberTable tm=null, string cacheDir = null) {
            List<IBody> bList = vv.Dataset.BodyListForId(pList) as List<IBody>;
            double[][] M = new double[bList.Count][];
            MT.LoopNoblocking(0, pList.Count, k => {
                var bs = LoadChain3D($"{cacheDir}/{pList[k]}.pmc");
                double[] L = new double[bs.Count-1];
                for (int i = 0; i < bs.Count - 1; i++)
                    L[i] = bs[i + 1].DistanceTo(bs[i]) - BOND_LENGTH;
                M[k] = L;
                if ((k > 0) && (k % 500 == 0)) {
                    vv.Title = $"Reading chains: {k} of {pList.Count}";
                }
            });

            if (tm == null) {
                int columns = M.Select(L => L.Length).Max();
                INumberTable D = New.NumberTable(bList, columns);
                for(int row=0; row<pList.Count; row++) 
                    Array.Copy(M[row], D.Matrix[row] as double[], M[row].Length);
                if ( pList.Count == 1) {
                    var bs = LoadChain3D($"{cacheDir}/{pList[0]}.pmc");
                    for (int k = 0; k < D.Columns; k++)
                        D.ColumnSpecList[k].CopyFromBody( bs[k+1] );
                }
                return D;
            } else {
                INumberTable D = New.NumberTable(bList, tm.Columns);
                MT.LoopNoblocking(0, pList.Count, k => {
                    VectorizeChainFT(M[k], tm, D.Matrix[k] as double[], 0);
                });
                return D;
            }
        }


        public void VectorizeChainFT(double[] bDist, INumberTable tm,  double[] R, int index0 = 0) {
            double[][] M = tm.Matrix as double[][];
            int L = bDist.Length;
            MT.Loop(0, M[0].Length, col=>{
                double v = 0.0;
                for (int k = 0; k < L; k++) {
                    int k1 = (k < L) ? k : (2 * L - 1 - k);
                    v += bDist[k1] * M[k % M.Length][col];
                }
                R[col + index0] = v;
            });
        }

        public List<IBody> InterpolateETC(List<IBody> bList, int rp = 3, bool hidIntp = false,
                string pId = null,  bool setChainId = false, bool typeByChainIdx=false, bool unifyId=false, bool matchPid=false) {
            string ChainName(IBody body) { return body.Name.Split('.')[2]; }

            double EPS = 0.085;
            List<IBody> bs = New.BodyList();
            int k0 = 0;
            string t0 = ChainName(bList[0]);
            int chIdx = 0;
            for (int k = 0; k <= bList.Count; k++) {
                if ((k == bList.Count) || (ChainName(bList[k]) != t0)) {
                    List<IBody> P0 = bList.GetRange(k0, k - k0);
                    List<IBody> P1 = Interpolate3D(P0, rp, EPS, bs.Count, chIdx);
                    chIdx += 1;
                    bs.AddRange(P1);
                    if (k < bList.Count) {
                        k0 = k;
                        t0 = ChainName(bList[k0]);
                    }
                }
            }
            if (hidIntp)
                foreach (var b in bs)
                    b.Hidden = (b.Id[0] == 'i');

            if (matchPid) {
                // set the pid to match those in the p-map
                string pId4 = pId.Substring(0, 4);

                Dictionary<string, string> ch2id = new Dictionary<string, string>();
                var ds = vv.Dataset;
                var allBodies = ds.BodyList;
                for(int row=0; row< allBodies.Count; row++) {
                    IBody b = allBodies[row];
                    if ( b.Id.StartsWith(pId4) ) {
                        string chName = ds.StringAt(row, 4);
                        if ( (chName != null) && ! ch2id.ContainsKey(chName) )
                            ch2id.Add(chName, b.Id);
                    }                    
                }

                if (bs.All( b=>ch2id.ContainsKey(ChainName(b)) )) {
                    // All chains are present in current dataset, e.g. ch2id[].
                    string curChName = null;
                    string curChId = null;
                    foreach (IBody b in bs) {
                        if (b.Id[0] == 'A') {
                            string chName = ChainName(b);
                            if (chName != curChName) {
                                curChName = chName;
                                curChId = ch2id[chName];
                            }
                        }
                        b.Id = curChId;
                    }
                } else {
                    // Some chains are not present in current dataset, e.g. ch2id[].
                    // We fall back to using chain indexes for chain id.
                    chIdx = -1;
                    pId4 += '_';
                    string curChId = null;
                    string curChName = null;
                    foreach (IBody b in bs) {
                        if (b.Id[0] == 'A') {
                            string chName = ChainName(b);
                            if (chName != curChName) {
                                chIdx++;
                                curChName = chName;
                                curChId = pId4 + chIdx.ToString();
                            }
                        }
                        b.Id = curChId;
                    }
                }
                return bs;
            }

            if (setChainId && (pId != null)) {
                var ds = vv.Dataset;
                string chId = pId.Substring(0, 4);
                var chName2Id = ds.BodyList.Where(b => b.Id.StartsWith("chId")).Select(b => b.Id).ToDictionary(id => id.Substring(0, 4));
                foreach (var b in bList) {
                    string chName = ChainName(b);
                    // notice some chains maybe exact repeats so that they not in the dataset table, but
                    // accounted in the Repeats columns. 
                    if (chName2Id.ContainsKey(chName))
                        b.Id = chName2Id[chName];
                    else
                        b.Id = chId + chName;
                }

                // set the id of interpolated items
                string curId =bs[0].Id;
                foreach(var b in bs) {
                    if ( b.Id[0] == 'i') {
                        b.Id = curId;
                    } else {
                        curId = b.Id;
                    }
                }
            }

            if (typeByChainIdx) {
                var chName2Type = new Dictionary<string, short>();

                foreach (var b in bList) {
                    string chName = ChainName(b);
                    if (!chName2Type.ContainsKey(chName))
                        chName2Type[chName] = (short)chName2Type.Count;
                    b.Type = chName2Type[chName];

                    if (unifyId) //assign the chain id to all members
                        b.Id = pId.Substring(0, 4) + '_' + b.Type;
                }

                // set the id and type of interpolated items
                string curId = bs[0].Id;
                short curType = bs[0].Type;
                foreach (var b in bs) {
                    if (b.Id[0] == 'i') {
                        b.Id = curId;
                        b.Type = curType;
                    } else {
                        curId = b.Id;
                        curType = b.Type;
                    }
                }
            }
            return bs;
        }


        public List<IBody> Interpolate3D(List<IBody> bList, int repeats, double convexcity, int bIdx0, int chIdx) {
            if ((bList.Count <= 1) || (repeats == 0))
                return bList;
            int L = 1 << repeats;
            int N = bList.Count;
            int iN = (N - 1) * L + 1;
            List<IBody> bs = new List<IBody>();
            for (int n=0; n<(N-1); n++) {
                Body b0 = bList[n] as Body;
                bs.Add(b0);
                int rsIdx = 0;
                if ((b0.Id[0] == 'A') && (char.IsDigit(b0.Id[1])))
                    rsIdx = int.Parse(b0.Id.Split('.')[0].Substring(1));
                string secPrefix = "i" + chIdx + "." + rsIdx + ".";
                for (int k=0; k<(L-1); k++) {
                    Body b = new Body(secPrefix + k, b0.Name, b0.Type);
                    b.Flags = b0.Flags;
                    bs.Add(b);
                }
            }
            bs.Add(bList[N - 1]);

            double[] P = new double[N];
            double[] X = new double[N];
            double[] Y = new double[N];
            double[] Z = new double[N];
            for (int k = 0; k < N; k++) {
                P[k] = k;
                var b = bList[k];
                X[k] = b.X;
                Y[k] = b.Y;
                Z[k] = b.Z;
            }
            double dx = P[N - 1] / (iN - 1);
            var spX = CubicSpline.InterpolateNaturalSorted(P, X);
            var spY = CubicSpline.InterpolateNaturalSorted(P, Y);
            var spZ = CubicSpline.InterpolateNaturalSorted(P, Z);
            MT.Loop(0, iN, k => {
                IBody b = bs[k];
                if ( b.Id[0] == 'i' ) {
                    double p = dx * k;
                    b.SetXYZ(spX.Interpolate(p), spY.Interpolate(p), spZ.Interpolate(p));
                }
            });
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

        public List<string> FlatSampling(List<string> chIds, double minDist) {
            IList<IBody> bList = vv.Dataset.BodyListForId(chIds);
            KdTree2D kd = new KdTree2D(bList);
            HashSet<string> sampling = new HashSet<string>();
            for(int k=0; k<bList.Count; k++) {
                int[] nbIdxes = kd.FindNeighbors(bList[k], minDist, 1000);
                bool isSampled = nbIdxes.Any(idx => sampling.Contains(chIds[idx]));
                if (! isSampled )
                    sampling.Add(chIds[k]);
            }
            return sampling.ToList();
        }

        public void LoopSection(IMap3DView mp, int selLen = 25) {
            var bs = mp.BodyList;
            int L;
            for (L = 0; L < bs.Count; L++)
                if (bs[L].Id != bs[0].Id)
                    break;
            for (int idx = 0; idx < L; idx += 2) {
                string cId = "";
                int k = 0;
                int idx2 = idx + selLen;
                foreach (IBody b in bs) {
                    if (b.Id != cId) {
                        k = 0;
                        cId = b.Id;
                    } else
                        k++;
                    b.Hidden = !((k >= idx) && (k < idx2));
                }
                mp.Redraw();
                vv.Sleep(20);
                if (vv.ModifierKeys.ControlPressed)
                    break;
            }

            vv.Sleep(500);
            int maxType = bs.Select(b => b.Type).Max();
            for (int t = 0; t <= maxType; t++) {
                if (vv.ModifierKeys.ControlPressed)
                    break;
                foreach (var b in bs)
                    b.Hidden = (b.Type != t);
                mp.Redraw();
                vv.Sleep(500);
            }

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

        static Vector3[] MovingWindowMean0(Vector3[] P, int winSize) {
            if ((P == null) || (P.Length == 0) || (winSize < 0))
                return null;
            int L = P.Length - 1;   // number bonds in the polipetides.  
            winSize = Math.Min(L, winSize);
            Vector3[] M = new Vector3[L + 1];
            int WS = 2 * winSize + 1;
            Vector3 S = WS * P[0];  // the sum of current initial moving-window [-winSize, +winSize]
            float cf = (float)(1.0 / WS);

            Vector3 xP(int idx) {
                return (idx < 0) ? (2*P[0] - P[-idx]) : 
                       (idx > L) ? (2*P[L] - P[2*L - idx]) : P[idx];
            }
            for (int k = 1; k < L; k++) {  // k is the index of window center.
                S += xP(k + winSize) - xP(k - winSize - 1);
                M[k] = cf*S;
            }
            M[0] = P[0]; // fixed.
            M[L] = P[L]; // 
            return M;
        }

        public void MovingWindowMean(IList<IBody> bs, int winSize) {
            var P = bs.Select(b => b.ToV3()).ToArray();
            Vector3[] M = MovingWindowMean0(P, winSize);
            for (int k = 0; k < bs.Count; k++)
                bs[k].SetXYZ(M[k]);
        }

        public double[] MovingWindowVariance(IList<IBody> bs, int winSize, double[] varBuffer = null) {  
            var P = bs.Select(b => b.ToV3()).ToArray();
            P = MovingWindowMean0(P, winSize);
            Vector3[] M = MovingWindowMean0(P, winSize);
            if (M == null)
                return null;
            double[] varArray = (varBuffer == null) ? new double[bs.Count] : varBuffer;
            MT.Loop(0, varArray.Length, k => {
                varArray[k] = (P[k] - M[k]).LengthSquared();
            });
            return varArray;
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

        public void SmoothenSeq(List<IBody> bList, int winSize) {
            int i = 0;
            while (i < bList.Count - 1) {
                string chainId = bList[i].Name.Split('.')[2];
                int j = i + 1;
                for(j=i + 1; j<bList.Count; j++)
                    if (bList[j].Name.Split('.')[2] != chainId)
                        break;
                if ( (j-i) > 2) {
                    var bs = bList.GetRange(i, j - i);
                    var P = bs.Select(b => b.ToV3()).ToArray();
                    Vector3[] Q = MovingWindowMean0(P, winSize);
                    for (int k = 0; k < bs.Count; k++)
                        bs[k].SetXYZ(Q[k]);
                }
                i = j;
            }
        }


        public List<IBody> ToSphere(List<IBody> bList, double contracting = 0) {
            var newList = new List<IBody>();
            for (int k = 1; k < bList.Count; k++) {
                var b = bList[k-1];
                var b1 = bList[k];
                IBody body = new Body(b1.Id, b1.Type, b1.X - b.X, b1.Y - b.Y, b1.Z - b.Z);
                body.Name = b1.Name;
                newList.Add(body);
            }
            
            // Scale the points into the range (0, BOND_LENGTH).
            double fct = BOND_LENGTH / newList.Average(b => b.Length);
            foreach (var b in newList)
               b.Mult(fct);

            if (contracting != 0) {
                ShrinkSphere(newList, (float)contracting);
            }

            return newList;
        }


        public INumberTable ToTorsionList(List<IBody> bList, double mom = 0.95, double nbEps= 1.0) {
            int L = bList.Count;
            if (L < 7)
                return null;

            Vector3[] V = bList.Select(b => new Vector3((float)b.X, (float)b.Y, (float)b.Z)).ToArray();            
            double DD(int i, int j) => Vector3.Distance(V[i], V[j]);

            INumberTable nt = New.NumberTable(L, 9);
            for (int k = 0; k < L; k++)  nt.RowSpecList[k].CopyFromBody(bList[k]);
            nt.ColumnSpecList[0].Id = "CurvatureNb1";
            nt.ColumnSpecList[1].Id = "CurvatureNb2";
            nt.ColumnSpecList[2].Id = "CurvatureNb3";
            nt.ColumnSpecList[3].Id = "TorsionA";
            nt.ColumnSpecList[4].Id = "TorsionB";
            nt.ColumnSpecList[5].Id = "Bnd_LenRatio";
            nt.ColumnSpecList[6].Id = "Cnt_Bnd_Dist";
            nt.ColumnSpecList[7].Id = "NB_Size";
            nt.ColumnSpecList[8].Id = "NB_Cnt";

            for (int k = 1; k < L-1; k++) {
                double[] R = nt.Matrix[k] as double[];
                if ( (k>=1) && (k+1)<L )
                    R[0] = 2 * BOND_LENGTH / DD(k + 1, k - 1);
                if ((k >= 2) && (k + 2) < L)
                    R[1] = 2 * BOND_LENGTH / DD(k + 2, k - 2);
                if ((k >= 3) && (k + 3) < L)
                    R[2] = 3 * BOND_LENGTH / DD(k + 3, k - 3);
            }

            // Fill the zeros with neighboring value.
            double[][] M = nt.Matrix as double[][];
            for (int r=0; r<3; r++) 
            for(int k=0; k<=r; k++) {
                M[k][r] = M[r + 1][r];
                M[L - 1 - k][r] = M[L - 2 - r][r];
            }
            
            Vector3[] dV = new Vector3[L - 1];
            for (int k=0; k<L-1; k++)  {
                dV[k] = V[k + 1] - V[k];
                dV[k].Normalize();
            }

            MT.Loop(0, L - 3, k => {                
                float cosA = Vector3.Dot(dV[k + 1], dV[k]);
                Vector3 P0 = cosA * dV[k];
                Vector3 P1 = dV[k + 1] - P0;
                Vector3 P2 = dV[k + 2] - P0;
                P1.Normalize();
                P2.Normalize();
                M[k][3] = Math.Acos(cosA);
                M[k][4] = Math.Acos(Vector3.Dot(P2, P1));
            });

            for(int k=L-3; k<L; k++) {
                M[k][3] = M[L - 4][3];
                M[k][4] = M[L - 4][4];
            }

            for (int k = 1; k < L; k++) 
                M[k][5] = Vector3.Distance(V[k], V[k - 1]);

            double[] vs = GlobeDistances(bList, mom);
            for (int k = 0; k < L; k++)
                M[k][6] = vs[k];


            var kd = new VisuMap.Clustering.KdTree3D(bList);
            const int BL = 7;
            for (int k = 0; k < L; k++) {
                int[] nbs = kd.FindNeighbors(bList[k], BL*BOND_LENGTH, 1000);
                vs[k] = (nbs.Length - 2*BL)/(0.5*BL*BL);
            }
            SmoothenSeries(vs);
            for (int k = 0; k < L; k++)
                M[k][7] = vs[k];

            KdTree3D kd3d = new VisuMap.Clustering.KdTree3D(bList);
            double nbDist = 5.25 * BOND_LENGTH;
            const int maxNB = 30;
            for (int k = 1; k < L; k++) {
                int[] nbList = kd3d.FindNeighbors(bList[k], nbDist, maxNB);
                double x = 0;
                double y = 0;
                double z = 0;
                int n = 0;
                foreach (int i in nbList) {
                    if ((i < k) && (i > 0)) {
                        IBody b = bList[i];
                        x += b.X;
                        y += b.Y;
                        z += b.Z;
                        n++;
                    }
                }
                if (n > 0) {
                    IBody b = bList[k];
                    x = x / n - b.X;
                    y = y / n - b.Y;
                    z = z / n - b.Z;
                    M[k][8] = x * x + y * y + z * z;
                }
            }

            double[] cMax = new double[nt.Columns];
            for(int row=0; row<nt.Rows; row++)
                for (int col = 0; col < nt.Columns; col++)
                    cMax[col] = Math.Max(cMax[col], nt.Matrix[row][col]);
            for (int row = 0; row < nt.Rows; row++) 
                for (int col = 0; col < nt.Columns; col++)
                    nt.Matrix[row][col] /= cMax[col];

            return nt;
        }

        public double[] GlobeDistances(IList<IBody> bList, double mom = 0.99) {
            int L = bList.Count-1;
            double[] vs = new double[L];
            double g = 1.0f - mom;
            IBody mp = bList[0].Clone(); // The mean-point.
            for (int k =0; k < L; k++) {
                IBody b = bList[k+1];
                vs[k] = b.DistanceSquared(mp);
                mp.X = mom * mp.X + g * b.X;
                mp.Y = mom * mp.Y + g * b.Y;
                mp.Z = mom * mp.Z + g * b.Z;
            }
            //MT.Loop(0, L, k => vs[k] = Math.Sqrt(vs[k]));
            MT.Loop(0, L, k => vs[k] = 1.0/(1.0+vs[k]));
            //MT.Loop(0, L, k => vs[k] = 1.0 / Math.Sqrt(vs[k]));
            return vs;
        }

        public void ToSphere(INumberTable nt, double fct = 0.0) {
            for (int k = 0; k < (nt.Rows - 1); k++) {
                double[] R0 = nt.Matrix[k] as double[];
                double[] R1 = nt.Matrix[k + 1] as double[];
                for (int col = 0; col < 3; col++)
                    R0[col] = R1[col] - R0[col];
            }
            nt.RemoveRows(new List<int>() { nt.Rows - 1 });
            if (fct != 0.0)
                ShrinkSphere(nt, fct);
        }

        public void ShrinkSphere(INumberTable nt, double fct) {
            Vector3[] P = new Vector3[nt.Rows];
            for (int k = 0; k < nt.Rows; k++) {
                P[k] = nt.Matrix[k].ToV3();
                P[k].Normalize();
            }
            Quaternion T = Quaternion.Identity;
            for (int k = 1; k < nt.Rows; k++) {
                Vector3 axis = Vector3.Cross(P[k], P[k - 1]);
                double angle = Math.Acos(Vector3.Dot(P[k - 1], P[k]));
                var Q = Quaternion.RotationAxis(axis, (float)(fct * angle));
                T = T * Q;
                var p = nt.Matrix[k].ToV3();
                nt.Matrix[k].SetXYZ(Vector3.Transform(p, T));
            }
        }

        public void ShrinkSphere(List<IBody> bList, float fct) {
            Quaternion T = Quaternion.Identity;
            Vector3 P0 = Vector3.Normalize( bList[0].ToV3() );
            for (int k = 1; k < bList.Count; k++) {
                Vector3 P1 = Vector3.Normalize(bList[k].ToV3());
                Vector3 axis = Vector3.Cross(P1, P0);
                float angle = (float) ( fct * Math.Acos(Vector3.Dot(P1, P0)) );
                T = T * Quaternion.RotationAxis(axis, angle);
                bList[k].SetXYZ(Vector3.Transform(bList[k].ToV3(), T));
                P0 = P1;
            }
        }
        public List<IBody> TorsionUnfold(List<IBody> bList, double contracting) {
            if (contracting == 0)
                return bList;
            var newList = New.BodyListClone(bList);
            Vector3 P = bList[1].ToV3();
            Vector3 S0 = Vector3.Normalize(P - bList[0].ToV3());
            Quaternion T = Quaternion.Identity;
            for (int k = 2; k < bList.Count; k++) {
                IBody b2 = bList[k];
                IBody b1 = bList[k - 1];
                Vector3 B = new Vector3((float)(b2.X - b1.X), (float)(b2.Y - b1.Y), (float)(b2.Z - b1.Z));
                Vector3 S1 = Vector3.Normalize(B);
                Vector3 axis = Vector3.Cross(S1, S0);
                float angle = (float)(contracting * Math.Acos(Vector3.Dot(S0, S1)));
                T = T * Quaternion.RotationAxis(axis, angle);
                P += Vector3.Transform(B, T);
                newList[k].SetXYZ(P);
                S0 = S1;
            }
            return newList;
        }

        public void RotateBodyList(List<IBody> bList, float angle, float dx, float dy, float dz ) {
            Quaternion T = Quaternion.RotationAxis(new Vector3(dx, dy, dz), angle);
            foreach(var b in bList) 
                b.SetXYZ(Vector3.Transform(b.ToV3(), T));
        }

        #region Miscellaneous

        public List<IBody> NewHelix(int residues) {
            double angleSpeed = 2 * Math.PI / 3.6;
            double R = 5.4 / 2;
            double dx = 1.5;  // Translation per residue
            List<IBody> bList = New.BodyList();
            for (int k = 0; k < residues; k++) {
                IBody b = New.Body("A" + k);
                b.Type = 1;
                b.Name = "..Hlx";
                double a = -k * angleSpeed;
                b.SetXYZ(k * dx, R * Math.Sin(a), R * Math.Cos(a));
                bList.Add(b);
            }
            return bList;
        }

        public int MaxChainLen(List<IBody> bList) {
            Dictionary<string, int> chainLen = new Dictionary<string, int>();
            foreach (var b in bList) {
                string chName = b.Name.Split('.')[2];
                if (chainLen.ContainsKey(chName))
                    chainLen[chName] += 1;
                else
                    chainLen[chName] = 1;
            }
            return chainLen.Values.Max();
        }

        public List<IBody> ShiftChain(List<IBody> bList, double dx, double dy, double dz) {
            foreach (var b in bList)
                b.Add(dx, dy, dz);
            return bList;
        }

        public List<IBody> Concatenate(List<IBody> bList1, List<IBody> bList2) {
            var last = bList1[bList1.Count - 1];
            bList2 = bList2.GetRange(1, bList2.Count - 1);
            ShiftChain(bList2, last.X, last.Y, last.Z);
            bList1.AddRange(bList2);
            return bList1;
        }

        #endregion

    }
}
