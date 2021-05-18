using System;
using System.Collections.Generic;
using System.Text;

using VisuMap.Plugin;
using VisuMap.Script;

namespace DataGenerator {
    public class Body {
        double x, y, z;
        int type;

        public double Z {
            get { return z; }
            set { z = value; }
        }

        public double Y {
            get { return y; }
            set { y = value; }
        }

        public double X {
            get { return x; }
            set { x = value; }
        }

        public int Type {
            get { return type; }
            set { type = value; }
        }

        public Body(int type, double x, double y, double z) {
            this.type = type;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Body Clone() {
            return new Body(type, x, y, z);
        }

        public Body Translate(double dx, double dy, double dz) {
            x += dx;
            y += dy;
            z += dz;
            return this;
        }

        public Body Scale(double fx, double fy, double fz) {
            x *= fx;
            y *= fy;
            z *= fz;
            return this;
        }

        public Body Rotate(double alpha, char axis) {
            double xh, yh, zh;
            switch (axis) {
                case 'x':
                    yh = Math.Cos(alpha) * y - Math.Sin(alpha) * z;
                    zh = Math.Sin(alpha) * y + Math.Cos(alpha) * z;
                    y = yh;
                    z = zh;
                    break;

                case 'y':
                    zh = Math.Cos(alpha) * z - Math.Sin(alpha) * x;
                    xh = Math.Sin(alpha) * z + Math.Cos(alpha) * x;
                    z = zh;
                    x = xh;
                    break;

                case 'z':
                    xh = Math.Cos(alpha) * x - Math.Sin(alpha) * y;
                    yh = Math.Sin(alpha) * x + Math.Cos(alpha) * y;
                    x = xh;
                    y = yh;
                    break;
            }
            return this;
        }

    }

