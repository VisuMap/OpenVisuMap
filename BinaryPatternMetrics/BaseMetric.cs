using System;
using System.Collections.Generic;
using System.Xml;
using VisuMap.Plugin;
using VisuMap.Script;

namespace BinaryPatternMetrics {
    /// <summary>
    /// The base class for binary metrics.
    /// </summary>
    public abstract class BaseMetric : IMetric {
        /// <summary>
        /// The data table converted to boolean values.
        /// </summary>
        protected bool[][] table;

        /// <summary>
        /// The number data columns.
        /// </summary>
        protected int columns = 0;

        /// <summary>
        /// The name of the distance metric.
        /// </summary>
        protected string name;

        /// <summary>
        /// Constructor of this class.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        public BaseMetric(string name) {
            this.name = name;
        }

        /// <summary>
        /// Initialize the metric.
        /// </summary>
        /// <param name="dataset">The IDataset object that provides raw data.</param>
        /// <param name="filterNode">The XML node that provides data for potential filter.</param>
        /// <remarks>
        /// <para>This method extracts the numerical part of the data table and
        /// convert them to binary. Zero value will be converted to "false";
        /// no-zero values will be convert to true. 
        /// </para>
        /// <para>This method will be called by the 
        /// VisuMap plug-in framework when intializing a metric of this class.
        /// </para>
        /// </remarks>
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
        /// Counts number of bits that are true in either of two binary vectors.
        /// </summary>
        /// <param name="bodyIdxA">Index of the first binary vector.</param>
        /// <param name="bodyIdxB">Index of the second binary vector.</param>
        /// <returns>Number of true-bits in either of the two vectors.</returns>
        public int UnionCount(int bodyIdxA, int bodyIdxB) {
            int unions = 0;

            for (int col = 0; col < columns; col++) {
                if (table[bodyIdxA][col] || table[bodyIdxB][col]) {
                    unions++;
                }
            }
            return unions;
        }

        /// <summary>
        /// Counts number of bits that are true in both of two binary vectors.
        /// </summary>
        /// <param name="bodyIdxA">Index of the first binary vector.</param>
        /// <param name="bodyIdxB">Index of the second binary vector.</param>
        /// <returns>Number of true-bits in both vectors.</returns>
        public int IntersectionCount(int bodyIdxA, int bodyIdxB) {
            int intersections = 0;

            for (int col = 0; col < columns; col++) {
                if (table[bodyIdxA][col] && table[bodyIdxB][col]) {
                    intersections++;
                }
            }
            return intersections;
        }

        /// <summary>
        /// Counts the bits that are true in first but false in second binary vectors.
        /// </summary>
        /// <param name="bodyIdxA">Index of the first binary vector.</param>
        /// <param name="bodyIdxB">Index of the second binary vector.</param>
        /// <returns>Number of bits which are true in first vector and false in 
        /// second vector.</returns>
        public int SetDiffCount(int bodyIdxA, int bodyIdxB) {
            int diff = 0;

            for (int col = 0; col < columns; col++) {
                if (table[bodyIdxA][col] && !(table[bodyIdxB][col])) {
                    diff++;
                }
            }
            return diff;
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
        /// Returns true if the filter is valid for the given dataset.
        /// </summary>
        /// <param name="dataset">The dataset object.</param>
        /// <param name="filterNode">The XML filter node.</param>
        /// <returns>True if the filter is valid for the dataset.</returns>
        public bool IsApplicable(IDataset dataset, XmlElement filterNode) {
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
        /// <remarks>This method has to be implemented by a derived class.
        /// </remarks>
        public abstract double Distance(int bodyIdxA, int bodyIdxB);
    }
}
