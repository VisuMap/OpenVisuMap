using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisuMap.DataCleansing {
    /// <summary>
    /// This module is ported from code from http://facs.stanford.edu/software/Logicle/.
    /// Full documentation can be found at: http://onlinelibrary.wiley.com/doi/10.1002/cyto.a.22030/full.
    /// </summary>
    public class FastLogicle : Logicle {
        /// <summary>
        /// The default number of entries in the interpolation lookup table
        /// </summary>
        public static int DEFAULT_BINS = 1 << 12;

        /// <summary>
        /// The number of bins for this Logicle scale
        /// <summary>
        public int bins;

        /// <summary>
        /// A lookup table of data values
        /// <summary>
        protected double[] lookup;

        /**
         * Constructs a <code>FastLogicle</code> object with all parameters
         * 
         * @param T
         *          the double maximum data value or "top of scale"
         * @param W
         *          the double number of decades to linearize
         * @param M
         *          the double number of decades that a pure log scale would cover
         * @param A
         *          the double additional number of negative decades to include on
         *          scale
         * @param bins
         *          number of data values in the lookup table
         */
        public FastLogicle(double T, double W, double M, double A, int bins)
            : base(T, W, M, A, bins) {
            this.bins = bins;
            lookup = new double[bins + 1];
            for (int i = 0; i <= bins; ++i) {
                lookup[i] = base.inverse((double)i / (double)bins);
            }
        }

        public double MinValue {
            get { return lookup[0]; }
        }

        public double MaxValue {
            get { return lookup[lookup.Length - 1]; }
        }

        /**
         * Constructs a <code>FastLogicle</code> object no additional negative decades
         * 
         * @param T
         *          the double maximum data value or "top of scale"
         * @param W
         *          the double number of decades to linearize
         * @param M
         *          the double number of decades that a pure log scale would cover
         * @param bins
         *          number of data values in the lookup table
         */
        public FastLogicle(double T, double W, double M, int bins)
            : this(T, W, M, 0, bins) {
        }

        /**
         * Constructs a <code>FastLogicle</code> object with the default number of
         * decades and no additional negative decades
         * 
         * @param T
         *          the double maximum data value or "top of scale"
         * @param W
         *          the double number of decades to linearize
         * @param bins
         *          number of data values in the lookup table
         */
        public FastLogicle(double T, double W, int bins)
            : this(T, W, DEFAULT_DECADES, 0, bins) {
        }

        /**
         * Constructs a <code>FastLogicle</code> object with the default number of
         * bins
         * 
         * @param T
         *          the double maximum data value or "top of scale"
         * @param W
         *          the double number of decades to linearize
         * @param M
         *          the double number of decades that a pure log scale would cover
         * @param A
         *          the double additional number of negative decades to include on
         *          scale
         */
        public FastLogicle(double T, double W, double M, double A)
            : this(T, W, M, A, DEFAULT_BINS) {
        }

        /**
         * Constructs a <code>FastLogicle</code> object with the default number of
         * bins and no additional negative decades
         * 
         * @param T
         *          the double maximum data value or "top of scale"
         * @param W
         *          the double number of decades to linearize
         * @param M
         *          the double number of decades that a pure log scale would cover
         */
        public FastLogicle(double T, double W, double M)
            : this(T, W, M, 0, DEFAULT_BINS) {
        }

        /**
         * Constructs a <code>FastLogicle</code> object with the default number of
         * bins, the default number of decades and no additional negative decades
         * 
         * @param T
         *          the double maximum data value or "top of scale"
         * @param W
         *          the double number of decades to linearize
         */
        public FastLogicle(double T, double W)
            : this(T, W, DEFAULT_DECADES, 0, DEFAULT_BINS) {
        }

        /**
         * Looks up a data value in the internal table. Provides a fast method of
         * binning data on the Logicle scale
         * 
         * @param value
         *          a double data value
         * @return the bin for that data value
         * @throws LogicleArgumentException
         *           if the data is out of range
         */
        public int intScale(double value) {
            // binary search for the appropriate bin
            int lo = 0;
            int hi = bins;
            while (lo <= hi) {
                int mid = (lo + hi) >> 1;
                double key = lookup[mid];
                if (value < key)
                    hi = mid - 1;
                else if (value > key)
                    lo = mid + 1;
                else if (mid < bins)
                    return mid;
                else
                    return -1;
            }

            if (hi < 0 || lo > bins) return -1;

            return lo - 1;
        }

        /**
         * Returns the minimum data value for the specified bin
         * 
         * @param index
         *          a bin number
         * @return the double data value
         * @throws LogicalArgumentException
         *           if the index is out of range
         */
        public double inverse(int index) {
            if (index < 0 || index >= bins)
                throw new LogicleArgumentException(index);

            return lookup[index];
        }

        /**
         * Computes an approximation of the Logicle scale of the given data value.
         * This method looks up the data in the table then performs an inverse linear
         * interpolation.
         * 
         * @throws LogicleArgumentException
         *           if the data value would be off scale
         * @see edu.stanford.facs.logicle.Logicle#scale(double)
         */
        public override double scale(double value) {
            // lookup the nearest value
            int index = intScale(value);

            if (index < 0) {
                return base.scale(value);
            }


            // inverse interpolate the table linearly
            double delta = (value - lookup[index])
              / (lookup[index + 1] - lookup[index]);

            return (index + delta) / (double)bins;
        }

        /**
         * Computes the approximate data value corresponding to a scale value. This
         * method uses linear interpolation between tabulated data values.
         * 
         * @throws LogicleArgumentException
         *           if the scale is out of range
         * @see Logicle#inverse(double)
         */
        public override double inverse(double scale) {
            // find the bin
            double x = scale * bins;
            int index = (int)Math.Floor(x);
            if (index < 0 || index >= bins)
                throw new LogicleArgumentException(scale);

            // interpolate the table linearly
            double delta = x - index;

            return (1 - delta) * lookup[index] + delta * lookup[index + 1];
        }
    }


    public class Logicle {
        /**
          * Number of decades in default scale
          */
        public static double DEFAULT_DECADES = 4.5;

        /**
         * Actual parameter of the Logicle scale as implemented
         */
        public double a, b, c, d, f, w, x0, x1, x2;

        /**
         * Formal parameter of the Logicle scale as defined in the Gating-ML standard.
         */
        public double T, W, M, A;

        protected double LN_10 = Math.Log(10);

        /**
         * Scale value below which Taylor series is used
         */
        protected double xTaylor;

        /**
         * Coefficients of Taylor series expansion
         */
        protected double[] taylor;

        /**
         * Real constructor that does all the work. Called only from implementing
         * classes.
         * 
         * @param T
         *          maximum data value or "top of scale"
         * @param W
         *          number of decades to linearize
         * @param M
         *          number of decades that a pure log scale would cover
         * @param A
         *          additional number of negative decades to include on scale
         * @param bins
         *          number of bins in the lookup table
         */
        protected Logicle(double T, double W, double M, double A, int bins) {
            if (T <= 0)
                throw new LogicleParameterException("T is not positive");
            if (W < 0)
                throw new LogicleParameterException("W is negative");
            if (M <= 0)
                throw new LogicleParameterException("M is not positive");
            if (2 * W > M)
                throw new LogicleParameterException("W is too large");
            if (-A > W || A + W > M - W)
                throw new LogicleParameterException("A is too large");

            // if we're going to bin the data make sure that
            // zero is on a bin boundary by adjusting A
            if (bins > 0) {
                double zero = (W + A) / (M + A);
                zero = Math.Round(zero * bins, 0) / bins;
                A = (M * zero - W) / (1 - zero);
            }

            // standard parameters
            this.T = T;
            this.M = M;
            this.W = W;
            this.A = A;

            // actual parameters
            // formulas from biexponential paper
            w = W / (M + A);
            x2 = A / (M + A);
            x1 = x2 + w;
            x0 = x2 + 2 * w;
            b = (M + A) * LN_10;
            d = solve(b, w);
            double c_a = Math.Exp(x0 * (b + d));
            double mf_a = Math.Exp(b * x1) - c_a / Math.Exp(d * x1);
            a = T / ((Math.Exp(b) - mf_a) - c_a / Math.Exp(d));
            c = c_a * a;
            f = -mf_a * a;

            // use Taylor series near x1, i.e., data zero to
            // avoid round off problems of formal definition
            xTaylor = x1 + w / 4;
            // compute coefficients of the Taylor series
            double posCoef = a * Math.Exp(b * x1);
            double negCoef = -c / Math.Exp(d * x1);
            // 16 is enough for full precision of typical scales
            taylor = new double[16];
            for (int i = 0; i < taylor.Length; ++i) {
                posCoef *= b / (i + 1);
                negCoef *= -d / (i + 1);
                taylor[i] = posCoef + negCoef;
            }
            taylor[1] = 0; // exact result of Logicle condition
        }

        /**
         * Constructor taking all possible parameters
         * 
         * @param T
         *          the double maximum data value or "top of scale"
         * @param W
         *          the double number of decades to linearize
         * @param M
         *          the double number of decades that a pure log scale would cover
         * @param A
         *          the double additional number of negative decades to include on
         *          scale
         */
        public Logicle(double T, double W, double M, double A)
            : this(T, W, M, A, 0) {
        }

        /**
         * Constructor with no additional negative decades
         * 
         * @param T
         *          the double maximum data value or "top of scale"
         * @param W
         *          the double number of decades to linearize
         * @param M
         *          the double number of decades that a pure log scale would cover
         */
        public Logicle(double T, double W, double M)
            : this(T, W, M, 0D) {
        }

        /**
         * Constructor with default number of decades and no additional negative
         * decades
         * 
         * @param T
         *          the double maximum data value or "top of scale"
         * @param W
         *          the double number of decades to linearize
         */
        public Logicle(double T, double W)
            : this(T, W, DEFAULT_DECADES, 0D) {
        }

        // http://stackoverflow.com/questions/9485943/calculate-the-unit-in-the-last-place-ulp-for-doubles
        static double Math_ulp(double value) {
            Int64 bits = BitConverter.DoubleToInt64Bits(value);
            if ((bits & 0x7FF0000000000000L) == 0x7FF0000000000000L) { // if x is not finite
                if ((bits & 0x000FFFFFFFFFFFFFL) == 0) { // if x is a NaN
                    return value;  // I did not force the sign bit here with NaNs.
                }
                return BitConverter.Int64BitsToDouble(0x7FF0000000000000L); // Positive Infinity;
            }
            bits &= 0x7FFFFFFFFFFFFFFFL; // make positive
            if (bits == 0x7FEFFFFFFFFFFFFL) { // if x == max_double (notice the _E_)
                return BitConverter.Int64BitsToDouble(bits) - BitConverter.Int64BitsToDouble(bits - 1);
            }
            double nextValue = BitConverter.Int64BitsToDouble(bits + 1);
            return nextValue - value;
        }


        /**
         * Solve f(d;w,b) = 2 * (ln(d) - ln(b)) + w * (d + b) = 0 for d, given b and w
         * 
         * @param b
         * @param w
         * @return double root d
         */
        protected static double solve(double b, double w) {
            // w == 0 means its really arcsinh
            if (w == 0)
                return b;

            // precision is the same as that of b
            double tolerance = 2 * Math_ulp(b);

            // based on RTSAFE from Numerical Recipes 1st Edition
            // bracket the root
            double d_lo = 0;
            double d_hi = b;

            // bisection first step
            double d = (d_lo + d_hi) / 2;
            double last_delta = d_hi - d_lo;
            double delta;

            // evaluate the f(d;w,b) = 2 * (ln(d) - ln(b)) + w * (b + d)
            // and its derivative
            double f_b = -2 * Math.Log(b) + w * b;
            double f = 2 * Math.Log(d) + w * d + f_b;
            double last_f = Double.NaN;

            for (int i = 1; i < 20; ++i) {
                // compute the derivative
                double df = 2 / d + w;

                // if Newton's method would step outside the bracket
                // or if it isn't converging quickly enough
                if (((d - d_hi) * df - f) * ((d - d_lo) * df - f) >= 0
                  || Math.Abs(1.9 * f) > Math.Abs(last_delta * df)) {
                    // take a bisection step
                    delta = (d_hi - d_lo) / 2;
                    d = d_lo + delta;
                    if (d == d_lo)
                        return d; // nothing changed, we're done
                } else {
                    // otherwise take a Newton's method step
                    delta = f / df;
                    double t = d;
                    d -= delta;
                    if (d == t)
                        return d; // nothing changed, we're done
                }
                // if we've reached the desired precision we're done
                if (Math.Abs(delta) < tolerance)
                    return d;
                last_delta = delta;

                // recompute the function
                f = 2 * Math.Log(d) + w * d + f_b;
                if (f == 0 || f == last_f)
                    return d; // found the root or are not going to get any closer
                last_f = f;

                // update the bracketing interval
                if (f < 0)
                    d_lo = d;
                else
                    d_hi = d;
            }

            throw new Exception("exceeded maximum iterations in solve()");
        }

        /**
         * Computes the slope of the biexponential function at a scale value.
         * 
         * @param scale
         * @return The slope of the biexponential at the scale point
         */
        protected double slope(double scale) {
            // reflect negative scale regions
            if (scale < x1)
                scale = 2 * x1 - scale;

            // compute the slope of the biexponential
            return a * b * Math.Exp(b * scale) + c * d / Math.Exp(d * scale);
        }

        /**
         * Computes the value of Taylor series at a point on the scale
         * 
         * @param scale
         * @return value of the biexponential function
         */
        protected double seriesBiexponential(double scale) {
            // Taylor series is around x1
            double x = scale - x1;
            // note that taylor[1] should be identically zero according
            // to the Logicle condition so skip it here
            double sum = taylor[taylor.Length - 1] * x;
            for (int i = taylor.Length - 2; i >= 2; --i)
                sum = (sum + taylor[i]) * x;
            return (sum * x + taylor[0]) * x;
        }

        /**
         * Computes the Logicle scale value of the given data value
         * 
         * @param value a data value
         * @return the double Logicle scale value
         */
        public virtual double scale(double value) {
            // handle true zero separately
            if (value == 0)
                return x1;

            // reflect negative values
            Boolean negative = value < 0;
            if (negative)
                value = -value;

            // initial guess at solution
            double x;
            if (value < f)
                // use linear approximation in the quasi linear region
                x = x1 + value / taylor[0];
            else
                // otherwise use ordinary logarithm
                x = Math.Log(value / a) / b;

            // try for double precision unless in extended range
            double tolerance = 3 * Math_ulp(1D);
            if (x > 1)
                tolerance = 3 * Math_ulp(x);

            for (int i = 0; i < 10; ++i) {
                // compute the function and its first two derivatives
                double ae2bx = a * Math.Exp(b * x);
                double ce2mdx = c / Math.Exp(d * x);
                double y;
                if (x < xTaylor)
                    // near zero use the Taylor series
                    y = seriesBiexponential(x) - value;
                else
                    // this formulation has better roundoff behavior
                    y = (ae2bx + f) - (ce2mdx + value);
                double abe2bx = b * ae2bx;
                double cde2mdx = d * ce2mdx;
                double dy = abe2bx + cde2mdx;
                double ddy = b * abe2bx - d * cde2mdx;

                // this is Halley's method with cubic convergence
                double delta = y / (dy * (1 - y * ddy / (2 * dy * dy)));
                x -= delta;

                // if we've reached the desired precision we're done
                if (Math.Abs(delta) < tolerance)
                    // handle negative arguments
                    if (negative)
                        return 2 * x1 - x;
                    else
                        return x;
            }

            throw new Exception("scale() didn't converge");
        }

        /**
         * Computes the data value corresponding to the given point of the Logicle
         * scale. This is the inverse of the {@link Logicle#scale(double) scale}
         * function.
         * 
         * @param scale
         *          a double scale value
         * @return the double data value
         */
        public virtual double inverse(double scale) {
            // reflect negative scale regions
            Boolean negative = scale < x1;
            if (negative)
                scale = 2 * x1 - scale;

            // compute the biexponential
            double inverse;
            if (scale < xTaylor)
                // near x1, i.e., data zero use the series expansion
                inverse = seriesBiexponential(scale);
            else
                // this formulation has better roundoff behavior
                inverse = (a * Math.Exp(b * scale) + f) - c / Math.Exp(d * scale);

            // handle scale for negative values
            if (negative)
                return -inverse;
            else
                return inverse;
        }

        /**
         * Computes the dynamic range of the Logicle scale. For the Logicle scales
         * this is the ratio of the pixels per unit at the high end of the scale
         * divided by the pixels per unit at zero.
         * 
         * @return the double dynamic range
         */
        public double dynamicRange() {
            return slope(1) / slope(x1);
        }

        /**
         * Choose a suitable set of data coordinates for a Logicle scale
         * 
         * @return a double array of data values
         */
        public double[] axisLabels() {
            // number of decades in the positive logarithmic region
            double p = M - 2 * W;
            // smallest power of 10 in the region
            double log10x = Math.Ceiling(Math.Log(T) / LN_10 - p);
            // data value at that point
            double x = Math.Exp(LN_10 * log10x);
            // number of positive labels
            int np;
            if (x > T) {
                x = T;
                np = 1;
            } else
                np = (int)(Math.Floor(Math.Log(T) / LN_10 - log10x)) + 1;
            // bottom of scale
            double B = this.inverse(0);
            // number of negative labels
            int nn;
            if (x > -B)
                nn = 0;
            else if (x == T)
                nn = 1;
            else
                nn = (int)Math.Floor(Math.Log(-B) / LN_10 - log10x) + 1;

            // fill in the axis labels
            double[] label = new double[nn + np + 1];
            label[nn] = 0;
            for (int i = 1; i <= nn; ++i) {
                label[nn - i] = -x;
                label[nn + i] = x;
                x *= 10;
            }
            for (int i = nn + 1; i <= np; ++i) {
                label[nn + i] = x;
                x *= 10;
            }

            return label;
        }
    }

    /**
     * Thrown to indicate that the argument of the FastLogicle function is out of range.
     * 
     * @author Wayne A. Moore
     * @version 1.0
     */
    public class LogicleArgumentException : Exception {
        public LogicleArgumentException(double value)
            : base("Illegal argument to Logicle scale: " + value.ToString()) {
        }

        public LogicleArgumentException(int index)
            : base("Illegal argument to Logicle inverse: " + index.ToString()) {
        }
    }

    public class LogicleParameterException : Exception {
        public LogicleParameterException(String msg)
            : base(msg) {
        }
    }
}
