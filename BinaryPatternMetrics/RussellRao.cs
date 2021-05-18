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
using System.Collections.Generic;
using System.Text;

namespace BinaryPatternMetrics {

    /// <summary>
    /// Implementation of the Russell-Yao distance metric.
    /// </summary>
    public class RussellRao : BaseBinaryMetric {
        public RussellRao() : base("Binary.Russell-Rao Distance") { }

        /// <summary>
        /// Returns the Russell-Rao distnce of two binary vectors.
        /// </summary>
        /// <returns>The Russell-Rao distance between the two vectors.</returns>
        public override double Distance(int N00, int N10, int N01, int N11, int Dimmension) {
            if (Dimmension == 0) {
                return 1.0;
            } else {
                return ((double)(Dimmension - N11)) / Dimmension;
            }
        }

    }
}
