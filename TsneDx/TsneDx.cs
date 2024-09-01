// Copyright (C) 2020 VisuMap Technologies Inc.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TsneDx {
    [ComVisible(true)]
    public class TsneDx {
        static void Main(string[] args) {
            if (args.Length == 0) {
                Console.WriteLine("Usage:  TsneDx.exe <input-file>.csv [perplexity-ratio] [epochs] [out-dim] [exaggeration]");
                Console.WriteLine("PCA Usage:  TsneDx.exe <out-dim> <input-file>.csv");
                return;
            }
            if (char.IsDigit(args[0][0]))
                DoPca(args);
            else
                DoTsne(args);
        }

        static void DoPca(string[] args) {
            int outDim = int.Parse(args[0]);
            string inFile = args[1];
            string outFile = inFile.Substring(0, inFile.Length - 4) + "_pc.csv";
            if (!(inFile.EndsWith(".csv") || inFile.EndsWith(".npy"))) {
                Console.WriteLine("PCA Usage:  TsneDx.exe <out-dim> <input-file>.csv");
                return;
            }
            float[][] X = inFile.EndsWith(".csv") ? ReadCsvFile(inFile) : TsneMap.ReadNumpyFile(inFile);
            if ( X == null) {
                Console.WriteLine("Cannot load input file: " + TsneMap.ErrorMsg);
                return;
            }
            var pca = new FastPca();
            var B = pca.DoPca(X, outDim);
            WriteCsvFile(B, outFile);
        }

        static void DoTsne(string[] args) { 
            string inFile = (args.Length >= 1) ? args[0] : "";
            double perplexityRatio = (args.Length >= 2) ? double.Parse(args[1]) : 0.05;
            int epochs = (args.Length >= 3) ? int.Parse(args[2]) : 500;
            int outDim = (args.Length >= 4) ? int.Parse(args[3]) : 2;
            double initExaggeration = (args.Length >= 5) ? double.Parse(args[4]) : 4.0;
            double finalExaggeration = (args.Length >= 6) ? double.Parse(args[5]) : 1.0;
            if ( ! (inFile.EndsWith(".csv")||inFile.EndsWith(".npy")) ) {
                Console.WriteLine("Usage:  TsneDx.exe <input-file>.csv [perplexity-ratio] [epochs] [out-dim] [init-exaggeration] [final-exaggeration]");
                return;
            }
            string outFile = inFile.Substring(0, inFile.Length - 4) + "_map.csv";

            Console.WriteLine("Loading file " + inFile);
            float[][] X = inFile.EndsWith(".csv") ? ReadCsvFile(inFile) : TsneMap.ReadNumpyFile(inFile);
            Console.WriteLine("Loaded table: " + X.Length + "x" + X[0].Length);
            Console.WriteLine(string.Format(
                "Running tSNE: Perpelxity Ratio:{0}, Epochs:{1}, Out Dimension:{2}, Exaggeration:{3}-{4} ...",
                perplexityRatio, epochs, outDim, initExaggeration, finalExaggeration));

            using (TsneMap tsne = new TsneMap() {
                PerplexityRatio = perplexityRatio,
                MaxEpochs = epochs,
                OutDim = outDim,
                ExaggerationInit = initExaggeration,
                ExaggerationFinal = finalExaggeration,
            }) {
                float[][] Y = tsne.Fit(X);
                WriteCsvFile(Y, outFile);
            }
        }

        public static float[][] ReadCsvFile(string fileName) {            
            using (var rd = new StreamReader(fileName)) {
                List<string> rows = new List<string>();
                while (!rd.EndOfStream) {
                    string line = rd.ReadLine().TrimStart();
                    if ( (line[0] == '#') || (line[0] == '%') )
                        continue;
                    rows.Add(line);
                }

                float[][] table = new float[rows.Count][];
                char[] sep = new char[] { '\t', ' ', ',', '|', ';' };
                Parallel.For(0, rows.Count, row => {
                    table[row] = rows[row].Split(sep).Select(s => float.Parse(s)).ToArray();
                });
                return table;
            }
        }

        public static void WriteCsvFile(float[][] Y, string fileName) {
            using (var wt = new StreamWriter(fileName)) {
                foreach (var R in Y) {
                    for (int col = 0; col < R.Length; col++) {
                        if (col > 0) wt.Write(',');
                        wt.Write(R[col].ToString("g5"));
                    }
                    wt.WriteLine();
                }
            }
        }

        public static void SafeDispose(params IDisposable[] objList) {
            foreach (var obj in objList) {
                if (obj != null) {
                    obj.Dispose();
                }
            }
        }
    }
}
