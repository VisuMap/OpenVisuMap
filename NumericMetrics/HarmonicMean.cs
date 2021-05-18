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
    public class HarmonicMean : IVectorMetric {
        public HarmonicMean() {
        }

        public override string Name {
            get { return "Numerical.Harmonic Mean"; }
            set { ; }
        }

        // The (-1)-th Minkovsky distance.
        public override double Distance(double[] vectorA, double[] vectorB) {
            int columns = vectorA.Length;
            double d = 0;

            for(int i=0; i<columns; i++) {
                double diff = Math.Abs(vectorA[i] - vectorB[i]);
                if (diff > 0) {
                    d += 1 / diff;
                }
            }
            return (d > 0) ? columns / d : 0;
        }
    }
}
