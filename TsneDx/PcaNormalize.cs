using System;
using System.Linq;
using SharpDX;

namespace TsneDx {
    public class PcaNormalize {
        public static float[][] DoNormalize(float[][] xyz) {
            int N = xyz.Length;
            int dim = xyz[0].Length;

            float[] c = new float[dim];
            for(int row=0; row < N; row++) {
                for(int col=0; col<dim; col++)
                    c[col] += xyz[row][col];
            }
            for (int col = 0; col < dim; col++)
                c[col] /= N;

            float[][] M = (dim == 2) ? xyz.Select(v => new float[] { v[0] - c[0], v[1] - c[1] }).ToArray()
                                     : xyz.Select(v => new float[] { v[0] - c[0], v[1] - c[1], v[2] - c[2] }).ToArray();

            float MomentFct(float v) {
                return (float)(Math.Sign(v) * Math.Sqrt(Math.Abs(v)));
            }

            M = new FastPca().DoPca(M, dim);
            for(int col=0; col<dim; col++)
                if (M.Sum(Row => MomentFct(Row[col])) <= 0)
                    for (int row = 0; row < N; row++)
                        M[row][col] *= -1;
            return M;
        }
    }
}
