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
    /// Class to perform Daubechies D4 transformation and filtering.
    /// </summary>
    /// <example>
    /// <code>
    ///     var t = vv.FindPluginObject("Transforms");
    ///     var tb = vv.Dataset.GetNumberTable();
    ///     var wt = t.NewWaveletD4(tb.Columns);
    ///     var newTable = wt.Filter(tb, 0.0, 0.5);
    ///     newTable.ShowValueDiagram();
    /// </code>
    /// </example>
    /// 
    public class WaveletD4Transform  : MarshalByRefObject {
        // Source of algorithm: http://en.wikipedia.org/wiki/Daubechies_wavelet#Transform.2C_D4.
        INumberTable D4;
        int dim;
        double h0, h1, h2, h3, g0, g1, g2, g3;

        public WaveletD4Transform(int dataDim) {
            dim = HaarTransform.RoundDimension(dataDim);
            InitializeConstants();
            D4 = WaveTransforms.App.ScriptApp.New.NumberTable(dim, dim);

            // Algorithm adapated from: http://www.bearcave.com/misl/misl_tech/wavelets/daubechies/daub.java
            for (int row = 0; row < dim; row++) {
                double[] v = (D4.Matrix as double[][])[row];
                Array.Clear(v, 0, v.Length);
                v[row] = 1.0;
                for (int n = v.Length; n >= 4; n >>= 1) {
                    TransformD4(v, n);
                }
            }

            int interval = dim / 16;
            for (int k = 0; k < dim; k++) {
                D4.RowSpecList[k].Type = D4.ColumnSpecList[k].Group = (short)(k / interval);
            }

            List<int> rows = new List<int>();
            for(int row=0; row<dataDim; row++) {
                rows.Add(row);
            }
            D4 = D4.SelectRows(rows);
        }

        void InitializeConstants() {
            h0 = (1 + Math.Sqrt(3)) / (4 * Math.Sqrt(2));
            h1 = (3 + Math.Sqrt(3)) / (4 * Math.Sqrt(2));
            h2 = (3 - Math.Sqrt(3)) / (4 * Math.Sqrt(2));
            h3 = (1 - Math.Sqrt(3)) / (4 * Math.Sqrt(2));

            g0 = h3;
            g1 = -h2;
            g2 = h1;
            g3 = -h0;
        }

        void TransformD4(double[] v, int n ){
            if (n >= 4) {
                int i, j;
             
                int half = n >> 1;
                double[] tmp = new double[n];

                i = 0;
                for (j = 0; j < n-3; j = j + 2) {
                    tmp[i] = v[j]*h0 + v[j+1]*h1 + v[j+2]*h2 + v[j+3]*h3;
                    tmp[i+half] = v[j]*g0 + v[j+1]*g1 + v[j+2]*g2 + v[j+3]*g3;
                    i++;
                }

                tmp[i] = v[n-2]*h0 + v[n-1]*h1 + v[0]*h2 + v[1]*h3;
                tmp[i+half] = v[n-2]*g0 + v[n-1]*g1 + v[0]*g2 + v[1]*g3;

                for (i = 0; i < n; i++) {
                    v[i] = tmp[i];
                }
            }
        }

        public INumberTable Transform(INumberTable inTable) {
            return FourierTransform.MatrixProduct(inTable, D4);
        }

        public INumberTable Filter(INumberTable inTable, double lowFreq, double highFreq) {
            return WalshTransform.Filter(inTable, lowFreq, highFreq, dim, D4);
        }

        public INumberTable BaseMatrix {
            get { return D4; }
        }
    }
}
