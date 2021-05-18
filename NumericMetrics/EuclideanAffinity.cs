/// <copyright from="2004" to="2008" company="VisuMap Technologies Inc.">
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

namespace VisuMap.NumericMetrics {
    public class EuclideanAffinity : IVectorMetric {
        double normalizeFactor;

        public EuclideanAffinity() {
            
        }

        public override void Initialize(VisuMap.Script.INumberTable numberTable) {
            normalizeFactor = 1e-10;
            for(int k=0; k<1000; k++) {
                var a = numberTable.Matrix[k * 2903 % numberTable.Rows];
                var b = numberTable.Matrix[k * 5231 % numberTable.Rows];
                double d = 0;
                for(int j=0; j<numberTable.Columns; j++) {
                    double diff = a[j] - b[j];
                    d += diff * diff;
                }
                normalizeFactor = Math.Max(normalizeFactor, d);
            }
        }

        public override string Name {
            get { return "Numerical.Euclidean Affinity"; }
            set { ; }
        }

        public override double Distance(double[] vectorA, double[] vectorB) {
            int columns = vectorA.Length;
            double d = 0;
            for (int i = 0; i < columns; i++) {
                double diff  = vectorA[i] - vectorB[i];
                d += diff * diff;
            }
            d /= normalizeFactor;
            d = Math.Sqrt(d);
            return 1/(1 + d);
        }
    }
}
