using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VisuMap.Script;


namespace DataGenerator {
    public partial class SingleCell : Form {
        double maxExpression = 10.0;
        Random rg = null;

        public SingleCell() {
            InitializeComponent();
        }

        double[] NewGaussianSampling(int n, double mean) {
            double[] v = new double[n];
            double stdDev = mean / 3.0;
            for (int i = 0; i < n; i++) {
                double u1 = 1.0 - rg.NextDouble(); //uniform(0,1] random doubles
                double u2 = 1.0 - rg.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                             Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                v[i] = Math.Max(0, mean + stdDev * randStdNormal); //random normal(mean,stdDev^2)
            }
            return v;
        }

        double[] InitProfile(int columns, double expRate) {
            double[] profile = new double[columns];
            for (int col = 0; col < columns; col++)
                if (rg.NextDouble() < expRate)
                    profile[col] = rg.NextDouble() * maxExpression;
            return profile;
        }

        double[] NewProfile(double[] baseProf, double mutationRate) {
            int columns = baseProf.Length;
            double[] prof = new double[columns];
            for (int col = 0; col < columns; col++) {
                if (rg.NextDouble() < mutationRate) {
                    if (baseProf[col] == 0) {
                        prof[col] = rg.NextDouble() * maxExpression;
                    } else {
                        prof[col] += (rg.NextDouble() - 0.5) * maxExpression;
                        prof[col] = Math.Max(0, baseProf[col]);
                    }
                } else
                    prof[col] = baseProf[col];
            }
            return prof;
        }
        public double[] AddProfile(double[] a, double[] b) {
            double[] p = new double[a.Length];
            for (int col = 0; col < a.Length; col++)
                p[col] = a[col] + b[col];
            return p;
        }

        void AddCluster(INumberTable nt, double[] expLevel, int count, short type) {
            int offset = (nt.Tag==null) ? 0 : (int)(nt.Tag);
            nt.Tag = offset + count;
            double[][] m = nt.Matrix as double[][];
            int columns = expLevel.Length;
            for (int col = 0; col < columns; col++) {
                double mean = expLevel[col];
                if (mean > 0) {
                    double[] v = NewGaussianSampling(count, expLevel[col]);
                    for (int row = 0; row < count; row++)
                        m[offset + row][col] = v[row];
                }
            }

            for (int row = offset; row < (offset+ count); row++)
                nt.RowSpecList[row].Type = type;
        }

        private void btnGenerate_Click(object sender, EventArgs e) {
            int rows = int.Parse(tboxRows.Text);
            int columns = int.Parse(tboxColumns.Text);
            var nt = DataGenerator.App.ScriptApp.New.NumberTable(3*rows, columns);
            
            rg = new Random();
            double[][] m = nt.Matrix as double[][];

            /*
            double[] p = InitProfile(columns, 0.5);
            AddCluster(nt, p, 1000, 0);

            var p1 = NewProfile(p, 0.025);
            AddCluster(nt, p1, 1000, 1);

            var p2 = NewProfile(p, 0.01);
            AddCluster(nt, p2, 500, 2);

            var p3 = AddProfile(p1, p2);
            AddCluster(nt, p3, 500, 3);
            */

            double[] p = InitProfile(columns, 0.15);
            AddCluster(nt, p, 1000, 0);

            var p1 = NewProfile(p, 0.35);
            AddCluster(nt, p1, 1000, 1);

            var p2 = NewProfile(p, 0.1);
            AddCluster(nt, p2, 500, 2);

            var p3 = AddProfile(p1, p2);
            AddCluster(nt, p3, 500, 3);


            nt.ShowHeatMap();

            this.Close();
        }

    }
}
