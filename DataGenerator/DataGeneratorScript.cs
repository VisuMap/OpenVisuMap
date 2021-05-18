using System;
using System.Collections.Generic;
using System.Text;
using VisuMap.Plugin;
using VisuMap.Script;

namespace DataGenerator {
    public class DataGeneratorScript : IPluginObject {
        public string Name { 
            get { return "DataGenerator"; } 
            set {; }
        }

        public void Synchronize3D() {
            DataGenerator.Synchronize3D();
        }

        public BodySet Empty() {
            return new BodySet();
        }

        public BodySet Rectangle(int type, int width, int height) {
            return new RectangleSet(type, width, height);
        }

        public BodySet Circle(int type, double R, int numberOfPoints) {
            return new Circle(type, R, numberOfPoints);
        }

        public BodySet Cubic(int type, double length, int number) {
            return new Cubic(type, length, number);
        }

        public BodySet Disc(int type, double R, double length) {
            return new Disc(type, R, length);
        }

        public BodySet Line(int type, double length, int number) {
            return new Line(type, length, number);
        }

        public BodySet RandomBall(int type, double radius, int bodyNumber) {
            return new RandomBall(type, radius, bodyNumber);
        }

        public BodySet RandomCubic(int type, double edgeLength, int totalNumber) {
            return new RandomCubic(type, edgeLength, totalNumber);
        }

        public BodySet SingleHelix(int type, double r, double delta, double alpha, double alphaTotal) {
            return new SingleHelix(type, r, delta, alpha, alphaTotal);
        }

        public BodySet Sphere(int type, double R, double alpha) {
            return new Sphere(type, R, alpha);
        }

        public BodySet Spheroid(int type, double rMajor, double rMinor, double arcLen) {
            return new Spheroid(type, rMajor, rMinor, arcLen);
        }

        public BodySet Torus(double R, double r, int N, int n) {
            return new Torus(R, r, N, n);
        }

        public BodySet Gaussian(int type, int n) {
            return new Gaussian(type, n);
        }

        public BodySet Triangle(int type, double edgeLength) {
            return new Triangle(type, edgeLength);
        }

        public BodySet OpenBox(int edgeSize) {
            return new OpenBox(edgeSize);
        }

        public KleinBottle4D KleinBottle4D(int width, int height) {
            return new KleinBottle4D(width, height);
        }

        public Projective4D Projective4D(double R, double alpha) {
            return new Projective4D(R, alpha);
        }
    }
}
