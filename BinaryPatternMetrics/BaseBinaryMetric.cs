/// <copyright from="2004" to="2014" company="VisuMap Technologies Inc.">
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
/// 
using System;
using System.Collections.Generic;
using System.Xml;
using System.Linq;
using System.Diagnostics;

using VisuMap.Plugin;
using VisuMap.Script;

namespace BinaryPatternMetrics {
    /// <summary>
    /// The base class for binary metrics.
    /// </summary>
    public abstract class BaseBinaryMetric : IMetric {
        /// <summary>
        /// The name of the distance metric.
        /// </summary>
        protected string name;


        bool[][] setTable;
        int[][] sparseTable;


        public void Initialize(IDataset dataset, XmlElement filterNode) {
            int sparseSetColumn = 0;
            var csList = dataset.ColumnSpecList;
            string flags = null;
            if (filterNode != null) {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("vm", "urn:VisuMap.Technologies:VisuMap");
                flags = filterNode.SelectSingleNode("vm:EnabledFlag", nsmgr).InnerText;
            }

            for (int col = 0; col < csList.Count; col++) {
                if (csList[col].Name == "SparseSet") {
                    sparseSetColumn = col;
                    sparseTable = new int[dataset.Rows][];
                    break;
                }
            }

            if (sparseTable != null) {
                for (int row = 0; row < dataset.Rows; row++) {
                    string[] fs = dataset.GetDataAt(row, sparseSetColumn).Split(',');
                    sparseTable[row] = new int[fs.Length];
                    int col2 = 0;
                    for (int col = 0; col < fs.Length; col++) {
                        if( (fs[col]!=null) && (fs[col] != "") ) {
                            sparseTable[row][col2++] = int.Parse(fs[col]);
                        }
                    }
                    if ( col2 < fs.Length ) {
                        Array.Resize(ref sparseTable[row], col2);
                    }
                }
            } else {
                int columns = 0;
                for (int col = 0; col < csList.Count; col++) {
                    if (csList[col].IsNumber && ((flags == null) || flags[col] == 'y'))
                        columns++;
                }
                
                setTable = new bool[dataset.Rows][];
                for (int row = 0; row < dataset.Rows; row++) {
                    setTable[row] = new bool[columns];
                    int idx = 0;
                    for (int col = 0; col < csList.Count; col++) {
                        if ( csList[col].IsNumber && ((flags == null) || flags[col] == 'y'))  {
                            setTable[row][idx] = (double.Parse(dataset.GetDataAt(row, col)) != 0);
                            idx++;
                        }
                    }
                }
            }
        }

        public bool[][] SetTable {
            get { return setTable; }
        }

        public int[][] SparseTable {
            get { return sparseTable; }
        }


        /// <summary>
        /// Constructor of this class.
        /// </summary>
        /// <param name="name">The name of the metric.</param>
        public BaseBinaryMetric(string name) {
            this.name = name;
        }

        /// <summary>
        /// Gets the name of the metric.
        /// </summary>
        public string Name { get { return name; } set { ;} }

        public double Distance(int idxA, int idxB) {
            int N11 = 0;  
            int N10 = 0;
            int N01 = 0;
            int N00 = 0;
            int Dimmension;

            if (sparseTable!=null) {
                int[] A = sparseTable[idxA];
                int[] B = sparseTable[idxB];
                int k = 0;
                for (int i = 0; i < A.Length; i++) {
                    int a = A[i];
                    for (; k < B.Length; k++) {
                        if (a>B[k]) {
                            N01++;
                        } else {
                            break;
                        }
                    }

                    if ( k >= B.Length ) {
                        // B does not has element A[i].
                        N10++;
                    } else {
                        if (a<B[k]) {
                            // no matching element to v in B.
                            N10++;
                        } else {  // fund a match:  a == B[k]
                            int cnt = 1;
                            while ((k + 1) < B.Length) {
                                k++;
                                if (a == B[k]) {
                                    cnt++;
                                } else {
                                    break;
                                }
                            }
                            N11 += cnt;
                            while ((i + 1) < A.Length) {
                                if (a == A[i + 1]) {
                                    i++;
                                    N11++;
                                } else {
                                    break;
                                }
                            }
                        } 
                    }
                }
                N01 += B.Length - k;
                Dimmension = N11 + N10 + N01;
            } else {
                bool[] A = setTable[idxA];
                bool[] B = setTable[idxB];
                Dimmension = A.Length;
                for (int col = 0; col < Dimmension; col++) {
                    if (A[col]) {
                        if (B[col]) {
                            N11++;
                        } else {
                            N10++;
                        }
                    } else {
                        if (B[col]) {
                            N01++;
                        }
                    }
                }
            }

            N00 = Dimmension - N11 - N10 - N01;
            
            return Distance(N00, N10, N01, N11, Dimmension);
        }

        public abstract double Distance(int N00, int N10, int N01, int N11, int Dimmension);

        public IFilterEditor FilterEditor {
            get { return null; }
        }

        public bool IsApplicable(IDataset dataset) {
            return true;
        }

        public bool IsApplicable(IDataset dataset, XmlElement filterNode) {
            if ( (filterNode.Name == "TableFilter") && filterNode.HasAttribute("Columns")) {
                if (dataset.Columns == int.Parse(filterNode.GetAttribute("Columns"))) {
                    return true;
                }
            }
            return false;
        }
    }
}