    public class RectangleSet : BodySet {
        public RectangleSet(int type, int width, int height) {
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    AddBody(new Body(type, (double)i, (double)j, 0.0));
                }
            }
        }
    }

    public class Circle : BodySet {
        public Circle(int type, double R, int numberOfPoints) {
            double angle = Math.PI * 2 / numberOfPoints;
            for (double a = 0.0; a < (Math.PI * 2 - 0.001); a += angle) {
                AddBody(new Body(type, R * Math.Sin(a), R * Math.Cos(a), 0));
            }
        }
    }

    public class Cubic : BodySet {
        public Cubic(int type, double length, int number) {
            for (int i = 0; i < number; i++) {
                for (int j = 0; j < number; j++) {
                    for (int k = 0; k < number; k++) {
                        AddBody(new Body(type, i * length, j * length, k * length));
                    }
                }
            }
        }
    }

    public class Tetrahedron : BodySet {
        public Tetrahedron(int type, int size) {
            for (int i = 0; i < size; i++) {
                for (int j = 0; j < size; j++) {
                    for (int k = 0; k < size; k++) {
                        if ( (i+j+k) <= size )
                            AddBody(new Body(type, i, j, k));
                    }
                }
            }
        }
    }

    public class Disc : BodySet {
        public Disc(int type, double R, double length) {
            double x, y;
            int N = (int)(R / length);
            for (int i = -N; i <= N; i++) {
                for (int j = -N; j <= N; j++) {
                    x = i * length;
                    y = j * length;
                    if (Math.Sqrt(x * x + y * y) < R) {
                        AddBody(new Body(type, x, y, 0));
                    }
                }
            }
        }
    }

    public class Line : BodySet {
        public Line(int type, double length, int number) {
            for (int i = -number; i < number; i++) {
                AddBody(new Body(type, i * length, 0, 0));
            }
        }
    }

    public class RandomBall : BodySet {
        public RandomBall(int type, double radius, int bodyNumber) {
            System.Random rGenerator = new Random();
            for (int i = 0; i < bodyNumber; i++) {
                while (true) {
                    double x, y, z;
                    x = 2 * radius * rGenerator.NextDouble() - radius;
                    y = 2 * radius * rGenerator.NextDouble() - radius;
                    z = 2 * radius * rGenerator.NextDouble() - radius;
                    if (Math.Sqrt(x * x + y * y + z * z) < radius) {
                        AddBody(new Body(type, x, y, z));
                        break;
                    }
                }
            }
        }
    }

    public class RandomCubic : BodySet {
        public RandomCubic(int type, double edgeLength, int totalNumber) {
            System.Random rGenerator = new Random();
            for (int i = 0; i < totalNumber; i++) {
                AddBody(new Body(type,
                    edgeLength * rGenerator.NextDouble(),
                    edgeLength * rGenerator.NextDouble(),
                    edgeLength * rGenerator.NextDouble()));
            }
        }
    }

    public class SingleHelix : BodySet {
        public SingleHelix(int type, double r, double delta, double alpha, double alphaTotal) {
            double x;
            double y;
            double z = 0;
            for (double a = 0; a < alphaTotal; a += alpha) {
                z += delta;
                x = r * Math.Cos(a);
                y = r * Math.Sin(a);
                AddBody(new Body(type, x, y, z));
            }
        }
    }

    public class Sphere : BodySet {
        public Sphere(int type, double R, double alpha) {
            double a;
            double r;
            double x, y, z;
            for (a = -Math.PI / 2 + alpha; a < Math.PI / 2; a += alpha) {
                r = R * Math.Cos(a);
                z = R * Math.Sin(a);
                double theta = R / r * alpha;
                for (double t = 0; t < 2 * Math.PI; t += theta) {
                    x = r * Math.Sin(t);
                    y = r * Math.Cos(t);
                    AddBody(new Body(type, x, y, z));
                }
            }
        }
    }

    /// <summary>
    /// Summary description for Sphere.
    /// </summary>
    public class Spheroid : BodySet {
        public Spheroid(int type, double rMajor, double rMinor, double arcLen) {
            double a;
            double r;
            double x, y, z;
            double alpha = Math.Asin(arcLen / rMajor);
            double ratio = rMajor / rMinor;

            for (a = -Math.PI / 2 + alpha; a < Math.PI / 2; ) {
                r = rMajor * Math.Cos(a);
                z = rMinor * Math.Sin(a);

                double theta = rMajor / r * alpha;
                for (double t = 0; t < 2 * Math.PI; t += theta) {
                    x = r * Math.Sin(t);
                    y = r * Math.Cos(t);
                    AddBody(new Body(type, x, y, z));
                }

                double rh = rMajor * Math.Cos(a + alpha);
                double zh = rMinor * Math.Sin(a + alpha);
                double arcLen_h = Math.Sqrt((rh - r) * (rh - r) + (zh - z) * (zh - z));
                a += arcLen / arcLen_h * alpha;
            }
        }
    }

    public class Torus : BodySet {
        public Torus(double R, double r, int N, int n) {
            for (int i = 0; i < N; i++) {
                double u = (2 * Math.PI * i) / N;
                for (int j = 0; j < n; j++) {
                    double v = (2 * Math.PI * j) / n;
                    double x = (R + r * Math.Cos(v)) * Math.Cos(u);
                    double y = (R + r * Math.Cos(v)) * Math.Sin(u);
                    double z = r * Math.Sin(v);
                    AddBody(new Body(0, x, y, z));
                }
            }
        }
    }

    public class Gaussian : BodySet {
        static Random rg;

        public Gaussian(int type, int n) {
            if (rg == null) {
                rg = new Random();
            }

            double[] xs = GaussianSampling(n, 1.0);
            double[] ys = GaussianSampling(n, 1.0);
            double[] zs = GaussianSampling(n, 1.0);
            for (int i = 0; i < n; i++) {
                AddBody(new Body(type, xs[i], ys[i], zs[i]));
            }
        }

        public static double[] GaussianSampling(int n, double variance) {
            double[] d = new double[n];
            int K = 100;
            double f = Math.Sqrt(variance);
            for (int i = 0; i < n; i++) {
                double v = 0;
                for (int k = 0; k < K; k++) {
                    v += rg.NextDouble();
                }
                v -= K / 2.0;
                d[i] = f * v * Math.Sqrt(12.0 / K);
            }
            return d;
        }
    }

    public class Triangle : BodySet {
        public Triangle(int type, double edgeLength) {
            double H = edgeLength * Math.Cos(Math.PI / 12);
            for (double y = 0; y <= H; y += 1.0) {
                double W = 2.0 * (H - y) * Math.Sin(Math.PI / 12);
                for (double x = -W; x <= W; x += 1.0) {
                    AddBody(new Body(type, x, y, 0.0));
                }
            }
        }
    }

    public class OpenBox : BodySet {
        public OpenBox(int edgeSize) {
            double xShift = edgeSize / 2;
            double yShift = edgeSize / 2;
            double zShift = edgeSize / 2;
            short type = 0;

            for (int i = 0; i <= edgeSize; i++) {
                for (int j = 0; j <= edgeSize; j++) {
                    for (int k = 0; k <= edgeSize; k++) {
                        if ((i == 0) || (j == 0) || (k == 0) || (i == edgeSize) || (k == edgeSize)) {
                            if (j == 0) {
                                type = 15;
                            } else if ((i == 0) || (i == edgeSize)) {
                                type = (short)((i == 0) ? 4 : 7);
                            } else {
                                type = (short)((k == 0) ? 8 : 11);
                            }
                            AddBody(new Body(type, i - xShift, j - yShift, k - zShift));
                        }
                    }
                }
            }
        }
    }

    public class Vector4D {
        public Vector4D(int type, double x, double y, double z, double w) {
            this.type = type;
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public int type;
        public double x;
        public double y;
        public double z;
        public double w;

        public static IPcaView Show(IList<Vector4D> points, int dimension) {
            INumberTable nt = DataGenerator.App.ScriptApp.New.NumberTable(points.Count, dimension);
            for (int row = 0; row < nt.Rows; row++) {
                nt.Matrix[row][0] = points[row].x;
                nt.Matrix[row][1] = points[row].y;
                if (dimension == 4) {
                    nt.Matrix[row][2] = points[row].z;
                    nt.Matrix[row][3] = points[row].w;
                }
                nt.RowSpecList[row].Type = (short)points[row].type;
            }
            return nt.ShowPcaView();
        }
    }

    public class KleinBottle4D {
        IList<Vector4D> points;
        IList<Vector4D> orgPoints;

        public KleinBottle4D(int width, int height) {
            points = new List<Vector4D>();
            orgPoints = new List<Vector4D>();

            const double a = 1.5;
            const double r = 1.0;

            for (int i = 0; i < width; i++) {
                double u = Math.PI * 2 * i / width;
                //
                // C. TOMPKINS: "A FLAT KLEIN BOTTLE ISOMETRICALLY EMBEDDED IN EUCLIDEAN 4-SPACE".
                // When u is constant, the surface becomes ellipse. When v is constant,
                // the surface become double winding ellpise.
                //
                // Notice: The above paper published the form for the case a = 0 and r = 1. 
                //
                // Notice: The embedding is only isometrically when a=0. The isometrical embedding
                // looks more convoluted, but is isometrical to the flat klein-bottle.
                //
                for (int j = 0; j < height; j++) {
                    double v = Math.PI * 2 * j / height;
                    int k = i * height + j;

                    int type = 0;
                    if (j < 2) {
                        type = 15;
                    }
                    if (i < 2) {
                        type = 11;
                    }

                    points.Add(new Vector4D(type,
                        (r * Math.Cos(v) + a) * Math.Cos(u),
                        (r * Math.Cos(v) + a) * Math.Sin(u),
                        2 * r * Math.Sin(v) * Math.Cos(u / 2),
                        2 * r * Math.Sin(v) * Math.Sin(u / 2)));
                    orgPoints.Add(new Vector4D(type, u, v, 0, 0));
                }
            }
        }

        public IPcaView Show() {
            Vector4D.Show(orgPoints, 2);
            return Vector4D.Show(points, 4);
        }
    }

    public class Projective4D {
        IList<Vector4D> points;
        IList<Vector4D> orgPoints;

        public Projective4D(double R, double alpha) {
            double a;
            double r;
            double x, y, z;
            points = new List<Vector4D>();
            orgPoints = new List<Vector4D>();

            for (a = 0; a <= Math.PI / 2; a += alpha) {
                r = R * Math.Cos(a);
                z = R * Math.Sin(a);
                double theta = R / r * alpha;
                for (double t = 0; t < 2 * Math.PI; t += theta) {
                    x = r * Math.Sin(t);
                    y = r * Math.Cos(t);
                    if ((z == 0) && (x <= 0)) {
                        // skip half of the points on the equrator.
                        continue;
                    }
                    int type = (a <= 1.5 * alpha) ? 15 : 0;
                    double dt = Math.Abs(t - Math.PI);
                    if ((t <= 0) || (dt < theta)) {
                        type = 11;
                    }

                    points.Add(new Vector4D(type, x * y, x * z, y * y - z * z, 2 * y * z));
                    orgPoints.Add(new Vector4D(type, x, y, z, 0));
                }
            }
        }

        public IPcaView Show() {
            Vector4D.Show(orgPoints, 2);
            return Vector4D.Show(points, 4);
        }
    }

    public class BodySet {
        private List<Body> bodies = new List<Body>();

        public List<Body> Bodies {
            get { return bodies; }
        }

        public void AddPoint(short type, double x, double y, double z) {
            bodies.Add(new Body(type, x, y, z));
        }

        public BodySet AddBody(Body b) {
            bodies.Add(b);
            return this;
        }

        public void RemoveBody(Body b) {
            bodies.Remove(b);
        }

        public void RemoveFromCubic(
            double x0, double y0, double z0,
            double x1, double y1, double z1) {
            List<Body> list = new List<Body>();
            foreach (Body b in bodies) {
                if ((x0 < b.X) && (b.X < x1) && (y0 < b.Y) && (b.Y < y1) && (z0 < b.Z) && (b.Z < z1)) {
                    list.Add(b);
                }
            }
            foreach (Body b in list) {
                RemoveBody(b);
            }
        }

        public void RemoveFromBall(double x0, double y0, double z0, double radius) {
            List<Body> list = new List<Body>();
            foreach (Body b in bodies) {
                double x = b.X - x0;
                double y = b.Y - y0;
                double z = b.Z - z0;
                if (Math.Sqrt(x * x + y * y + z * z) < radius) {
                    list.Add(b);
                }
            }
            foreach (Body b in list) {
                RemoveBody(b);
            }
        }

        public static BodySet operator+(BodySet set1, BodySet set2) {
            return set1.AddBodySet(set2);
        }


        public BodySet AddBodySet(BodySet bodySet) {
            bodies.AddRange(bodySet.bodies);
            return this;
        }

        public BodySet Translate(double dx, double dy, double dz) {
            foreach (Body b in bodies) {
                b.Translate(dx, dy, dz);
            }
            return this;
        }

        public BodySet Scale(double fx, double fy, double fz) {
            foreach (Body b in bodies) {
                b.Scale(fx, fy, fz);
            }
            return this;
        }

        public BodySet Clone() {
            BodySet bs = new BodySet();
            foreach (Body b in bodies) {
                bs.AddBody(b.Clone());
            }
            return bs;
        }

        public BodySet SetType(int type) {
            foreach (Body b in bodies) {
                b.Type = type;
            }
            return this;
        }

        public BodySet Rotate(double alpha, char axis) {
            foreach (Body b in bodies) {
                b.Rotate(alpha, axis);
            }
            return this;
        }
        
        static Random rdGenerator;

        public BodySet AddNoice(double percent, double range, short noiceType) {
            if (rdGenerator == null) {
                rdGenerator = new Random();
            }
            foreach (Body b in bodies) {
                if (rdGenerator.NextDouble() < percent) {
                    b.Type = noiceType;
                    b.X += rdGenerator.NextDouble() * 2 * range - range;
                    b.Y += rdGenerator.NextDouble() * 2 * range - range;
                    b.Z += rdGenerator.NextDouble() * 2 * range - range;
                }
            }
            return this;
        }

        public IPcaView Show() {
            INumberTable nt = DataGenerator.App.ScriptApp.New.NumberTable(bodies.Count, 3);
            for (int row = 0; row < nt.Rows; row++) {
                nt.Matrix[row][0] = bodies[row].X;
                nt.Matrix[row][1] = bodies[row].Y;
                nt.Matrix[row][2] = bodies[row].Z;
                nt.RowSpecList[row].Type = (short) bodies[row].Type;
            }
            return nt.ShowPcaView();
        }
    }
}
