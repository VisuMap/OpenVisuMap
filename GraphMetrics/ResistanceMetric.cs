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
    public class ResistanceMetric : IMetric {
        double[][] d;
        public double Distance(int bodyIndexA, int bodyIndexB) {
            return d[bodyIndexA][bodyIndexB];
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            INumberTable numTable = dataset.GetNumberTable();
            int N = numTable.Rows;
            double[][] L = Matrix(N, N);
            int offset = numTable.Columns - N;
            for(int i =0; i<N; i++) {
                for (int j = 0; j < N; j++) {
                    if (i != j) {
                        L[i][j] = -numTable.Matrix[i][offset + j];
                    } else {
                        L[i][j] = 0;
                    }
                }
            }
            for (int i = 0; i < N; i++) {
                double rowSum = 0;
                for (int j = 0; j < N; j++) {
                    rowSum += L[i][j];
                }
                L[i][i] = -rowSum;
            }

            IMathAdaptor math = GraphMetrics.App.GetMathAdaptor();
            double[][] H = math.InvertMatrix(L);

            d = Matrix(N, N);
            for (int i = 0; i < N; i++) {
                for (int j = 0; j < N; j++) {
                    d[i][j] = H[i][i] + H[j][j] - H[i][j] - H[j][i];
                }
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
            get { return "Graph.ResistanceMetric"; }
            set { ; }
        }

        public bool IsApplicable(IDataset dataset) {
            if (dataset.ColumnSpecList.Count < dataset.Rows) {
                return false;
            }

            int offset = dataset.Columns - dataset.Rows;
            for (int row = 0; row < dataset.Rows; row++) {
                if (! dataset.ColumnSpecList[row+offset].DataType.Equals('n')) {
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
