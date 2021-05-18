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
    ///     var wt = t.NewPca(tb);
    ///     var newTable = wt.Filter(tb, 0.0, 0.5);
    ///     newTable.ShowValueDiagram();
    /// </code>
    /// </example>
    /// 
    public class PcaTransform : MarshalByRefObject {
        INumberTable baseTable; // the base vectors are the column vectors of the table.
        double[] columnMean;
        public PcaTransform(INumberTable numberTable) {
            baseTable = numberTable.GetPcaEigenvectors(null, 0).Transpose();
            columnMean = new double[numberTable.Columns];
            IList<IList<double>> m = numberTable.Matrix;
            for (int col = 0; col < numberTable.Columns; col++) {
                columnMean[col] = 0;
                for (int row = 0; row < numberTable.Rows; row++) {
                    columnMean[col] += m[row][col];
                }
                columnMean[col] /= numberTable.Rows;
            }
        }

        public INumberTable Transform(INumberTable inTable) {
            return inTable.Multiply(baseTable);
        }

        public INumberTable BaseMatrix {
            get { return baseTable; }
        }

        public INumberTable Filter(INumberTable inTable, double lowFreq, double highFreq) {
            // Centralize the table.
            inTable = inTable.Clone();
            IList<IList<double>> m = inTable.Matrix;
            for (int row = 0; row < inTable.Rows; row++) {
                for (int col = 0; col < inTable.Columns; col++) {
                    m[row][col] -= columnMean[col];
                }
            }

            INumberTable outTable = WalshTransform.Filter(inTable, lowFreq, highFreq, baseTable.Columns, baseTable);

            m = outTable.Matrix;
            for (int row = 0; row < outTable.Rows; row++) {
                for (int col = 0; col < outTable.Columns; col++) {
                    m[row][col] += columnMean[col];
                }
            }

            return outTable;
        }
    }
}
