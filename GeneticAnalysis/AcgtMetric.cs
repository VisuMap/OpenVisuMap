using System;
using System.Xml;
using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class AcgtMetric : IMetric {
        double[][] vecList;
        const int L = 20;
        public AcgtMetric() {
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            vecList = new double[dataset.Rows][];
            var rg = new Random();
            String ACGT = "ACGT";
            double[] amplitudes1 = new double[L];
            for (int k = 0; k < L; k++) {
                amplitudes1[k] = rg.NextDouble();
            }
            double[] amplitudes2 = new double[L];
            for (int k = 0; k < L; k++) {
                amplitudes2[k] = rg.NextDouble();
            }

            for (int row = 0; row < dataset.Rows; row++) {
                vecList[row] = new double[8];
                var s = dataset.GetDataAt(row, 0);
                for (int k = 0; k < s.Length; k++) {
                    int kk = k % L;
                    int ii = ACGT.IndexOf(s[k]);
                    vecList[row][ii] += amplitudes1[kk];
                    vecList[row][4+ii] += amplitudes2[kk];
                }
            }
        }

        public string Name {
            get { return "Seq.ACGT"; }
            set { ; }
        }

        public double Distance(int idxI, int idxJ) {
            double d = 0;
            var vI = vecList[idxI];
            var vJ = vecList[idxJ];
            var N = vI.Length;
            for (int k = 0; k < N; k++) {
                double diff = vI[k] - vJ[k];
                d += diff * diff;
            }
            return Math.Sqrt(d);
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
