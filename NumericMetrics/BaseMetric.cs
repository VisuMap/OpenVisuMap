using System;
using System.Collections.Generic;
using System.Xml;
using VisuMap.Plugin;
using VisuMap.Script;

namespace NumericMetrics {
    public abstract class BaseMetric : IMetric {
        protected double[][] table;
        protected int columns = 0;
        protected string name;

        public BaseMetric(string name) {
            this.name = name;
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            IList<IColumnSpec> columnList = dataset.ColumnSpecList;
            columns = 0;
            foreach (IColumnSpec cs in columnList) {
                if (cs.DataType == 'n') {
                    columns++;
                }
            }
            table = new double[dataset.Rows][];

            for (int row = 0; row < dataset.Rows; row++) {
                table[row] = new double[columns];
                int col = 0;
                for (int ch = 0; ch < dataset.Columns; ch++) {
                    if (columnList[ch].DataType == 'n') {
                        table[row][col] = double.Parse(dataset.GetDataAt(row, ch));
                        col++;
                    }
                }
            }
        }

        public abstract double Distance(int bodyIdxA, int bodyIdxB);

        public bool IsApplicable(IDataset dataset) {
            return true;
        }

        public bool IsApplicable(IDataset dataset, XmlElement filterNode) {
            return true;
        }

        public IFilterEditor FilterEditor {
            get { return new FilterEditor(); }
        }

        public string Name { get { return name; } set { ;} }
    }
}
