using System;
using System.Collections.Generic;
using Vector3 = SharpDX.Vector3;
using VisuMap.Script;

namespace VisuMap {
    public static class MyExtensions {
        public static Vector3 ToV3(this IBody b) {
            return new Vector3((float)b.X, (float)b.Y, (float)b.Z);
        }
        public static Vector3 ToV3(this IList<double> v) {
            return new Vector3((float)v[0], (float)v[1], (float)v[2]);
        }

        public static void SetXYZ(this IBody b, Vector3 p) {
            b.SetXYZ(p.X, p.Y, p.Z);
        }

        public static void SetXYZ(this IList<double> v, Vector3 p) {
            v[0] = p.X;
            v[1] = p.Y;
            v[2] = p.Z;
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

}
