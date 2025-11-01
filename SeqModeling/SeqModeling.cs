using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using VisuMap.Script;
using Vector3 = SharpDX.Vector3;
using Quaternion = SharpDX.Quaternion;
using EntityInfo = System.Tuple<int, string, string, int>;
using System.IO;
using MathNet.Numerics.Interpolation;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.LinearAlgebra;

namespace VisuMap {
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
            double mx = bList.Select(b => b.X).Average();
            double my = bList.Select(b => b.Y).Average();
            IBody mBody = null;
            double mDist = 1.0e10;
            foreach (IBody b in bList) {
                double d2 = (b.X - mx) * (b.X - mx) + (b.Y - my) * (b.Y - my);
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
            for (int k = 0; k < N; k++)
                P[k] = bList[k].ToV3();
            Vector3[] Mean = new Vector3[N - 2];
            float c = -(float)smoothenRatio;
            for (int rp=0; rp<repeats; rp++) {
                for(int k=0; k<Mean.Length; k++) 
                    Mean[k] = 0.5f * (P[k] + P[k + 2]);
                for (int k = 0; k < Mean.Length; k++)
                    P[k+1] += c * (P[k+1] - Mean[k]);
            }
            for(int k=1; k<(N-1); k++)
                bList[k].SetXYZ(P[k]);
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
            const short SeqMap_HEAD = 155;
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

        public INumberTable AugmentByStretch(List<IBody> bList, double strechFactor, int intRp) {
            if (strechFactor == 0)
                return New.NumberTable(bList, 3);
            INumberTable nt = New.NumberTable(bList, 4);
            int intCnt = 1;
            for (int k = 0; k < intRp; k++)
                intCnt += intCnt;
            double dx = 0.1 * strechFactor / intCnt;
            double[][] M = (double[][])nt.Matrix;
            for (int k = 0; k < nt.Rows; k++)
                M[k][3] = k * dx;
            return nt;
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

        public void PcaNormalize2D(INumberTable nt) {
            if (nt.Rows <= 3) return;
            double[][] M = nt.Matrix as double[][];
            MathUtil.CenteringInPlace(M);
            double[][] E = MathUtil.DoPca(M, 2);

            if (nt.Columns == 1) {
                ;
            } else if (nt.Columns == 2) {
                MT.ForEach(M, R => {
                    double x = R[0] * E[0][0] + R[1] * E[0][1];
                    double y = R[0] * E[1][0] + R[1] * E[1][1];
                    R[0] = x;
                    R[1] = y;
                });
            } else { // assuming nt.Columns > 2.
                MT.ForEach(M, R => {
                    double x = R[0] * E[0][0] + R[1] * E[0][1] + R[2] * E[0][2];
                    double y = R[0] * E[1][0] + R[1] * E[1][1] + R[2] * E[1][2];
                    R[0] = x;
                    R[1] = y;
                });
                M = M.Select(R => new double[] { R[0], R[1] }).ToArray();
            }

            if (nt.Rows >= 2)
                FlipNormalize(nt.Matrix as double[][]);
        }

        public void PcaNormalize3D(INumberTable nt) {
            if (nt.Rows <= 3) return;
            double[][] M = nt.Matrix as double[][];
            MathUtil.CenteringInPlace(M);
            double[][] E = MathUtil.DoPca(M, 3);

            MT.ForEach(M, R => {
                double x = R[0] * E[0][0] + R[1] * E[0][1] + R[2] * E[0][2];
                double y = R[0] * E[1][0] + R[1] * E[1][1] + R[2] * E[1][2];
                double z = R[0] * E[2][0] + R[1] * E[2][1] + R[2] * E[2][2];
                R[0] = x;
                R[1] = y;
                R[2] = z;
            });

            FlipNormalize(nt.Matrix as double[][]);
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

        public void MeanFieldTrans(INumberTable dt, double[] R) {
            int DIM = dt.Columns;
            int L = R.Length / DIM;    // number of sections
            int secLen = dt.Rows / L;  // section length
            if (dt.Rows % L != 0)
                secLen++;
            Array.Clear(R, 0, R.Length);
            for (int k = 0; k < dt.Rows; k++) {
                double[] Mr = dt.Matrix[k] as double[];
                int i0 = (k / secLen) * DIM;
                for (int i = 0; i < DIM; i++)
                    R[i0 + i] += Mr[i];
            }
        }

        public void GlobeChainTrans(List<IBody> bList, double[] R) {
            int L = R.Length / 3;    // number of sections
            int secLen = bList.Count / L;  // section length
            if (bList.Count % L != 0)
                secLen++;

            List<List<IBody>> globeList = new List<List<IBody>>();  // row indexes in the sections.
            for (int k = 0; k<bList.Count; k+=secLen) {
                var G = new List<IBody>();
                int sL = Math.Min(secLen, bList.Count - k);
                for (int i = 0; i < sL; i++)
                    G.Add(bList[k+i]);
                globeList.Add(G);
            }

            Array.Clear(R, 0, R.Length);
            MT.ForEach(globeList, (G, sI) => {
                if (G.Count == 1)  // singleton globe have zero dimension, and will be discarded.
                    return;
                double[] eValues;
                double[][] M = G.Select(b => new double[] { b.X, b.Y, b.Z }).ToArray();
                MathUtil.DoPca(M, 3, out eValues);                
                for (int i = 0; i < eValues.Length; i++)
                    R[3*sI + i] = eValues[i];
            });
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

        public List<IBody> Interpolate3D_Old(List<IBody> bList, int repeats, double convexcity, int bIdx0, int chIdx) {
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

        public List<string> FlatSampling(List<string> chIds, double minDist) {
            IList<IBody> bsList = vv.Dataset.BodyListForId(chIds);
            List<IBody> sampling = New.BodyList();
            double limit = minDist * minDist;
            foreach (IBody b in bsList) {
                bool isSampled = false;
                double bX = b.X;
                double bY = b.Y;
                for (int k = 0; k < sampling.Count; k++) {
                    IBody a = sampling[k];
                    double dx = a.X - bX;
                    double dy = a.Y - bY;
                    if ((dx * dx + dy * dy) < limit) {
                        isSampled = true;
                        break;
                    }
                }
                if (!isSampled)
                    sampling.Add(b);
            }
            return sampling.Select(b => b.Id).ToList();
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

        const double BOND_LENGTH = 3.8015;  // Average bond length. with std ca 0.1

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


        public INumberTable ToTorsionList(List<IBody> bList) {
            int L = bList.Count;
            if (L < 7)
                return null;
            Vector3[] V = bList.Select(b => new Vector3((float)b.X, (float)b.Y, (float)b.Z)).ToArray();            
            double DD(int i, int j) => Vector3.Distance(V[i], V[j]);
            INumberTable nt = New.NumberTable(L, 5);
            for (int k = 0; k < L; k++)
                nt.RowSpecList[k].CopyFromBody(bList[k]);

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

            return nt;
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

        #region Load chains from chain cache files

        public void SaveChain(string cacheFile, IList<IBody>bodyList) {
            using (StreamWriter sw = new StreamWriter(cacheFile)) {
                foreach(var b in bodyList) {
                    sw.WriteLine($"{b.Id}|{b.Name}|{b.Type}|{b.X:f2}|{b.Y:f2}|{b.Z:f2}");
                }
            }
        }

        public List<IBody> LoadChain3D(string cacheFile) {
            string[] lines = File.ReadAllLines(cacheFile);
            IBody[] bList = new IBody[lines.Length];
            MT.Loop(0, lines.Length, lineIdx => {
                string line = lines[lineIdx];
                if (line == null)
                    return;
                string[] fs = line.Split('|');
                Body b = new Body(fs[0]);
                b.Name = fs[1];
                b.Type = short.Parse(fs[2]);
                b.X = float.Parse(fs[3]);
                b.Y = float.Parse(fs[4]);
                b.Z = float.Parse(fs[5]);
                bList[lineIdx] = b;
            });
            return bList.ToList();
        }

        public string LoadChainSeq(string cacheFile) {
            StringBuilder sb = new StringBuilder();
            using (TextReader tr = new StreamReader(cacheFile)) {
                while (true) {
                    string line = tr.ReadLine();
                    if (line == null)
                        break;
                    int idx = line.IndexOf('|');
                    char c = line[idx + 1];  // The first char of the Name field.
                    if ((c == 'r') || (c == 'd'))
                        c = line[idx + 3];
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        #endregion

        #region LoadCif() method
        string pdbTitle = null;
        List<IBody> heteroChains = null;
        Dictionary<string, string> acc2chain = null;
        List<EntityInfo> entityTable = null;
        public List<EntityInfo> EntityTable { get => entityTable; }

        public List<IBody> LoadCif(string fileName, List<string> chainNames) {
            List<IBody> bList = null;
            HashSet<int> betaSet = new HashSet<int>();
            HashSet<int> helixSet = new HashSet<int>();
            entityTable = null;
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
                        //acc2chain = GetAcc2Chain(tr);
                    } else if (L.StartsWith("_atom_site.")) {
                        bList = LoadAtoms(tr, helixSet, betaSet, chainNames);
                        break;
                    } else if (L.StartsWith("_entity.details")) {
                        //LoadEnityTable(tr);
                    }
                }
            }
            return bList;
        }

        public List<List<IBody>> SplitByChainName(List<IBody> bodyList) {
            if ( (bodyList == null) || (bodyList.Count==0) )
                    return null;
            List<List<IBody>> chainList = new List<List<IBody>>();
            int iBegin = 0;
            string curName = bodyList[0].Name.Split('.')[2];
            for(int iEnd = 1; iEnd<bodyList.Count; iEnd++) {
                string chName = bodyList[iEnd].Name.Split('.')[2];
                if (chName != curName) {
                    chainList.Add(bodyList.GetRange(iBegin, iEnd - iBegin));
                    iBegin = iEnd;
                    curName = chName;
                }
            }
            if ( iBegin < bodyList.Count)
                chainList.Add(bodyList.GetRange(iBegin, bodyList.Count - iBegin));
            return chainList;
        }

        public string ToSequence(List<IBody> bList) {
            if ((bList == null) || (bList.Count == 0))
                return "";
            StringBuilder sb = new StringBuilder();
            string[] fs = bList[0].Name.Split('.');
            int chIdx = (fs[0] == "r") || (fs[0] == "d") ? 2 : 0;
            foreach (var b in bList)
                sb.Append(b.Name[chIdx]);
            return sb.ToString();
        }

        public List<EntityInfo> GetEntityTable(string fileName) {
            entityTable = null;
            using (TextReader tr = new StreamReader(fileName)) {
                string L = tr.ReadLine();
                if (!L.StartsWith("data_"))
                    return null;
                while (true) {
                    L = tr.ReadLine();
                    if (L == null)
                        break;
                    if (L.StartsWith("_entity.details")) {
                        LoadEnityTable(tr);
                        break;
                    }
                }
            }
            return entityTable;
        }

        void LoadEnityTable(TextReader tr) {
            entityTable = new List<EntityInfo>();
            List<string> fs = new List<string>();
            while (true) {
                string L = tr.ReadLine();
                if (L[0] == '#')
                    break;
                if (L[0] == ';') {
                    continue;
                }
                string[] bs = L.Split(new char[] { '\'', '\"' });
                fs.AddRange(L.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            }

            // Merge spices of quoated string togehter.
            List<string> fList = new List<string>();
            string parF = "";
            bool inWord = false;
            char quota = '\'';

            foreach(string f in fs) {
                if (inWord) {
                    if (f[f.Length - 1] == quota) {
                        fList.Add(parF + " " + f.Substring(0, f.Length - 1));
                        inWord = false;
                    } else {
                        parF += " " + f;
                    }
                } else { 
                    if ( (f[0]=='\'') || (f[0]=='\"') ) {
                        if (f[f.Length - 1] == f[0]) {
                            fList.Add(f.Substring(1, f.Length - 2));
                            inWord = false;
                        } else {
                            parF = f.Substring(1);
                            inWord = true;
                            quota = f[0];
                        }
                    } else
                        fList.Add(f);
                }
            }

            try {
                for (int k = 0; k < fList.Count; k += 10) {
                    if ((k + 5) < fList.Count) {
                        int entId = int.Parse(fList[k]);
                        string entType = fList[k + 1];
                        string entDesc = fList[k + 3];
                        int entCnt = char.IsDigit(fList[k + 5][0]) ? int.Parse(fList[k + 5]) : 1;
                        if (entId > 0)
                            entityTable.Add(new EntityInfo(entId, entType, entDesc, entCnt));
                    }
                }
            }catch(Exception) {
                // The entity section sometimes has wild format which still causes SOME TROUBERS.
            }
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
                if ((fs.Length>=9) && !dict.ContainsKey(fs[8]))
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
            {"VAL", 'V' },
            {"UNK", 'x' }
        };

        const int maxChainIndex = 144; // 4x36 of "36 Clusters" 
        List<IBody> LoadAtoms(TextReader tr, HashSet<int> helixSet, HashSet<int> betaSet, List<string> chainNames) {
            Dictionary<string, int> ch2idx = new Dictionary<string, int>() {
                { "HOH", maxChainIndex + 3 },
                { "NAG", maxChainIndex + 11 } } ;
            List<IBody> bsList = vv.New.BodyList();
            List<IBody> bsList2 = vv.New.BodyList();
            int rsIdxPre = -1;
            var RNA_set = new HashSet<string>() { "A", "U", "G", "C" };
            var DNA_set = new HashSet<string>() { "DA", "DT", "DG", "DC" };
            char[] fSeparator = new char[] { ' ' };
            char[] dbQuoats = new char[] { '"' };

            int C_ATOM_ID = -1;
            int C_COMP_ID = -1;
            int C_ENTITY_ID = -1;
            int C_SEQ_ID = -1;
            int C_CARTN_X = -1;
            int C_CARTN_Y = -1;
            int C_CARTN_Z = -1;
            int C_ASYM_ID = -1;
            int C_MODEL_NUM = -1;
            Dictionary<string, int> colName2Idx = new Dictionary<string, int>();
            int idxF = 1;
            int cntF = -1;

            while (true) {
                string L = tr.ReadLine();
                if ((L == null) || (L[0] == '#'))
                    break;
                if ( L.StartsWith("_atom_site.") ){
                    string cName = L.Substring(L.IndexOf('.') + 1).TrimEnd();
                    colName2Idx[cName] = idxF++;
                    continue;
                }
                if (C_ATOM_ID < 0) {
                    try {
                        C_ATOM_ID = colName2Idx["label_atom_id"];  //3
                        C_COMP_ID = colName2Idx["label_comp_id"];  // 5
                        C_ENTITY_ID = colName2Idx["label_entity_id"];  // 7
                        C_SEQ_ID = colName2Idx["label_seq_id"];  // 8
                        C_CARTN_X = colName2Idx["Cartn_x"];  // 10
                        C_CARTN_Y = colName2Idx["Cartn_y"];  // 11
                        C_CARTN_Z = colName2Idx["Cartn_z"];  // 12
                        C_ASYM_ID = colName2Idx["auth_asym_id"]; // 18
                        C_MODEL_NUM = colName2Idx["pdbx_PDB_model_num"]; // 20
                        cntF = colName2Idx.Count + 1;
                    } catch(Exception ex) {
                        vv.LastError = "Invalid ATOM sections" + ex.ToString();
                        return null;
                    }
                }

                // Ignore comments withing ATOM rows.
                if (L[0] == '_') 
                    continue;

                string[] fs = L.Split(fSeparator, StringSplitOptions.RemoveEmptyEntries);
                if ( fs.Length < cntF) { // The row extends to the next line.
                    L = tr.ReadLine();
                    var fs2 = fs.ToList();
                    fs2.AddRange(L.Split(fSeparator, StringSplitOptions.RemoveEmptyEntries));
                    fs = fs2.ToArray();
                    if (fs.Length < cntF)
                        continue;
                }

                string chName = fs[C_ASYM_ID] + "_" + fs[C_MODEL_NUM];
                string atName = fs[C_ATOM_ID].Trim(dbQuoats);
                string rsName = fs[C_COMP_ID];
                int entityId = int.Parse(fs[C_ENTITY_ID]);
                string secType = "x";
                string p1 = "x";
                string bId = null;               

                if (fs[0] == "ATOM") {
                    int rsIdx = int.Parse(fs[C_SEQ_ID]) - 1;
                    if (rsIdx == rsIdxPre)
                        continue;
                    if (P3.ContainsKey(rsName) && ((atName == "CA") || (atName == "C2"))) {
                        p1 = P3[rsName].ToString();
                        if (helixSet.Contains(rsIdx))
                            secType = "h";
                        else if (betaSet.Contains(rsIdx))
                            secType = "b";
                        rsName = "";
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
                    bId = $"H.{fs[C_ATOM_ID]}.{bsList2.Count}";
                    p1 = fs[C_ATOM_ID];
                } else
                    continue;

                IBody b = vv.New.Body(bId);
                b.X = float.Parse(fs[C_CARTN_X]);
                b.Y = float.Parse(fs[C_CARTN_Y]);
                b.Z = float.Parse(fs[C_CARTN_Z]);

                b.Name = p1 + '.' + rsName + '.' + chName + '.' + secType;
                b.Type = (short)(entityId - 1);

                if (b.Id[0] == 'H') {
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
