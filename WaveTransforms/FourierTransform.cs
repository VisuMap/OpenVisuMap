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
    /// 
    /// </summary>
    /// <example>
    /// <code>
    ///   var t = vv.FindPluginObject("WaveTransforms");
    ///   var tb = vv.EventSource.Form.GetNumberTable();
    ///   var ft = t.NewFourier(tb.Columns);
    ///   ft.Filter(tb, 0.0, 0.35).ShowValueDiagram();
    /// </code>
    /// </example>
    public class FourierTransform : MarshalByRefObject {
        INumberTable matrixReal;
        INumberTable matrixImg;
        
        int tDim;  // the dimension of the transformation.
        int[] selectReal;
        int[] selectImg;

        public FourierTransform(int dimension) {
            tDim = dimension;
            matrixReal = WaveTransforms.App.ScriptApp.New.NumberTable(tDim, tDim);
            matrixImg = WaveTransforms.App.ScriptApp.New.NumberTable(tDim, tDim);
            double[][] mr = matrixReal.Matrix as double[][];
            double[][] mi = matrixImg.Matrix as double[][];
            double w = -2 * Math.PI / dimension;
            double f = 1/ Math.Sqrt(tDim);
            IList<IRowSpec> rowListReal = matrixReal.RowSpecList;
            IList<IRowSpec> rowListImg = matrixImg.RowSpecList;

            for (int i = 0; i < tDim; i++) {
                for (int j = 0; j <= i; j++) {
                    double wij = i * j * w;
                    mr[i][j] = mr[j][i] = f * Math.Cos(wij);
                    mi[i][j] = mi[j][i] = f * Math.Sin(wij);
                }
                short freq = (short) ((i < tDim / 2) ? i : (tDim - i));

                if (tDim % 2 == 0) {
                    rowListReal[i].Type = freq;
                    rowListImg[i].Type = freq;
                    if (i == tDim / 2) rowListImg[i].Type = 0;
                } else {
                    rowListReal[i].Type = freq;
                    rowListImg[i].Type = freq;
                    if (i == tDim / 2) {
                        rowListImg[i].Type = rowListReal[i].Type = (short)(tDim / 2);
                    }
                }

                matrixReal.ColumnSpecList[i].Group = rowListReal[i].Type;
                matrixImg.ColumnSpecList[i].Group = rowListImg[i].Type;
            }

            // indexes to select non-duplicate part of the fourier matrix.
            selectReal = new int[tDim / 2 + 1];
            for (int i = 0; i < selectReal.Length; i++) {
                selectReal[i] = i;
            }
            selectImg = new int[(tDim % 2 == 0) ? (tDim / 2 - 1) : (tDim / 2)];
            for (int i = 0; i < selectImg.Length; i++) {
                selectImg[i] = i + 1;
            }
        }

        public INumberTable BaseMatrixReal {
            get { return matrixReal; }
        }

        public INumberTable BaseMatrixImage {
            get { return matrixImg; }
        }

        /// <summary>
        /// Multiply two matrix with padding and repeating if dimensions don't match.
        /// </summary>
        /// <param name="left">The left matrix.</param>
        /// <param name="right">The right matrix.</param>
        /// <returns>The product of two matrix.</returns>
        public static INumberTable MatrixProduct(INumberTable left, INumberTable right) {
            int dim = right.Rows;  // The dimension of the transformation.
            int repeats = left.Columns / dim;
            if (left.Columns % dim > 0) 
                repeats++;
            int outColumns = repeats * right.Columns;

            INumberTable prod = WaveTransforms.App.ScriptApp.New.NumberTable(left.Rows, outColumns);
            double[][] P = (double[][])prod.Matrix;
            double[][] L = (double[][])left.Matrix;
            double[][] R = (double[][])right.Matrix;

            MT.Loop(0, prod.Rows, row => {
                double[] Lrow = L[row];
                double[] Prow = P[row];
                for (int col = 0; col < prod.Columns; col++) {
                    int col2 = col % right.Columns;
                    int k0 = col / right.Columns * dim;
                    double v = 0;
                    for (int k = 0; k < dim; k++) {
                        if ((k0 + k) < Lrow.Length) {
                            v += Lrow[k0 + k] * R[k][col2];
                        }
                    }
                    Prow[col] = v;
                }
            });

            for (int row = 0; row < left.Rows; row++) 
                prod.RowSpecList[row].CopyFrom(left.RowSpecList[row]);
            

            IList<IColumnSpec> pSpecList = prod.ColumnSpecList;
            IList<IColumnSpec> rSpecList = right.ColumnSpecList;
            for (int col = 0; col < prod.Columns; col++) {
                IColumnSpec cs = pSpecList[col];
                cs.CopyFrom(rSpecList[col % right.Columns]);
                int n = col / right.Columns;
                if (n > 0) {
                    cs.Id += "_" + n;
                }
            }
            return prod;
        }

        public INumberTable Transform(INumberTable inTable) {
            INumberTable rMatrix = matrixReal.SelectColumns(selectReal);
            INumberTable iMatrix = matrixImg.SelectColumns(selectImg);
            IList<IColumnSpec> cs = iMatrix.ColumnSpecList;
            for (int i = 0; i < cs.Count; i++) {
                cs[i].Id += "_i";
            }
            return MatrixProduct(inTable, rMatrix.AppendColumns(iMatrix));
        }

        public INumberTable Filter(INumberTable inTable, double lowFreq, double highFreq) {            
            INumberTable freqTable = Transform(inTable);

            //
            // Since the frequency vectors are half vector (the other half are mirred and not stored)
            // we need to duplicate the vector except the first and, in case n is even, the n/2 th element.
            // Notice that the Fourier matrixes have the same mirred structure.
            //
            for (int row = 0; row < freqTable.Rows; row++) {
                for (int col = 0; col < freqTable.Columns; col++) {
                    int c = col % tDim; // c is the index within the repeat interval.
                    if (c == 0) continue;
                    if ((tDim % 2 == 0) && (c == tDim / 2)) {
                        // In case tDim is even, the n/2-th element is at the centre of the symmetry
                        // and must not be duplciated.
                        continue;
                    }

                    freqTable.Matrix[row][col] *= 2;
                }
            }

            // Calculate the filter indexes to pick up bases vectors 
            // with frequency (stored as column group)falling between give range.
            int lFreq = (int)(lowFreq * (tDim / 2 + 1));
            int hFreq = (int)(highFreq * (tDim / 2 + 1));
            List<int> filterReal = new List<int>();
            List<int> filterImg = new List<int>();
            List<int> filterFreq = new List<int>();
            IList<IColumnSpec> mrSpecList = matrixReal.ColumnSpecList;
            foreach (int idx in selectReal) {
                int freq = mrSpecList[idx].Group;
                if ((lFreq <= freq) && (freq <= hFreq)) {
                    filterReal.Add(idx);
                    filterFreq.Add(idx);
                }
            }
            IList<IColumnSpec> miSpecList = matrixImg.ColumnSpecList;
            foreach (int idx in selectImg) {
                int freq = miSpecList[idx].Group;
                if ((lFreq <= freq) && (freq <= hFreq)) {
                    filterImg.Add(idx);
                    filterFreq.Add(tDim / 2 + idx);
                }
            }

            int repeats = freqTable.Columns / tDim;
            if (repeats > 1) {
                // the transformation has been repeated. We need to add the repeated coefficient here.                
                for (int r = 1; r < repeats; r++) {
                    for (int n = 0; n < tDim; n++) {
                        filterFreq.Add(r * tDim + filterFreq[n]);
                    }
                }
            }

            freqTable = freqTable.SelectColumns(filterFreq);
            INumberTable revMatrix = matrixReal.SelectRows(filterReal).Append(matrixImg.SelectRows(filterImg));
            INumberTable outTable = MatrixProduct(freqTable, revMatrix);

            IList<IColumnSpec> oSpecList = outTable.ColumnSpecList;
            IList<IColumnSpec> iSpecList = inTable.ColumnSpecList;
            for (int col = 0; col < outTable.Columns; col++) {
                if (col < inTable.Columns) {
                    oSpecList[col].CopyFrom(iSpecList[col]);
                }
            }
            return outTable;
        }
    }
}
