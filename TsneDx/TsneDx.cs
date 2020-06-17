using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TsneDx {
    [ComVisible(true)]
    public class TsneDx {
        static void Main(string[] args) {
            string inFile = args[0];
            double perplexityRatio = (args.Length == 2) ? double.Parse(args[1]) : 0.05;
            int epochs = (args.Length == 3) ? int.Parse(args[2]) : 500;
            uint outDim = (args.Length == 4) ? uint.Parse(args[3]) : 2;

            if ( ! inFile.EndsWith(".csv") ) {
                Console.WriteLine("Usage:  TsneDx.exe <input-file>.csv");
                return;
            }
            
            double[][] X = ReadCsvFile(inFile);
            using (TsneMap tsne = new TsneMap() { PerplexityRatio = perplexityRatio, MaxEpochs= epochs, OutDim=outDim }) {
                double[][] Y = tsne.Fit(X);
                WriteCsvFile(Y, inFile.Substring(0, inFile.Length - 4) + "_map.csv");
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
                        if (col > 0) wt.Write(',');
                        wt.Write(R[col]);
                    }
                    wt.WriteLine();
                }
            }
        }

    }
}
