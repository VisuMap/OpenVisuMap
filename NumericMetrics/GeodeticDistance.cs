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
    /// <summary>
    /// Calculate the geodetic distance from the 3D coordinates of two points on 
    /// the earth surface.
    /// </summary>
    public class GeodeticDistance : IVectorMetric {
        public override string Name {
            get { return "Numerical.Geodetic Distance"; }
            set { ; }
        }

        // Quadratic Mean Radius of equatorial and polar radii (6378.135 & 6356.75).
        // double R = 6372.795477598; 
        double R = 0;
        public override double Distance(double[] vectorA, double[] vectorB) {
            double dx = vectorA[0]-vectorB[0];
            double dy = vectorA[1]-vectorB[1];
            double dz = vectorA[2]-vectorB[2];

            if (R == 0) {
                // If the coordinates is not in kilometers we calculate the radius 
                // from the data while assuming the coordinator centered a the center of earth.
                R = Math.Sqrt(4 * vectorA[0] * vectorA[0] + 4 * vectorA[1] * vectorA[1] + 4 * vectorA[2] * vectorA[2]);
            }

            double d = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            double s = 2 * R * Math.Asin(0.5*d/R);
            return s;
        }
    }
}
