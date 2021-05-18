using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class MotifAffinity : IMetric {
        int[][] motifPos;

        public MotifAffinity() {
        }

        public double Distance(int i, int j) {
            int aff = 0;
            var R = motifPos[i];
            var S = motifPos[j];
            int ii = 0;
            for (int jj = 0; jj < S.Length; jj++) {
                for (; ii < R.Length; ii++) {
                    if (R[ii] < S[jj]) {
                        continue;
                    } else {
                        if (R[ii] == S[jj]) {
                            aff++;
                            ii++;
                        }
                        break;
                    }
                }
            }
            return aff;
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            motifPos = new int[dataset.Rows][];
            for (int row = 0; row < dataset.Rows; row++) {
                string[] posList = dataset.GetDataAt(row, 0).Split(',');
                int[] R = new int[posList.Length];
                for (int i = 0; i < R.Length; i++)
                    R[i] = int.Parse(posList[i]);
                motifPos[row] = R;
            }
        }

        public string Name {
            get { return "MotifAffinity"; }
            set { ; }
        }

        public bool IsApplicable(IDataset dataset) {
            var csList = dataset.ColumnSpecList;
            if (csList[0].IsEnumerate) {
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