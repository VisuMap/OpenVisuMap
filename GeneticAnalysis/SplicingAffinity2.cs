using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class SplicingAffinity2 : IMetric {
        double[][] intronSize;
        bool[] antiSense;

        public SplicingAffinity2() {
        }

        public double Distance(int i, int j) {
            double d = 0;
            int K = Math.Min(intronSize[i].Length, intronSize[j].Length);
            for(int k=0; k<K; k++) {
                d += intronSize[i][k] * intronSize[j][k];
            }
            return (d == 0) ? 0.00001 : d;
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            intronSize = new double[dataset.Rows][];
            antiSense = new bool[dataset.Rows];
            int strandIdx = dataset.IndexOfColumn("Strand");

            for (int row = 0; row < dataset.Rows; row++) {
                string[] beginList = dataset.GetDataAt(row, 3).Split(',');
                string[] endList = dataset.GetDataAt(row, 4).Split(',');
                if (beginList.Length != endList.Length) continue;
                double[] R = new double[beginList.Length - 1];
                intronSize[row] = R;
                for (int col = 0; col < R.Length; col++) {
                    R[col] = int.Parse(beginList[col + 1]) - int.Parse(endList[col]) - 1;
                }

                antiSense[row] = (dataset.GetDataAt(row, strandIdx) == "-1");
                if (antiSense[row]) {
                    Array.Reverse(intronSize[row]);
                }

                // normalize
                double sum = R.Sum();
                for (int col = 0; col < R.Length; col++) R[col] /= sum;
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
