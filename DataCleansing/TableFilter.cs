using System;
using System.Collections.Generic;
using System.Text;
using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.DataCleansing {
    public class TableFilter :  IPluginObject {

        public string Name {
            get { return "TableFilter"; }
            set { }
        }

        IList<int> ToIndexList(INumberTable nTable, bool attributeMode, IList<string> itemList) {
            if (attributeMode) {
                return nTable.IndexOfColumns(itemList);
            } else {
                return nTable.IndexOfRows(itemList);
            }
        }

        public void Logarithmic(INumberTable nTable, bool attributeMode, IList<string> itemList) {
            double[][] m = (double[][]) nTable.Matrix;
            foreach (int i in ToIndexList(nTable, attributeMode, itemList)) {
                if (attributeMode) {
                    for (int row = 0; row < nTable.Rows; row++) 
                        m[row][i] = Math.Log(1 + Math.Abs(m[row][i]));
                } else {
                    for (int col = 0; col < nTable.Columns; col++)
                        m[i][col] = Math.Log(1 + Math.Max(0, m[i][col]));
                }
            }
        }

        public void Logicle(INumberTable nTable, bool attributeMode, IList<string> itemList) {
            double[][] m = (double[][])nTable.Matrix;

            double T = 262144;
            double W = 1.0;
            double M = 4.5;
            string[] settings = DataCleansing.App.ScriptApp.GetProperty(
                "DataCleansing.Logicle.Settings", "262144; 1.0; 4.5").Split(';');
            if (settings.Length == 3) {
                double.TryParse(settings[0], out T);
                double.TryParse(settings[1], out W);
                double.TryParse(settings[2], out M);
            }

            FastLogicle fastLogicle = new FastLogicle(T, W, M);
            double maxVal = fastLogicle.MaxValue;
            double minVal = fastLogicle.MinValue;

            foreach (int i in ToIndexList(nTable, attributeMode, itemList)) {
                if (attributeMode) {
                    for (int row = 0; row < nTable.Rows; row++) {
                        m[row][i] = M * fastLogicle.scale(Math.Min(maxVal, Math.Max(minVal, m[row][i])));
                    }
                } else {
                    for (int col = 0; col < nTable.Columns; col++) {
                        m[i][col] = M * fastLogicle.scale(Math.Min(maxVal, Math.Max(minVal, m[i][col])));
                    }
                }
            }
        }

        public void InverseLogicle(INumberTable nTable, bool attributeMode, IList<string> itemList) {
            double[][] m = (double[][])nTable.Matrix;

            double T = 262144;
            double W = 1.0;
            double M = 4.5;
            string[] settings = DataCleansing.App.ScriptApp.GetProperty(
                "DataCleansing.Logicle.Settings", "262144; 1.0; 4.5").Split(';');
            if (settings.Length == 3) {
                double.TryParse(settings[0], out T);
                double.TryParse(settings[1], out W);
                double.TryParse(settings[2], out M);
            }
            FastLogicle fastLogicle = new FastLogicle(T, W, M);

            foreach (int i in ToIndexList(nTable, attributeMode, itemList)) {
                if (attributeMode) {
                    for (int row = 0; row < nTable.Rows; row++) {
                        m[row][i] = fastLogicle.inverse(Math.Min(0.999999, Math.Max(0, m[row][i]/M)));
                    }
                } else {
                    for (int col = 0; col < nTable.Columns; col++) {
                        m[i][col] = fastLogicle.inverse(Math.Min(1, Math.Max(0, m[i][col]/M)));
                    }
                }
            }
        }

        public void Scale(INumberTable nTable, bool attributeMode, IList<string> itemList,  double factor) {
            double[][] m = (double[][])nTable.Matrix;
            IList<int> idxList = ToIndexList(nTable, attributeMode, itemList);
            if (factor == 0) {
                double maxValue = double.MinValue;
                foreach (int i in idxList) {
                    if (attributeMode) {
                        for (int row = 0; row < nTable.Rows; row++) maxValue = Math.Max(maxValue, m[row][i]);
                    } else {
                        for (int col = 0; col < nTable.Columns; col++) maxValue = Math.Max(maxValue, m[i][col]);
                    }
                }

                if (maxValue != 0) {
                    factor = Math.Abs(nTable.MaximumValue()) / Math.Abs(maxValue);
                }
            }

            foreach (int i in idxList) {
                if (attributeMode) {
                    for (int row = 0; row < nTable.Rows; row++) m[row][i] *= factor;
                } else {
                    for (int col = 0; col < nTable.Columns; col++) m[i][col] *= factor;
                }
            }
        }

        public void Normalize(INumberTable nTable, bool attributeMode, IList<string> itemList) {
            double[][] m = (double[][])nTable.Matrix;
            IList<int> idxList = ToIndexList(nTable, attributeMode, itemList);
            if (attributeMode) {
                foreach (int col in idxList) {
                    double vMax = double.MinValue;
                    double vMin = double.MaxValue;
                    for (int row = 0; row < nTable.Rows; row++) {
                        vMax = Math.Max(vMax, m[row][col]);
                        vMin = Math.Min(vMin, m[row][col]);
                    }
                    double vRange = vMax - vMin;
                    for (int row = 0; row < nTable.Rows; row++) {
                        m[row][col] = (vRange == 0) ? 0 : (m[row][col] - vMin) / vRange;
                    }
                }
            } else {
                foreach (int row in idxList) {
                    double vMax = double.MinValue;
                    double vMin = double.MaxValue;
                    for (int col = 0; col < nTable.Columns; col++) {
                        vMax = Math.Max(vMax, m[row][col]);
                        vMin = Math.Min(vMin, m[row][col]);
                    }
                    double vRange = vMax - vMin;
                    for (int col = 0; col < nTable.Columns; col++) {
                        m[row][col] = (vRange == 0) ? 0 : (m[row][col] - vMin) / vRange;
                    }
                }
            }
        }

        public void Delete(INumberTable nTable, bool attributeMode, IList<string> itemList) {
            IList<int> idxList = ToIndexList(nTable, attributeMode, itemList);
            if (attributeMode) {
                nTable.RemoveColumns(idxList);
            } else {
                nTable.RemoveRows(idxList);
            }
        }

        public void Duplicate(INumberTable nTable, bool attributeMode, IList<string> itemList) {
            if (attributeMode) {
                nTable.AppendColumns(nTable.SelectColumnsById(itemList));
            } else {
                nTable.Append(nTable.SelectRowsById(itemList));
            }
        }
    }
}
