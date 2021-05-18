/// <copyright from="2004" to="2010" company="VisuMap Technologies Inc.">
///   Copyright (C) VisuMap Technologies Inc.
/// 
///   Permission to use, copy, modify, distribute and sell this 
///   software and its documentation for any purpose is hereby 
///   granted without fee, provided that the above copyright notice 
///   appear in all copies and that both that copyright notice and 
///   this permission notice appear in supporting documentation. 
///   VisuMap Technologies Company makes no representations about the 
///   suitability of this software for any purpose. It is provided 
///   "as is" without explicit or implied warranty. 
/// </copyright>

using System;
using System.Collections.Generic;
using System.Xml;
using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GraphMetrics {
    public class MinimalPathMetric : IMetric {
        double[][] d;
        public double Distance(int bodyIndexA, int bodyIndexB) {
            return d[bodyIndexA][bodyIndexB];
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            INumberTable numTable = dataset.GetNumberTable();
            int N = numTable.Rows;
            d = Matrix(N, N);
            int offset = numTable.Columns - N;
            for (int i = 0; i < N; i++) {
                for (int j = 0; j < N; j++) {
                    d[i][j] = double.PositiveInfinity;
                    if ( i != j) {
                        double v = numTable.Matrix[i][offset + j];
                        if (v != 0) {
                            d[i][j] = v;
                        }
                    }
                }
            }
            //Apply the Floyd–Warshall algorithm with some optimization
            //posted by Valerio Schiavoni (http://www.jroller.com/vschiavoni/entry/a_fast_java_implementation_of).
            for (int k = 0; k < N; k++) {
                double[] dk = d[k];
                for (int i = 0; i < N; i++) {
                    if (k == i) continue;

                    double[] di = d[i];
                    double dki = (k < i) ? di[k] : dk[i];
                    if (dki == double.PositiveInfinity) continue;

                    for (int j = 0; j < Math.Min(k, i); j++) {
                        double s = dki + dk[j];
                        if (s < di[j]) di[j] = s;
                    }

                    for (int j = k + 1; j < i; j++) {
                        double s = dki + d[j][k];
                        if (s < di[j]) di[j] = s;
                    }
                }
            }

            for (int k = 0; k < N; k++) {
                d[k][k] = 0;
            }
        }

        public static double[][] Matrix(int rows, int columns) {
            double[][] m = new double[rows][];
            for (int row = 0; row < rows; row++) {
                m[row] = new double[columns];
            }
            return m;
        }


        #region Other methods & properties.
        public IFilterEditor FilterEditor {
            get { return null; }
        }

        public string Name {
            get { return "Graph.MinimalPath"; }
            set { ; }
        }

        public bool IsApplicable(IDataset dataset) {
            if (dataset.ColumnSpecList.Count < dataset.Rows) {
                return false;
            }

            int offset = dataset.Columns - dataset.Rows;
            for (int row = 0; row < dataset.Rows; row++) {
                if (!dataset.ColumnSpecList[row + offset].DataType.Equals('n')) {
                    return false;
                }

                if (dataset.ColumnSpecList[row + offset].Id != dataset.BodyList[row].Id) {
                    return false;
                }
            }
            return true;
        }

        public bool IsApplicable(IDataset dataset, XmlElement filterNode) {
            return false;
        }
        #endregion
    }
}
