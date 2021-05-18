/// <copyright from="2004" to="2011" company="VisuMap Technologies Inc.">
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
using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.MultivariateAnalysis {
    /// <summary>
    /// Data structure returned by PLS and CCA analysis.
    /// </summary>
    public class PlsResult {
        /// <summary>
        /// The projection of data table X to maximal covariation/correlation directions.
        /// </summary>
        public INumberTable ProjectionX;

        /// <summary>
        /// The projection of data table Y to maximal covariation/correlation directions
        /// </summary>
        public INumberTable ProjectionY;

        /// <summary>
        /// The maximal covariation/correlation directions for the table X.
        /// </summary>
        /// <remarks>
        /// <para>The directions vectors are row vectors of the matrix. In case
        /// of PLS analysis, the directions provide maximal covariance with the data table
        /// Y; In case of CCA analysis, the directions provide maximal correlation with the data table Y.
        /// </para>
        /// </remarks>
        public double[][] EigenVectorsX;


        /// <summary>
        /// The maximal covariation/correlation directions for the table Y.
        /// </summary>
        /// <remarks>
        /// <para>The directions vectors are row vectors of the matrix. In case
        /// of PLS analysis, the directions provide maximal covariance with the data table
        /// X; In case of CCA analysis, the directions provide maximal correlation with the data table X.
        /// </para>
        /// </remarks>
        public double[][] EigenVectorsY;

        /// <summary>
        /// The eigenvalues for the maximal covariance/correlation directions EigenVectorsX.
        /// </summary>
        /// <remarks>Larger eigenvalues means that the corresponding direction contributes larger covariance or correlation.</remarks>
        public double[] EigenValuesX;

        /// <summary>
        /// The eigenvalues for the maximal covariance/correlation directions EigenVectorsY.
        /// </summary>
        /// <remarks>Larger eigenvalues means that the corresponding direction contributes larger covariance or correlation.</remarks>
        public double[] EigenValuesY;
    }

    /// <summary>
    /// Data structure returned by LDA analysis.
    /// </summary>
    public class LdaResult {
        /// <summary>
        /// The projection of the input data table to the maximal separation directions.
        /// </summary>
        public INumberTable Projection;

        /// <summary>
        /// The direction vectors which provide maximal separation for input data.
        /// </summary>
        /// <remarks>The directions vectors are row vectors of the matrix.</remarks>
        public INumberTable EigenVectors;  // The eigenvectors are the row vectors.

        /// <summary>
        /// The eigenvalues for the maximal separation directions.
        /// </summary>
        /// <remarks>Larger eigenvalues means that larger separation in corresponding direction.</remarks>
        public double[] EigenValues;
    }

    /// <summary>
    /// Implmeents various multivariant data analysis.
    /// </summary>
    /// <remarks>This class will be instanciated and installed as an plugin object when this plugin is loaded. 
    /// Script program normally calls: 
    /// <code>vv.FindPluginObject("MultivariateAnalysis");</code> to obtain a reference to this object.</remarks>
    public class Mva : MarshalByRefObject, IPluginObject {
        /// <summary>
        /// Gets or sets the name of this object.
        /// </summary>
        public string Name {
            get { return "MultivariateAnalysis"; }
            set { }
        }

        /// <summary>
        /// Performs the Linear Discreminate Analysis (LDA) on a given number table.
        /// </summary>
        /// <param name="table">A numerical data table.</param>
        /// <returns>A LdaResult structure</returns>
        /// <remarks>This method find a set of directions, so that the projection of the row vector of the input data
        /// table to those directions will maximally separate rows with different types.</remarks>
        public LdaResult DoLDA(INumberTable table) {
            int rows = table.Rows;
            int columns = table.Columns;
            double[][] m = (double[][])table.Matrix;

            int classNumber = 0;  // the number of classes
            Dictionary<int, int> type2ClassIndex = new Dictionary<int, int>();
            int[] row2ClassIndex = new int[rows];
            for (int row = 0; row < rows; row++) {
                int type = table.RowSpecList[row].Type;
                if (!type2ClassIndex.ContainsKey(type)) {
                    type2ClassIndex[type] = classNumber++;
                }
                row2ClassIndex[row] = type2ClassIndex[type];
            }

            // Calculate the class mean vectors.
            double[][] classMean = NewMatrix(classNumber, columns);
            for (int cIdx = 0; cIdx < classNumber; cIdx++) {
                Array.Clear(classMean[cIdx], 0, columns);
            }
            double[] totalMean = new double[columns];
            Array.Clear(totalMean, 0, columns);
            int[] classSize = new int[classNumber];
            Array.Clear(classSize, 0, classSize.Length);

            for (int row = 0; row < rows; row++) {
                int cIdx = row2ClassIndex[row];
                classSize[cIdx]++;
                for (int col = 0; col < columns; col++) {
                    double v = m[row][col];
                    totalMean[col] += v;
                    classMean[cIdx][col] += v;
                }
            }

            for (int col = 0; col < columns; col++) {
                for (int cIdx = 0; cIdx < classNumber; cIdx++) {
                    classMean[cIdx][col] /= classSize[cIdx];
                }
                totalMean[col] /= rows;
            }

            // Zero mean the matrix m and the classMean vectors. 
            double[][] z = NewMatrix(rows, columns);
            for (int row = 0; row < rows; row++) {
                int cIdx = row2ClassIndex[row];
                for (int col = 0; col < columns; col++) {
                    z[row][col] = m[row][col] - classMean[cIdx][col];
                }
            }
            for (int cIdx = 0; cIdx < classNumber; cIdx++) {
                for (int col = 0; col < columns; col++) {
                    classMean[cIdx][col] -= totalMean[col];
                }
            }

            // Calculate the within- and between- covariance matrix.
            double[][] Sw = NewMatrix(columns, columns);
            double[][] Sb = NewMatrix(columns, columns);
            double[][] S = NewMatrix(columns, columns);

            for (int i = 0; i < columns; i++) {
                for (int j = 0; j < columns; j++) {
                    double v = 0.0;
                    for (int row = 0; row < rows; row++) {
                        v += z[row][i] * z[row][j];
                    }
                    Sw[i][j] = v;

                    v = 0.0;
                    for (int cIdx = 0; cIdx < classNumber; cIdx++) {
                        v += classMean[cIdx][i] * classMean[cIdx][j];
                    }
                    Sb[i][j] = v;
                }
            }

            // Calculate the inverse of Sw, and then Sb/Sw into S.
            IMathAdaptor math = MultivariateAnalysisPlugin.App.GetMathAdaptor();
            Sw = math.InvertMatrix(Sw);
            if (Sw == null) {
                S = Sb;
            } else {
                for (int i = 0; i < columns; i++) {
                    for (int j = 0; j < columns; j++) {
                        double v = 0.0;
                        for (int k = 0; k < columns; k++) {
                            v += Sw[i][k] * Sb[k][j];
                        }
                        S[i][j] = v;
                    }
                }
            }

            //
            // REVISIT: Make the matrix S symmetric. This matrix should be symmetrical
            // by itself, but is normally not completely symmetric due to caculation errors (particularily,
            // the matrix Sw Inverted is mostly not symmetrically.) Notice: the method math.EigenDecomposition()
            // follows completely different algorithm when the matrix is not symmetric
            //
            MakeSymmetric(S);

            double[][] eigenVectors;
            double[] eigenValues;
            if (!math.EigenDecomposition(S, out eigenVectors, out eigenValues)) {
                MultivariateAnalysisPlugin.App.ScriptApp.LastError = "Failed to calculate the eigenvectors.";
                return null;
            }

            /*
            MultivariateAnalysisPlugin.App.ScriptApp.New.NumberTable(S).ShowAsTable();
            INumberTable eT = MultivariateAnalysisPlugin.App.ScriptApp.New.NumberTable(eigenVectors);
            eT.AddRows(1);
            for (int i = 0; i < eigenValues.Length; i++) eT.Matrix[eT.Rows - 1][i] = eigenValues[i];
            eT.RowSpecList[eT.Rows - 1].Id = "EV";
            eT.ShowAsTable();
            */

            SortEigenVectors(eigenVectors, eigenValues);

            // project the original data m to the eigenvectors.
            LdaResult ret = new LdaResult();
            ret.EigenVectors = MultivariateAnalysisPlugin.App.ScriptApp.New.NumberTable(eigenVectors);
            int columnNumber = Math.Min(ret.EigenVectors.Columns, table.Columns);
            for (int col = 0; col < columnNumber; col++) {
                ret.EigenVectors.ColumnSpecList[col].CopyFrom(table.ColumnSpecList[col]);
            }
            ret.EigenValues = eigenValues;
            ret.Projection = EigenProjection(table, eigenVectors);
            return ret;
        }

        void MakeSymmetric(double[][] m) {
            for (int i = 0; i < m.Length; i++)
                for (int j = 0; j < i; j++)
                    m[i][j] = m[j][i];
        }

        /// <summary>
        /// Sorts the eigenvectors based on their eigenvalues.
        /// </summary>
        /// <param name="eigenVectors">Eigenvectors as matrix rows.</param>
        /// <param name="eigenValues">Eigenvalues of the corresponding the eigenvectors.</param>
        void SortEigenVectors(double[][] eigenVectors, double[] eigenValues) {
            Array.Sort(eigenValues, eigenVectors);
            Array.Reverse(eigenVectors);
            Array.Reverse(eigenValues);
        }

        /// <summary>
        /// Projects the rows of a data table to a set of eigenvectors.
        /// </summary>
        /// <param name="dataTable">The input data table.</param>
        /// <param name="eigenVectors">The eigenvectors as row vectors.</param>
        /// <returns>An INumberTable with row vectors as the projection.</returns>
        INumberTable EigenProjection(INumberTable dataTable, double[][] eigenVectors) {
            int rows = dataTable.Rows;
            int columns = dataTable.Columns;
            int outDim = eigenVectors.Length;
            INumberTable outTable = MultivariateAnalysisPlugin.App.ScriptApp.New.NumberTable(rows, outDim);
            double[][] outMatrix = (double[][])outTable.Matrix;
            double[][] m = (double[][])dataTable.Matrix;

            for (int row = 0; row < rows; row++) {
                for (int j = 0; j < outDim; j++) {
                    double v = 0.0;
                    for (int col = 0; col < columns; col++) {
                        v += m[row][col] * eigenVectors[j][col];
                    }
                    outMatrix[row][j] = v;
                }
            }
            for (int row = 0; row < rows; row++) {
                outTable.RowSpecList[row].CopyFrom(dataTable.RowSpecList[row]);
            }
            return outTable;
        }

        /// <summary>
        /// Creates a new matrix.
        /// </summary>
        /// <param name="rows">The number of rows of the matrix.</param>
        /// <param name="columns">The number of columns of the matrix.</param>
        /// <returns>A number matrix.</returns>
        static double[][] NewMatrix(int rows, int columns) {
            double[][] matrix = new double[rows][];
            for (int row = 0; row < rows; row++) matrix[row] = new double[columns];
            return matrix;
        }

        /// <summary>
        /// Calculates the matrix production of two matrixes.
        /// </summary>
        /// <param name="A">A matrix.</param>
        /// <param name="B">A matrix</param>
        /// <returns>The product of matrix A and B.</returns>
        static double[][] MatrixProduct(double[][] A, double[][] B) {
            int rows = A.Length;
            int columns = B[0].Length;
            int K = A[0].Length;
            double[][] P = NewMatrix(rows, columns);

            for (int row = 0; row < rows; row++) {
                for (int col = 0; col < columns; col++) {
                    double p = 0;
                    for (int k = 0; k < K; k++) {
                        p += A[row][k] * B[k][col];
                    }
                    P[row][col] = p;
                }
            }
            return P;
        }

        /// <summary>
        /// Calculates the covariance matrix between columns of two tables.
        /// </summary>
        /// <param name="matrixX">A number table.</param>
        /// <param name="matrixY">A number table.</param>
        /// <returns>The converiance matrix</returns>
        /// <remarks>The two number tables must have equal number of rows.</remarks>
        double[][] Covariance(double[][] matrixX, double[][] matrixY) {
            int columnsX = matrixX[0].Length;
            int columnsY = matrixY[0].Length;
            int rows = matrixX.Length;
            double[] mX = new double[columnsX];
            double[] mY = new double[columnsY];

            for (int col = 0; col < columnsX; col++) {
                double mean = 0;
                for (int row = 0; row < rows; row++) {
                    mean += matrixX[row][col];
                }
                mX[col] = mean / rows;
            }

            for (int col = 0; col < columnsY; col++) {
                double mean = 0;
                for (int row = 0; row < rows; row++) {
                    mean += matrixY[row][col];
                }
                mY[col] = mean /rows;
            }


            double[][] cov = NewMatrix(columnsX, columnsY);
            for (int row = 0; row < columnsX; row++) {
                for (int col = 0; col < columnsY; col++) {
                    double prod = 0.0;
                    for (int k = 0; k < rows; k++) {
                        prod += (matrixX[k][row] - mX[row]) * (matrixY[k][col] - mY[col]);
                    }
                    cov[row][col] = prod / (rows - 1);
                }
            }

            return cov;
        }

        /// <summary>
        /// Perform PLS analysis on columns of two number tables.
        /// </summary>
        /// <param name="tableX">A number table.</param>
        /// <param name="tableY">A number table.</param>
        /// <returns>A PlsResult structure.</returns>
        /// <remarks>This method finds directions for tableX and tableY; so that these two tables will
        /// have maximal covariance in those directions.</remarks>
        public PlsResult DoPLS(INumberTable tableX, INumberTable tableY) {
            double[][] Cxy = Covariance((double[][])tableX.Matrix, (double[][])tableY.Matrix);
            double[][] Cyx = Covariance((double[][])tableY.Matrix, (double[][])tableX.Matrix);
            double[][] CxyCyx = MatrixProduct(Cxy, Cyx);
            double[][] CyxCxy = MatrixProduct(Cyx, Cxy);

            IMathAdaptor math = MultivariateAnalysisPlugin.App.GetMathAdaptor();

            double[][] Wx, Wy;
            double[] Rx, Ry;

            // These two matrix might be slightly asymmetric due to calculation errors.
            MakeSymmetric(CxyCyx);
            MakeSymmetric(CyxCxy);

            math.EigenDecomposition(CxyCyx, out Wx, out Rx);
            math.EigenDecomposition(CyxCxy, out Wy, out Ry);

            SortEigenVectors(Wx, Rx);
            SortEigenVectors(Wy, Ry);

            PlsResult ret = new PlsResult();
            ret.ProjectionX = EigenProjection(tableX, Wx);
            ret.ProjectionY = EigenProjection(tableY, Wy);
            ret.EigenVectorsX = Wx;
            ret.EigenVectorsY = Wy;
            ret.EigenValuesX = Rx;
            ret.EigenValuesY = Ry;
            return ret;
        }

        /// <summary>
        /// Perform CCA analysis on columns of two number tables.
        /// </summary>
        /// <param name="tableX">A number table.</param>
        /// <param name="tableY">A number table.</param>
        /// <returns>A PlsResult structure.</returns>
        /// <remarks>This method finds directions for tableX and tableY; so that these two tables will
        /// have maximal correlation in those directions.</remarks>
        public PlsResult DoCCA(INumberTable tableX, INumberTable tableY) {
            double[][] X = (double[][])tableX.Matrix;
            double[][] Y = (double[][])tableY.Matrix;
            double[][] Cxy = Covariance(X, Y);
            double[][] Cyx = Covariance(Y, X);

            IMathAdaptor math = MultivariateAnalysisPlugin.App.GetMathAdaptor();

            double[][] rCxx = math.InvertMatrix(Covariance(X, X));
            double[][] rCyy = math.InvertMatrix(Covariance(Y, Y));

            MakeSymmetric(rCxx);
            MakeSymmetric(rCyy);

            double[][] A = MatrixProduct(rCxx, Cxy);
            double[][] B = MatrixProduct(rCyy, Cyx);
            double[][] Cx = MatrixProduct(A, B);
            double[][] Cy = MatrixProduct(B, A);

            double[][] Wx, Wy;
            double[] Rx, Ry;

            math.EigenDecomposition(Cx, out Wx, out Rx);
            math.EigenDecomposition(Cy, out Wy, out Ry);

            //
            // Rx and Ry should be the equal upto permutation and dimension.
            // Wy can be calculated from Wx by Ry = sqrt(1/Rx) * B * Rx
            //

            /*
            double x0 = Math.Sqrt(Rx[0]);
            double x1 = Math.Sqrt(Rx[1]);
            double x2 = Math.Sqrt(Rx[2]);

            double y0 = Math.Sqrt(Ry[0]);
            double y1 = Math.Sqrt(Ry[1]);
            double y2 = Math.Sqrt(Ry[2]);
            
            0.7165   0.4906   0.2668
            */

            SortEigenVectors(Wx, Rx);
            SortEigenVectors(Wy, Ry);

            //ValidateMatrix(Wx);
            //ValidateMatrix(Wy);

            PlsResult ret = new PlsResult();
            ret.ProjectionX = EigenProjection(tableX, Wx);
            ret.ProjectionY = EigenProjection(tableY, Wy);
            ret.EigenVectorsX = Wx;
            ret.EigenVectorsY = Wy;
            ret.EigenValuesX = Rx;
            ret.EigenValuesY = Ry;

            return ret;
        }

        void ValidateMatrix(double[][] m) {
            INumberTable nt = MultivariateAnalysisPlugin.App.ScriptApp.New.NumberTable(m);
            INumberTable ntT = nt.Transpose2();
            INumberTable I = nt.Multiply(ntT);
            I.ShowAsTable();
        }
    }
}
