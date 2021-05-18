using System;
using System.Collections.Generic;
using System.Text;

namespace BinaryPatternMetrics {
    /// <summary>
    /// Implementation of the matching distance for binary vectors.
    /// </summary>
    public class MatchingDistance : BaseMetric {
        public MatchingDistance() : base("Matching Distance") { }

        /// <summary>
        /// Returns the matching distnce of two binary vectors.
        /// </summary>
        /// <param name="bodyIdxA">The index of a binary vector.</param>
        /// <param name="bodyIdxB">The index of a binary vector.</param>
        /// <returns>The matching distance between the two vectors.</returns>
        public override double Distance(int bodyIdxA, int bodyIdxB) {
            int unions = UnionCount(bodyIdxA, bodyIdxB);
            int intersections = IntersectionCount(bodyIdxA, bodyIdxB);
            return (double) (unions-intersections) / columns;
        }
    }
}
