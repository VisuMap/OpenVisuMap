using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CustomGlyphSets {
    public partial class ColorWheel : Form {
        Bitmap bmp;
        public ColorWheel() {
            InitializeComponent();
            this.bmp = draw2();
        }

        // The following code are ported from from samples at: 
        //   http://viziblr.com/news/2011/12/1/drawing-a-color-hue-wheel-with-c.html.
        private static Bitmap draw2() {
            int padding = 10;
            int inner_radius = 200;
            int outer_radius = inner_radius + 50;

            int bmp_width = (2 * outer_radius) + (2 * padding);
            int bmp_height = bmp_width;

            Point center = new Point(bmp_width / 2, bmp_height / 2);
            Color c = Color.Red;

            Bitmap bmp = new Bitmap(bmp_width, bmp_height);
            using (Graphics g = Graphics.FromImage(bmp)) {
                g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
            }
            for (int y = 0; y < bmp_width; y++) {
                int dy = (center.Y - y);

                for (int x = 0; x < bmp_width; x++) {
                    int dx = (center.X - x);

                    double dist = System.Math.Sqrt(dx * dx + dy * dy);


                    if (dist >= inner_radius && dist <= outer_radius) {
                        double theta = System.Math.Atan2(dy, dx);


                        bmp.SetPixel(x, y, ColorAtAngle(theta));
                    }
                }
            }
            return bmp;
        }

        // theta can go from -pi to pi
        public static Color ColorAtAngle(double theta) {
            double hue = (theta + System.Math.PI) / (2 * System.Math.PI);

            // HSV -> RGB
            const double sat = 1.0;
            const double val = 1.0;
            return HSVToRGB2(hue, sat, val);            
        }

        public static void HSVToRGB(double H, double S, double V, out double R, out double G, out double B) {
            if (H == 1.0) {
                H = 0.0;
            }

            double step = 1.0 / 6.0;
            double vh = H / step;

            int i = (int)System.Math.Floor(vh);

            double f = vh - i;
            double p = V * (1.0 - S);
            double q = V * (1.0 - (S * f));
            double t = V * (1.0 - (S * (1.0 - f)));

            switch (i) {
                case 0: {
                        R = V;
                        G = t;
                        B = p;
                        break;
                    }
                case 1: {
                        R = q;
                        G = V;
                        B = p;
                        break;
                    }
                case 2: {
                        R = p;
                        G = V;
                        B = t;
                        break;
                    }
                case 3: {
                        R = p;
                        G = q;
                        B = V;
                        break;
                    }
                case 4: {
                        R = t;
                        G = p;
                        B = V;
                        break;
                    }
                case 5: {
                        R = V;
                        G = p;
                        B = q;
                        break;
                    }
                default: {
                        // not possible - if we get here it is an internal error
                        throw new ArgumentException();
                    }
            }
        }

        public static Color HSVToRGB2(double H, double S, double V) {
            double dr, dg, db;
            HSVToRGB(H, S, V, out dr, out dg, out db);
            Color c = Color.FromArgb((int)(dr * 255), (int)(dg * 255), (int)(db * 255));
            return c;

        }

        private void ColorWheel_Paint(object sender, PaintEventArgs e) {
            e.Graphics.DrawImage(bmp, 0, 0);
        }
    }
}