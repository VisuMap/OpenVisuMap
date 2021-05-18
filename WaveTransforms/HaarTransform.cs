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
    /// Class to perform Haar transformation and filtering.
    /// </summary>
    /// <example>
    /// <code>
    ///     var t = vv.FindPluginObject("WaveTransforms");
    ///     var tb = vv.Dataset.GetNumberTable();
    ///     var ht = t.NewHaar(tb.Columns);
    ///     var newTable = ht.Filter(tb, 0.0, 0.5);
    ///     newTable.ShowValueDiagram();
    /// </code>
    /// </example>
    /// 
    public class HaarTransform : MarshalByRefObject {
        INumberTable haar;
        int dim;

        public HaarTransform(int dataDim) {
            dim = RoundDimension(dataDim);
            haar = WaveTransforms.App.ScriptApp.New.NumberTable(dim, dim);


            // Algorithm adapated from: http://www.cs.ucf.edu/~mali/haar/.
            double sqrt2 = 1 / Math.Sqrt(2.0);
            double[] vp = new double[dim];
            for (int row = 0; row < dim; row++) {
                double[] v = (haar.Matrix as double[][])[row];
                Array.Clear(v, 0, v.Length);
                v[row] = 1.0;
                Array.Clear(vp, 0, vp.Length);

                int w = dim;
                while (w > 1) {
                    w /= 2;
                    for (int i = 0; i < w; i++) {
                        vp[i] = (v[2 * i] + v[2 * i + 1]) * sqrt2;
                        vp[i + w] = (v[2 * i] - v[2 * i + 1]) * sqrt2;
                    }
                    Array.Copy(vp, v, 2 * w);
                }
            }

            int interval = dim / 16;
            for (int k = 0; k < dim; k++) {
                haar.RowSpecList[k].Type = haar.ColumnSpecList[k].Group = (short)(k / interval);
            }

            List<int> rows = new List<int>();
            for(int row=0; row<dataDim; row++) {
                rows.Add(row);
            }
            haar = haar.SelectRows(rows);
        }

        public static int RoundDimension(int dim) {
            int i;
            for (i = 31; i >=0; i--) {
                if (((1 << i) & dim) != 0) {
                    break;
                }
            }
            if (dim == (1 << i)) {
                return dim;
            } else {
                return (1 << (i + 1));
            }
        }

        public INumberTable Transform(INumberTable inTable) {
            return FourierTransform.MatrixProduct(inTable, haar);
        }

        public INumberTable Filter(INumberTable inTable, double lowFreq, double highFreq) {
            return WalshTransform.Filter(inTable, lowFreq, highFreq, dim, haar);
        }


        public INumberTable BaseMatrix {
            get { return haar; }
        }
    }
}
