/// <copyright from="2004" to="2016" company="VisuMap Technologies Inc.">
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
using VisuMap.Plugin;
using INumberTable = VisuMap.Script.INumberTable;

namespace VisuMap.NumericMetrics {
    public class Covariance : IVectorMetric {
        double maxVariance;
        public Covariance() {
        }
        
        public override void Initialize(INumberTable numberTable) {
            var m = numberTable.Matrix;
            for (int i = 0; i < numberTable.Rows; i++) {
                double mean = 0;
                for (int j = 0; j < numberTable.Columns; j++) {
                    mean += m[i][j];
                }
                mean /= numberTable.Columns;
                for (int j = 0; j < numberTable.Columns; j++) {
                    m[i][j] -= mean;
                }
            }

            // Estimates the maximal variance.
            maxVariance = 0;
            Random rg = new System.Random();
            for (int n = 0; n < 500; n++) {
                int i = rg.Next() % numberTable.Rows;
                int j = rg.Next() % numberTable.Rows;
                for (int k = 0; k < numberTable.Columns; k++) {
                    maxVariance = Math.Max(maxVariance, Math.Abs(m[i][k] * m[j][k]));
                }
            }
        }

        public override string Name {
            get { return "Numerical.Covariance Affinity"; }
            set { ; }
        }

        public override double Distance(double[] vectorA, double[] vectorB) {
            double d = 0;
            for (int i = 0; i < vectorA.Length; i++) {
                d += vectorA[i] * vectorB[i];
            }
            return 1 / (1 + Math.Exp(-d / maxVariance));
        }
    }
}
