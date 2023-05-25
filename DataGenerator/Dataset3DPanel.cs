using System;
using System.Windows.Forms;
using System.ComponentModel;
using VisuMap;
using VisuMap.Controls;

namespace DataGenerator {
    public partial class Dataset3DPanel : Form {
        public Dataset3DPanel() {
            InitializeComponent();
        }

        [Configurable, Category("Rectangle"), Description("Rows of the rectangle")]
        public int RectRows { get; set; } = 100;

        [Configurable, Category("Rectangle"), Description("Columns of the rectangle")]
        public int RectColumns { get; set; } = 50;

        private void button2_Click(object sender, EventArgs e) {
            RectangleSet rect = new RectangleSet(1, RectRows, RectColumns);
            rect.Show();
        }

        private void button1_Click(object sender, EventArgs e) {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e) {
            new SingleCell().Show();
        }

        private void button4_Click(object sender, EventArgs e) {
            Projective4D plane = new Projective4D(10, Math.PI / 500);
            plane.Show();
        }

        [Configurable, Category("Sphere"), Description("Radius of sphere")]
        public double SphereRadius { get; set; } = 15;
        [Configurable, Category("Sphere"), Description("Angle between neighboring points")]
        public double SphereAlpha { get; set; } = Math.PI / 100;

        private void button5_Click(object sender, EventArgs e) {
            Sphere sphere = new Sphere(14, SphereRadius, SphereAlpha); 
            sphere.Scale(3.0, 3.0, 3.0);
            sphere.Show();
        }

        [Configurable, Category("Torus"), Description("The size of the major radius")]
        public double TorusRadiusMajor { get; set; } = 2;
        [Configurable, Category("Torus"), Description("The size of the major radius")]
        public double TorusRadiusMinor { get; set; } = 1;
        [Configurable, Category("Torus"), Description("The points along the major radius")]
        public int TorusPointsMajor { get; set; } = 120;
        [Configurable, Category("Torus"), Description("The points along the minor radius")]
        public int TorusPointsMinor { get; set; } = 60;

        private void button6_Click(object sender, EventArgs e) {
            (new Torus(TorusRadiusMajor, TorusRadiusMinor, TorusPointsMajor, TorusPointsMinor)).Show();
        }

        private void button7_Click(object sender, EventArgs e) {
            (new RandomBall(0, 1.0, RandomPoints)).Show();
        }

        [Configurable, Category("Circle"), Description("Number of data points")]
        public int CirclePoints { get; set; } = 1000;

        private void button8_Click(object sender, EventArgs e) {
            (new Circle(0, 500, CirclePoints)).Show();
        }

        [Configurable, Category("Disc"), Description("The disc radius")]
        public double DiscRadius { get; set; } = 50.0;
        [Configurable, Category("Disc"), Description("The length between two neibhroing points")]
        public double DiscResulotion { get; set; } = 1.5;

        private void button9_Click(object sender, EventArgs e) {
            (new Disc(0, DiscRadius, DiscResulotion)).Show();
        }

        [Configurable, Category("Random Ball"), Description("Number of data points")]
        public int RandomPoints { get; set; } = 2000;

        private void button10_Click(object sender, EventArgs e) {
            (new Line(0, 400, 200)).Show();
        }

        private void button12_Click(object sender, EventArgs e) {
            (new RandomCubic(0, 100, 2000)).Show();
        }

        private void button11_Click(object sender, EventArgs e) {
            (new SingleHelix(0, 5, 0.25, Math.PI / 24.0, 12 * Math.PI)).Show();
        }

        [Configurable, Category("Spheroid"), Description("The length of the major radius")]
        public double RadiusMajor { get; set; } = 10.0;
        [Configurable, Category("Spheroid"), Description("The length of the minor radius")]
        public double RadiusMinor { get; set; } = 5.0;
        [Configurable, Category("Spheroid"), Description("Distance between two neighboring points")]
        public double ArcLen { get; set; } = 0.5;
        private void button14_Click(object sender, EventArgs e) {
            (new Spheroid(0, RadiusMajor, RadiusMinor, ArcLen)).Show();
        }

        private void button15_Click(object sender, EventArgs e) {
            (new Triangle(0, 100.0)).Show();
        }

        private void button17_Click(object sender, EventArgs e) {
            (new OpenBox(50)).Show();
        }

        [Configurable, Category("Gaussian"), Description("Number of points in the first blob")]
        public int BlobPoints1 { get; set; } = 2000;


        [Configurable, Category("Gaussian"), Description("Number of points in the first blob")]
        public int BlobPoints2 { get; set; } = 3000;


        [Configurable, Category("Gaussian"), Description("Number of points in the first blob")]
        public int BlobPoints3 { get; set; } = 4000;

        private void Gaussian_Click(object sender, EventArgs e) {
            Gaussian g1 = new Gaussian(0, BlobPoints1);
            Gaussian g2 = new Gaussian(4, BlobPoints2);
            Gaussian g3 = new Gaussian(8, BlobPoints3);
            g2.Scale(1, 0.5, 1).Translate(3.5, 0, 0);
            g3.Scale(1.5, 1, 1).Translate(0, 5, 0);
            (g1 + g2 + g3).Show();
        }

        private void button16_Click(object sender, EventArgs e) {
            new Tetrahedron(0, 40).Show();
        }

        private void button19_Click(object sender, EventArgs e) {
            using (var cfg = new VisuMap.Controls.ConfigSettings("Parameters", 500, 700)) {
                cfg.OpenPropertyWindow(this);
            }
        }

        [Configurable, Category("Cartersian"), Description("The size of a Cartersian plane")]
        public int CartersianSize { get; set; } = 101;
        private void btnCartersian_Click(object sender, EventArgs e) {
            RectangleSet pA = new RectangleSet(0, CartersianSize, CartersianSize);
            double shift = -(CartersianSize - 1) / 2.0;
            pA.Translate(shift, shift, 0);
            var pB = pA.Clone().SetType(1);
            var pC = pA.Clone().SetType(2);
            pB.Rotate(Math.PI/2, 'y');
            pC.Rotate(Math.PI/2, 'x');
            (pA + pB + pC).Show();
        }
    }
}