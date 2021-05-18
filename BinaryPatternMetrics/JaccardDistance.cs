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

namespace BinaryPatternMetrics {

    /// <summary>
    /// Implementation of the Jaccard distance for binary vectors.
    /// </summary>
    public class JaccardDistance : BaseBinaryMetric {
        public JaccardDistance() : base("Binary.Jaccard Distance") { }

        /// <summary>
        /// Returns the Jaccard distnce of two binary vectors.
        /// </summary>
        /// <returns>The Jaccard distance between the two vectors.</returns>
        public override double Distance(int N00, int N10, int N01, int N11, int Dimmension) {
            int unions = N10 + N01 + N11;
            if (unions == 0) {
                return 1;
            } else {
                return (N10 + N01) / (double)unions;
            }
        }
    }
}
