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
    /// Implementation of the Dice distance for binary vectors.
    /// </summary>
    public class DiceDistance : BaseBinaryMetric {
        public DiceDistance() : base("Binary.Dice Distance") { }

        /// <summary>
        /// Returns the Dice distnce of two binary vectors.
        /// </summary>
        /// <returns>The Dice distance between the two vectors.</returns>
        public override double Distance(int N00, int N10, int N01, int N11, int Dimmension) {
            int n = N10 + N01 + N11 + N11;
            if (n == 0) {
                return 1;
            } else {
                return ((double)(N10 + N01)) / n;
            }
        }
    }
}
