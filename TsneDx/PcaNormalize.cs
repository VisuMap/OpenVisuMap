using System;
using System.Linq;
using SharpDX;

namespace TsneDx {
    // Perform PCA based rotation normalization on a 2D or 3D maps.
    public class PcaNormalize {
        public static float[][] DoNormalize(float[][] xyz) {
            int dim = xyz[0].Length;

            float[][] M = (dim == 1) ? xyz : new FastPca().DoPca(xyz, dim);

            float Moment(float v) {
                if (float.IsNaN(v))
                    return 0;
                return (float)(Math.Sign(v) * Math.Sqrt(Math.Abs(v)));
            }

            // Flipping the map according to the 1-order moment alonge the axises.
            for (int col=0; col<dim; col++)
                if (M.Sum(R => Moment(R[col])) <= 0)
                    for (int row = 0; row < M.Length; row++)
                        M[row][col] *= -1;
            return M;
        }
    }
}
