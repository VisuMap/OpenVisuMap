using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class SmithWatermanMetric : IMetric {
        string[] motifs;
        public SmithWatermanMetric() {
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            motifs = new string[dataset.Rows];
            for (int row = 0; row < dataset.Rows; row++) {
                motifs[row] = dataset.GetDataAt(row, 0);
            }
            maxLength = motifs.Max(s => s.Length);
            mem = null;  // This is only for the single threaded execution.

            string settings = GeneticAnalysis.App.ScriptApp.GetProperty("GeneticAnalysis.SmithWaterman", null);
            if (settings != null) {
                string[] fs = settings.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                gapCost = short.Parse(fs[0]);
                minMismatchScore = short.Parse(fs[1]);
                matchScore = short.Parse(fs[2]);
                mismatchScore = short.Parse(fs[3]);
            }
            minScore = (short)Math.Max(minMismatchScore, (short)-gapCost);
        }

        short gapCost = 1;
        short minMismatchScore = 0;
        short matchScore = 2;
        short mismatchScore = -4;
        short minScore;

        short GetScore(string S, int i, string T, int j) {
            return ((S.Length <= i) || (T.Length <= j) || (S[i] != T[j])) ? mismatchScore : matchScore;
        }

        int maxLength;  // the maximal string length allowd
        [ThreadStatic] static short[][] mem = null;   // thread specifical memory for distance matrix, to avoid calling too often new[]
        short[][] GetThreadMemory(int n, int m) {
            if (mem == null) {
                mem = new short[maxLength + 1][];
                for (int i = 0; i < mem.Length; i++) mem[i] = new short[maxLength + 1];
            } else {
                for (int i = 0; i < n; i++) Array.Clear(mem[i], 0, m);
            }


            return mem;
        }

        public double Distance(int idxI, int idxJ) {
            string S = motifs[idxI];
            string T = motifs[idxJ];
            int n = S.Length;
            int m = T.Length;
            if (n == 0) return matchScore * m;
            if (m == 0) return matchScore * n;

            short[][] d = GetThreadMemory(n, m);
            short maxSoFar = minMismatchScore;
            for (int i = 0; i < n; i++) {
                short score = GetScore(S, i, T, 0);
                d[i][0] = (i == 0) ? (short)Math.Max(minScore, score) :
                        (short)Math.Max(Math.Max(minMismatchScore, d[i - 1][0] - gapCost), score);
                maxSoFar = Math.Max(maxSoFar, d[i][0]);
            }

            for (int j = 0; j < m; j++) {
                int score = GetScore(S, 0, T, j);
                d[0][j] = (j == 0) ? (short)Math.Max(minScore, score) :
                    (short)Math.Max(Math.Max(minMismatchScore, d[0][j - 1] - gapCost), score);
                maxSoFar = Math.Max(maxSoFar, d[0][j]);
            }

            for (int i = 1; i < n; i++) {
                for (int j = 1; j < m; j++) {
                    short score = GetScore(S, i, T, j);
                    d[i][j] = Math.Max(
                                (short)Math.Max(minMismatchScore, d[i - 1][j] - gapCost),
                                (short)Math.Max(d[i][j - 1] - gapCost, d[i - 1][j - 1] + score)
                              );
                    maxSoFar = Math.Max(maxSoFar, d[i][j]);
                }
            }

            return matchScore * Math.Max(m, n) - maxSoFar;
        }

        public string Name {
            get { return "Seq.SmithWaterman"; }
            set { ; }
        }

        public bool IsApplicable(IDataset dataset) {
            if ((dataset.Columns > 0) && (dataset.ColumnSpecList[0].DataType == 'e'))
                return true;
            else
                return false;
        }

        public bool IsApplicable(IDataset dataset, XmlElement filterNode) {
            return false;
        }

        public IFilterEditor FilterEditor {
            get { return null; }
        }

    }
}
