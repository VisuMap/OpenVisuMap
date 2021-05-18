using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class NeedlemanWunschMetric : IMetric {
        string[] motifs;
        short gapCost = 2;
        short matchCost = 0;
        short mismatchCost = -1;

        public NeedlemanWunschMetric() {
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            motifs = new string[dataset.Rows];
            for (int row = 0; row < dataset.Rows; row++) {
                motifs[row] = dataset.GetDataAt(row, 0);
            }
            maxLength = motifs.Max(s => s.Length);
            mem = null;  // This is only for the single threaded execution.

            string settings = GeneticAnalysis.App.ScriptApp.GetProperty("GeneticAnalysis.NeedlemanWunsch", null);
            if (settings != null) {
                string[] fs = settings.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                gapCost = short.Parse(fs[0]);
                matchCost = short.Parse(fs[1]);
                mismatchCost = short.Parse(fs[2]);
            }
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
            if (n == 0) return m * gapCost;
            if (m == 0) return n * gapCost;

            short[][] d = GetThreadMemory(n+1, m+1);
            for (short i = 0; i <= n; i++) d[i][0] = (short)(i * gapCost); ;
            for (short j = 0; j <= m; j++) d[0][j] = (short)(j * gapCost);

            for (int i = 1; i <= n; i++) {
                for (int j = 1; j <= m; j++) {
                    int alignCost = (S[i - 1] == T[j - 1]) ? matchCost : mismatchCost;
                    d[i][j] = (short) Math.Min(Math.Min(
                                d[i - 1][j] + gapCost, 
                                d[i][j - 1] + gapCost),
                                d[i - 1][j - 1] + alignCost);
                }
            }
            return d[n][m];
        }

        public string Name {
            get { return "Seq.NeedlemanWunsch"; }
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
