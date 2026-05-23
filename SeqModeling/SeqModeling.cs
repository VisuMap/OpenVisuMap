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

        public INumberTable MovingWindowFT(IList<string> pList, IList<double> wsList, 
                INumberTable tm, int intRp = 0, string cacheDir=null) {
            List<IBody> bList = vv.Dataset.BodyListForId(pList) as List<IBody>;
            INumberTable D = New.NumberTable(bList, tm.Columns * wsList.Count);
            const double EPS = 0.085;
            System.IO.Directory.SetCurrentDirectory(cacheDir);
            MT.LoopNoblocking(0, pList.Count, k => {
                string pId = pList[k];
                var bs = LoadChain3D(pId + ".pmc");
                if (intRp > 0)
                    bs = Interpolate3D(bs, intRp, EPS, bs.Count, 0);
                int idx0 = 0;
                bool isNucleotide = vv.Dataset.StringAt(pId, 0)[0] != 'A';
                double[] Rk = (double[])D.Matrix[k];
                double[] bDist = new double[bs.Count];
                foreach (double w in wsList) {
                    int ws = (int) ( (w<1.0) ? (bs.Count * w) : w ) ;
                    if (ws < bs.Count) {
                        MovingWindowVariance(bs, ws, bDist);
                        VectorizeChainFT(bDist, tm, Rk, idx0);
                    }
                    idx0 += tm.Columns;
                }
                if ((k > 0) && (k % 500 == 0)) {
                    vv.Title = $"Reading chains: {k} of {pList.Count}";
                }
            });

            // Normalize each section by the average value of their first column.
            double[] sum = new double[wsList.Count];
            double[][] M = (double[][]) D.Matrix;
            for(int row=0; row<D.Rows; row++)
                for (int col = 0, c = 0; col < D.Columns; col += tm.Columns, c += 1)
                    sum[c] += M[row][col];
            for (int c = 0; c < sum.Length; c++) 
                sum[c] /= D.Rows;
            MT.ForEach(M, R=> {
                for (int c = 0; c < sum.Length; c++) {
                    int idx0 = c * tm.Columns;
                    for (int col = 0; col < tm.Columns; col += 1)
                        R[idx0 + col] /= sum[c];
                }
            });
            return D;
        }

        void CalculateTorsions(Vector3[] P, double[] tA, double[] tB) {
            int L = P.Length;
            for (int k = 0; k < L - 1; k++)
                P[k] -= P[k + 1];
            P[L - 1] = new Vector3();
            for (int k = 0; k < L - 2; k++)
                tA[k] = 4 * Vector3.Dot(P[k], P[k + 1]);

            for (int k = 0; k < L - 2; k++)
                P[k] -= P[k + 1];
            P[L - 2] = new Vector3();
            for (int k = 0; k < L - 3; k++)
                tB[k] = -Vector3.Dot(P[k], P[k + 1]);
        }

        public INumberTable BondTorsionFT(IList<string> pList, INumberTable tm, string cacheDir = null) {
            List<IBody> bList = vv.Dataset.BodyListForId(pList) as List<IBody>;
            INumberTable nt = New.NumberTable(pList.Count, 2 * tm.Columns);
            double[][] M = (double[][])nt.Matrix;
            for (int row = 0; row < nt.Rows; row++)
                nt.RowSpecList[row].CopyFromBody(bList[row]);
            System.IO.Directory.SetCurrentDirectory(cacheDir);
            MT.LoopNoblocking(0, pList.Count, row => {
                Vector3[] P = LoadChainV3(pList[row] + ".pmc"); // The positions of alpha-C.
                double[] tA = new double[P.Length - 2];
                double[] tB = new double[P.Length - 3];
                CalculateTorsions(P, tA, tB);
                VectorizeChainFT(tA, tm, M[row], 0);
                VectorizeChainFT(tB, tm, M[row], tm.Columns);
            });
            return nt;
        }

        public INumberTable ToTorsionList(List<IBody> bList) {
            int L = bList.Count;
            if (L < 4)
                return null;
            Vector3[] P = bList.Select(b => new Vector3((float)b.X, (float)b.Y, (float)b.Z)).ToArray();
            INumberTable nt = New.NumberTable(2, L - 2);
            for (int k = 0; k < nt.Columns; k++)
                nt.ColumnSpecList[k].CopyFromBody(bList[k]);
            nt.RowSpecList[0].Id = "TorsionA";
            nt.RowSpecList[1].Id = "TorsionB";
            CalculateTorsions(P, (double[])nt.Matrix[0], (double[])nt.Matrix[1]);
            return nt;
        }


        public INumberTable BondGapeSpetrum(IList<string> pList, INumberTable tm=null, string cacheDir = null) {
            List<IBody> bList = vv.Dataset.BodyListForId(pList) as List<IBody>;
            double[][] M = new double[bList.Count][];
            System.IO.Directory.SetCurrentDirectory(cacheDir);
            MT.LoopNoblocking(0, pList.Count, k => {
                var bs = LoadChain3D(pList[k] + ".pmc");
                double[] L = new double[bs.Count-1];
                double sum = 0.0;
                int sumCnt = 0;
                for (int i = 0; i < bs.Count - 1; i++) {
                    double v = bs[i + 1].DistanceTo(bs[i]);
                    L[i] = v;
                    if ( Math.Abs(v - BOND_LENGTH) < 0.2 ) { 
                        sum += v;
                        sumCnt++;
                    }
                }
                if (sumCnt > 0) {
                    double av = sum / sumCnt;
                    for (int i = 0; i < L.Length; i++)
                        L[i] -= av;
                }
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
                    var bs = LoadChain3D(pList[0] + ".pmc");
                    for (int k = 0; k < D.Columns; k++)
                        D.ColumnSpecList[k].CopyFromBody( bs[k+1] );
                }
                return D;
            } else {
                INumberTable D = New.NumberTable(bList, tm.Columns);
                MT.LoopNoblocking(0, pList.Count, k => {
                    double[] R = M[k];
                    for (int i = 0; i < R.Length; i++) {
                        // Enlarge the bond lenght reduction 
                        // (which is normally just ca. 0.8) by a factor.
                        if (R[i] < -0.5)
                            R[i] *= 15.0;
                    }
                    VectorizeChainFT(R, tm, D.Matrix[k] as double[], 0);
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
                R[index0+col] = v;
            });
        }

        public List<IBody> InterpolateETC(List<IBody> bList, int intp = 8, bool hidIntp = false,
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
                    List<IBody> P1 = Interpolate3D(P0, intp, EPS, bs.Count, chIdx);
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


        public List<IBody> Interpolate3D(List<IBody> bList, int intp, double convexcity, int bIdx0, int chIdx) {
            if ((bList.Count <= 1) || (intp <= 1))
                return bList;
            int L = intp;
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

        public INumberTable InterpolateVector(float[][] vList, int intp) {
            if ((vList.Length <= 1) || (intp <= 1))
                return null;
            int dim = vList[0].Length;
            int N = vList.Length;
            int iN = (N - 1) * intp + 1;
            double[] S = Enumerable.Range(0, N).Select(k=>(double)k).ToArray();
            double dx = S[N - 1] / (iN - 1);
            double[][] T = MathUtil.NewMatrix(iN, dim);
            for (int col = 0; col < dim; col++) {
                double[] V = vList.Select(v => (double) v[col]).ToArray();
                var sp = CubicSpline.InterpolateNaturalSorted(S, V);                
                for (int row = 0; row < iN; row++)
                    T[row][col] = sp.Interpolate(row * dx);
            }
            return New.NumberTable(T);
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

        void MovingWindowMean(float[][] P, float[][] P1, int winSize) {
            int L = P[0].Length - 1;
            winSize = Math.Min(L, winSize);
            int WS = 2 * winSize + 1;
            float cf = (float)(1.0 / WS);

            float GetValue(float[] R, int i) {
                return (i<0) ? (2*R[0]-R[-i]) : ((i>L) ? (2*R[L]-R[2*L-i]) : R[i]);
            }

            for (int row=0; row<P.Length; row++) {
                var R = P[row];
                var T = P1[row];
                float S = WS * R[0];
                for(int col=1; col<L; col++) {
                    S += GetValue(R, col + winSize) - GetValue(R, col - winSize - 1);
                    T[col] = cf * S;
                }
                T[0] = R[0];
                T[L] = R[L];
            }
        }

        static void VectorVariance(float[][] A, float[][] B, double[] R) {
            for (int col = 0; col < A[0].Length; col++) {
                double sum2 = 0.0;
                for (int row = 0; row < A.Length; row++) {
                    float diff = A[row][col] - B[row][col];
                    sum2 += diff * diff;
                }
                R[col] = sum2;
            }
        }
        static void InterpolateColumns(float[][] P, int intp) {
            int L = P[0].Length;
            int iL = (L - 1) * intp + 1;
            double[] S = Enumerable.Range(0, L).Select(k => (double)k).ToArray();
            double dx = S[L - 1] / (iL - 1);
            for (int row = 0; row < P.Length; row++) {
                double[] V = P[row].Select(v => (double)v).ToArray();
                float[] newP = new float[iL];
                var sp = CubicSpline.InterpolateNaturalSorted(S, V);
                for (int col = 0; col < iL; col++)
                    newP[col] = (float)sp.Interpolate(col * dx);
                P[row] = newP;
            }
        }

        float[][] SeqToOneHot(string s, Dictionary<char, List<int>> aa2cIdxes) {
            int groups = aa2cIdxes.Values.Max(v => v.Max()) + 1;

            int L = s.Length;
            // Create the 1-hot table for s
            float[][] P = MathUtil.NewMatrix<float>(groups, L);
            for (int k = 0; k < L; k++) {
                char c = s[k];
                if (aa2cIdxes.ContainsKey(c))
                    foreach (int idx in aa2cIdxes[c])
                        P[idx][k] = 1.0f;
            }
            return P;
        }

        public INumberTable MovingWinVar(string s, string aaGroups, int wsStepSize, int wsCnt, int intp) {
            var aa2cIdxes = Cluster2IdxList(aaGroups);
            float[][] P = SeqToOneHot(s, aa2cIdxes);

            // Intepolate the table P.
            if (intp > 1)
                InterpolateColumns(P, intp);

            // Calculate the mwMean.
            int iL = P[0].Length;
            INumberTable nt = New.NumberTable(wsCnt, iL);
            for (int row = 0; row < wsCnt; row++) {
                float[][] P1 = MathUtil.NewMatrix<float>(P.Length, iL);
                float[][] P2 = MathUtil.NewMatrix<float>(P.Length, iL);
                int ws = row * wsStepSize;
                MovingWindowMean(P, P1, ws);
                MovingWindowMean(P1, P2, ws);
                VectorVariance(P1, P2, (double []) nt.Matrix[row]);
            }
            return nt;
        }

        public INumberTable VectorizeProtein(IList<string> seqList, string aaGroups, INumberTable trMatrix=null, int winSize=7, int intp=8) {
            var aa2cIdxes = Cluster2IdxList(aaGroups);
            int maxL = seqList.Select(s => s.Length).Max();
            int groups = aa2cIdxes.Values.Max(v => v.Max()) + 1;  // number of aa-groups in aaGroups
            int columns = maxL;
            if (trMatrix != null) 
                columns = (winSize != 0) ? trMatrix.Columns : groups * trMatrix.Columns;           
            INumberTable nt = New.NumberTable(seqList.Count, columns);

            MT.Loop(0, seqList.Count, pIdx => {
                // Convert sequence to multidimensional 1-hot vectors.
                string s = seqList[pIdx];
                float[][] P = SeqToOneHot(s, aa2cIdxes);

                //Interpolating the vectors.
                if (intp > 1) 
                    InterpolateColumns(P, intp);

                double[] R = (double[])nt.Matrix[pIdx];

                // calculate mmV of P.
                if (winSize > 0) {
                    float[][] P1 = MathUtil.NewMatrix<float>(P.Length, P[0].Length);
                    MovingWindowMean(P, P1, winSize);
                    MovingWindowMean(P1, P, winSize);
                    double[] vvs = new double[P[0].Length];  // Local variances at each aa.
                    VectorVariance(P, P1, vvs);
                    // Apply FFT on the mmV and store the result into nt.Matrix[pIdx]
                    if (trMatrix != null) {
                        VectorizeChainFT(vvs, trMatrix, R, 0);
                    } else {
                        Array.Copy(vvs, R, Math.Min(vvs.Length, nt.Columns));
                    }
                } else {
                    // apply FFT directly on P
                    for(int row=0; row<P.Length; row++) {
                        double[] Prow = new double[P[row].Length];
                        Array.Copy(P[row], Prow, Prow.Length);
                        VectorizeChainFT(Prow, trMatrix, R, row * trMatrix.Columns);
                    }
                }
            });
            return nt;
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

            Vector3 xP(int i) {
                return (i < 0) ? (2*P[0] - P[-i]) : 
                       (i > L) ? (2*P[L] - P[2*L - i]) : P[i];
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
