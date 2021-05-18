/// <copyright from="2004" to="2010" company="VisuMap Technologies Inc.">
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
using VisuMap.Script;

namespace VisuMap.WaveTransforms {
    /// <summary>
    /// Class to perform Walsh-Hadamard transformation and filtering.
    /// </summary>
    /// <example>
    /// <code>
    ///     var t = vv.FindPluginObject("Transforms");
    ///     var tb = vv.Dataset.GetNumberTable();
    ///     var wt = t.NewWalsh(tb.Columns);
    ///     var newTable = wt.Filter(tb, 0.0, 0.5);
    ///     newTable.ShowValueDiagram();
    /// </code>
    /// </example>
    /// 
    public class WalshTransform : MarshalByRefObject {
        INumberTable walsh;
        int dim;
        public WalshTransform(int dataDim) {
            dim = HaarTransform.RoundDimension(dataDim);
            walsh = WaveTransforms.App.ScriptApp.New.NumberTable(dim, dim);

            //
            // The Walsh matrix differs from Hadamard in ordering of the base vectors:
            // the former uses the sequency order (bit-reversal or Gray code permutation)
            // while the latter uses nature order resulted
            // from recursive calculation.
            //
            // The following code is adapted from http://www.musicdsp.org/showone.php?id=18.
            int log2 = Log2(dim);
            for (int row = 0; row < dim; row++) {
                double[] v = (walsh.Matrix as double[][])[row];
                v[row] = 1.0;

                for (int i = 0; i < log2; i++) {
                    int i2 = (1 << i);
                    for (int j = 0; j < (1 << log2); j += 2*i2) {
                        for (int k = 0; k < i2; k++) {
                            int jk = j + k;
                            double a = v[jk] + v[jk + i2];
                            v[jk + i2] = v[jk] - v[jk + i2];
                            v[jk] = a;
                        }
                    }
                }
            }

            //
            // perform the bit-reserve and gray code re-ordering.
            //
            int[] order = new int[dim];
            int[] grayCode = GrayOrder(log2);
            for (int i = 0; i < dim; i++) {
                order[i] = ReverseOrder(log2, grayCode[i]);
            }

            // Permutate the rows.
            double[][] newM = new double[dim][];
            double[][] M = walsh.Matrix as double[][];
            for (int row = 0; row < dim; row++) {
                newM[row] = M[order[row]]; 
            }
            for (int row = 0; row < dim; row++) {
                M[row] = newM[row];
            }

            int interval = dim / 16;
            for (int k = 0; k < dim; k++) {
                walsh.RowSpecList[k].Type = walsh.ColumnSpecList[k].Group = (short)( k / interval );
            }

            // Remove the redudant rows.
            List<int> rows = new List<int>();
            for(int row=0; row<dataDim; row++) {
                rows.Add(row);
            }
            walsh = walsh.SelectRows(rows);

            // Normalize the matrix.
            double normFactor = Math.Pow(2, - log2 / 2.0);
            for (int row = 0; row < walsh.Rows; row++) {
                for (int col = 0; col < walsh.Columns; col++) {
                    walsh.Matrix[row][col] *= normFactor;
                }
            }
        }

        int[] GrayOrder(int n) {
            int N = 1<<n;
            int[] order = new int[N];
            Array.Clear(order, 0, N);            
            for (int i = 0; i < n; i++) {
                int len = 1<<i;
                for (int k = 0; k < len; k++) {
                    order[len + k] = order[len - 1 - k] | len;
                }
            }
            return order;
        }

        static int ReverseOrder(int len, int x) {
            int h = 0;
            for(int i = 0; i < len; i++) {
                h = (h << 1) + (x & 1); 
                x >>= 1; 
            }
            return h;
        }

        public INumberTable Transform(INumberTable inTable) {
            return FourierTransform.MatrixProduct(inTable, walsh);
        }

        public INumberTable BaseMatrix {
            get { return walsh; }
        }

        public static int Log2(int n) {
            int log2 = 0;
            while (n >= 2) {
                log2++;
                n = n>>1;
            }
            return log2;
        }

        public INumberTable Filter(INumberTable inTable, double lowFreq, double highFreq) {
            return Filter(inTable, lowFreq, highFreq, dim, walsh);
        }

        public static INumberTable Filter(INumberTable inTable, double lowFreq, double highFreq, int dimension, INumberTable baseTable) {
            int lowIdx = (int)(lowFreq * dimension);
            int hiIdx = (int)(highFreq * dimension);
            List<int> columns = new List<int>();
            for (int i = 0; i < dimension; i++) {
                if ((i >= lowIdx) && (i <= hiIdx)) {
                    columns.Add(i);
                }
            }

            INumberTable B = baseTable.SelectColumns(columns);
            INumberTable Bt = B.Transpose2();
            int b = B.Columns;
            int r = inTable.Rows;
            int m = inTable.Columns;
            INumberTable outTable = null;

            //Depending on the size of m relative to r and b, we can 
            //optimize the calculation by arrange the matrix multiplication.
            if (2 * r * b < m * (r + b)) { 
                // The complexity in this arrangement is 2*m*r*b.
                outTable = FourierTransform.MatrixProduct(FourierTransform.MatrixProduct(inTable, B), Bt);
            } else {
                // The complexity in this arrangement is m*m*(r+b).
                outTable = FourierTransform.MatrixProduct(inTable, FourierTransform.MatrixProduct(B, Bt));
            }

            IList<IColumnSpec> oSpecList = outTable.ColumnSpecList;
            IList<IColumnSpec> iSpecList = inTable.ColumnSpecList;
            for (int col = 0; col < outTable.Columns; col++) {
                oSpecList[col].CopyFrom(iSpecList[col]);
            }

            return outTable;
        }
    }
}
