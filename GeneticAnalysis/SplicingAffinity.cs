using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class SplicingAffinity : IMetric {
        int[][] seqList;
        int[] cdsLength;
        int[] geneSpan;
        bool[] antiSense;

        public SplicingAffinity() {
        }

        public double Distance(int i, int j) {
            int span = Math.Min(geneSpan[i], geneSpan[j]);
            int[] Ri = seqList[i];
            int[] Rj = seqList[j];
            int ii = 0; 
            int jj = 0;
            int iEnd = -1;
            int jEnd = -1;
            int k = 0;
            int d = 0;

            while( k < span ) {
                if (k > iEnd) {
                    ii++;
                    iEnd = Ri[ii];
                    if ((ii&1) == 0) iEnd--;
                }
                    
                if (k > jEnd) {
                    jj++;
                    jEnd = Rj[jj];
                    if ((jj&1) == 0) jEnd--;
                }

                int secEnd = Math.Min(iEnd, jEnd);
                if ((ii & 1) != (jj & 1)) {
                    d += secEnd - k + 1;
                }
                k = secEnd + 1;
            }

            if (antiSense[i] != antiSense[j]) {
                d += 10000;
            }

            return d;
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            seqList = new int[dataset.Rows][];
            cdsLength = new int[dataset.Rows];
            geneSpan = new int[dataset.Rows];
            antiSense = new bool[dataset.Rows];
            int strandIdx = dataset.IndexOfColumn("Strand");

            for (int row = 0; row < dataset.Rows; row++) {
                string[] beginList = dataset.GetDataAt(row, 3).Split(',');
                string[] endList = dataset.GetDataAt(row, 4).Split(',');
                if (beginList.Length != endList.Length) continue;
                seqList[row] = new int[2*beginList.Length];
                int cdsLen = 0;
                int minBegin = int.MaxValue;
                int maxEnd = int.MinValue;
                int[] R = seqList[row];
                for (int col = 0; col < beginList.Length; col++) {
                    int idxBegin = int.Parse(beginList[col]) - 1;
                    int idxEnd = int.Parse(endList[col]) - 1;
                    R[2*col] = idxBegin;
                    R[2*col+1] = idxEnd;
                    cdsLen += idxEnd - idxBegin + 1;
                    minBegin = Math.Min(minBegin, idxBegin);
                    maxEnd = Math.Max(maxEnd, idxEnd);
                }
                cdsLength[row] = cdsLen;
                geneSpan[row] = maxEnd - minBegin + 1;

                antiSense[row] = (dataset.GetDataAt(row, strandIdx) == "-1");
                               
                for (int col = 0; col < R.Length; col++) 
                    R[col] -= minBegin;

                if (antiSense[row]) {
                    int maxIdx = R[R.Length - 1];
                    for (int col = 0; col < R.Length; col++)
                        R[col] = maxIdx - R[col];
                    Array.Reverse(R);
                }
            }
        }

        public string Name {
            get { return "Splicing Affinity"; }
            set { ; }
        }

        public bool IsApplicable(IDataset dataset) {
            var csList = dataset.ColumnSpecList;
            if ((csList.Count >= 5) && csList[3].IsEnumerate && csList[4].IsEnumerate) {
                return true;
            } else {
                return false;
            }
        }

        public bool IsApplicable(IDataset dataset, XmlElement filterNode) {
            return false;
        }

        public IFilterEditor FilterEditor {
            get { return null; }
        }

    }
}
