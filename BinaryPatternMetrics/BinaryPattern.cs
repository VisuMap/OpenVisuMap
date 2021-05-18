using System;
using System.Collections.Generic;
using System.Xml;
using VisuMap.Plugin;
using VisuMap.Script;

namespace BinaryPatternMetrics {
    /// <summary>
    /// The base class for most binary metrics.
    /// </summary>
    public abstract class BinaryPattern : IMetric {
        protected bool[][] table;
        protected int columns = 0;
        protected string name;

        /// <summary>
        /// Constructs of object.
        /// </summary>
        /// <param name="name"></param>
        public BinaryPattern(string name) {
            this.name = name;
        }

        /// <summary>
        /// Initialize the metric.
        /// </summary>
        /// <param name="dataset">The IDataset object that provides raw data.</param>
        /// <param name="filterNode">The XML node that provides data for potential filter.</param>
        public void Initialize(IDataset dataset, XmlElement filterNode) {
            IList<IColumnSpec> columnList = dataset.ColumnSpecList;
            columns = 0;
            foreach (IColumnSpec cs in columnList) {
                if (cs.DataType == 'n') {
                    columns++;
                }
            }
            table = new bool[dataset.Rows][];
            for (int row = 0; row < dataset.Rows; row++) {
                table[row] = new bool[columns];
                int col = 0;
                for (int ch = 0; ch < dataset.Columns; ch++) {
                    if (columnList[ch].DataType == 'n') {
                        table[row][col] = (double.Parse(dataset.GetDataAt(row, ch)) != 0);
                        col++;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the unions and intersections of two binary vectors.
        /// </summary>
        /// <param name="bodyIdxA">The index for the first binary vector.</param>
        /// <param name="bodyIdxB">The index for the first binary vector.</param>
        /// <param name="unions">Number of columns at which at least one of the two vectors has the value 1.</param>
        /// <param name="intersections">Number of columns at which both vectors have the value 1.</param>
        public void SetUnionIntersection(int bodyIdxA, int bodyIdxB, out int unions, out int intersections) {
            unions = 0;
            intersections = 0;

            for (int col = 0; col < columns; col++) {
                if (table[bodyIdxA][col] && table[bodyIdxB][col]) {
                    intersections++;
                }
                if (table[bodyIdxA][col] || table[bodyIdxB][col]) {
                    unions++;
                }
            }
        }

        /// <summary>
        /// Returns whether this metric is applicable on a given dataset.
        /// </summary>
        /// <param name="dataset">The dataset object.</param>
        /// <returns>Always tru</returns>
        /// <remarks>The metric will only use the numeric columns.</remarks>
        public bool IsApplicable(IDataset dataset) {
            return true;
        }

        /// <summary>
        /// Implmentation for a filter editor.
        /// </summary>
        /// <remarks>No implementation yet.</remarks>
        public IFilterEditor FilterEditor {
            get { return null; }
        }

        /// <summary>
        /// Gets the name of the metric.
        /// </summary>
        public string Name { get { return name; } set { ;} }

        /// <summary>
        /// Returns the distance between two binary vectors.
        /// </summary>
        /// <param name="bodyIdxA">The index for the first vector.</param>
        /// <param name="bodyIdxB">The index for the second vector.</param>
        /// <returns>The distance between the two vectors.</returns>
        public abstract double Distance(int bodyIdxA, int bodyIdxB);

    }
}
