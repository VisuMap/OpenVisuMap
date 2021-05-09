using System;
using System.Linq;
using SharpDX;

namespace TsneDx {
    public class PcaNormalize {
        public static float[][] DoNormalize(float[][] xyz) {
            int N = xyz.Length;
            int dim = xyz[0].Length;

            float[] c = new float[dim];
            for (int col = 0; col < dim; col++) {
                for (int row = 0; row < N; row++)
                    c[col] += xyz[row][col];
                c[col] /= N;
            }

            float[][] M = new float[N][];
            for (int row = 0; row < N; row++) {
                M[row] = new float[dim];
                for (int col = 0; col < dim; col++)
                    M[row][col] = xyz[row][col] - c[col];
            }

            float MomentFct(float v) {
                if (float.IsNaN(v))
                    return 0;
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
