using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TsneDx {
    [ComVisible(true)]
    public class TsneDx {
        static void Main(string[] args) {
            double[][] X = ReadCsvFile("SP500.csv");
            using (TsneMap tsne = new TsneMap() {OutDim = 3, PerplexityRatio = 0.1}) {
                double[][] Y = tsne.Fit(X);
                WriteCsvFile(Y, "SP500Map.csv");
            }
        }

        public static double[][] ReadCsvFile(string fileName) {
            List<double[]> rows = new List<double[]>();
            using (var rd = new StreamReader(fileName)) {
                char[] sep = new char[] { '\t', ' ', ',', '|', ';' };
                while (!rd.EndOfStream) {
                    string line = rd.ReadLine().TrimStart();
                    if (!char.IsNumber(line[0]))
                        continue;
                    string[] fs = line.Split(sep);
                    rows.Add(fs.Select(s => double.Parse(s)).ToArray());
                }
            }
            return rows.ToArray();
        }

        public static void WriteCsvFile(double[][] Y, string fileName) {
            using (var wt = new StreamWriter(fileName)) {
                foreach (double[] R in Y) {
                    for (int col = 0; col < R.Length; col++) {
                        if (col > 0) wt.Write('\t');
                        wt.Write(R[col]);
                    }
                    wt.WriteLine();
                }
            }
        }

    }
}
