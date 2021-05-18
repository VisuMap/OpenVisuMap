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
    /// Implementing the Tanimoto distance for binary vectors.
    /// </summary>
    public class TanimotoDistance : BaseBinaryMetric {
        public TanimotoDistance() : base("Binary.Tanimoto Distance") { }

        /// <summary>
        /// Returns the Tanimoto distnce of two binary vectors.
        /// </summary>
        /// <returns>The Tanimoto distance between the two vectors.</returns>
        public override double Distance(int N00, int N10, int N01, int N11, int Dimmension) {
            int unions = N10 + N11 + N01;
            if (unions == 0) {
                return 1.0;
            } else {
                return ((double)(N10+N01)) / unions;
            }
        }
    }
}
