using System;
using System.Collections.Generic;
using System.Linq;
using VisuMap.Script;

namespace VisuMap.SingleCell {
    public class Utilities {        
        public bool GeneToCell { get; set; }

        public int Genes { get; set; }

        public int Cells { get; set; }

        public double MeanExpression { get; set; }

        public void ExpMin1(double[][] m) {
		    for (int row = 0; row < m.Length; row++)
		    for (int col = 0; col < m[0].Length; col++)
			    if (m[row][col] > 0)
				    m[row][col] = Math.Exp(m[row][col]) - 1.0;
	    }

	    public void LogPlus1(double[][] m) {
		    for (int row = 0; row < m.Length; row++)
		    for (int col = 0; col < m[0].Length; col++)
			    if (m[row][col] > 0)
				    m[row][col] = Math.Log(1 + m[row][col]);
	    }

	    double ExpThreshold() {
            double maxV = double.MinValue;
            MT.ForEach(NumTable.Matrix, R=> {
                double mxV = double.MinValue;
                for (int col = 0; col < NumTable.Columns; col++)
                    mxV = Math.Max(R[col], mxV);
                lock (this)
                    maxV = Math.Max(maxV, mxV);
            });
		    return (maxV < 100) ? maxV / 2.0 : Math.Sqrt(maxV);
	    }

        INumberTable NumTable = null;
        IList<IBody> OrgBodies = null;
        IList<IBody> BodyList = null;
        int N = 0;
        int[] row2bodyIdx;
        int[] col2bodyIdx;

        public void SetExpression(INumberTable numTable, IList<IBody> bodyList, IList<IBody> orgBodies, int N) {
            NumTable = numTable;
            OrgBodies = orgBodies;
            BodyList = bodyList;
            this.N = N;
            row2bodyIdx = Enumerable.Range(0, N).Where(i => !OrgBodies[i].Disabled).ToArray();
            col2bodyIdx = Enumerable.Range(N, OrgBodies.Count - N).Where(i => !OrgBodies[i].Disabled).ToArray();

            MeanExpression = ExpThreshold();
        }

        public List<string> Expression(List<string> selectedIds, double threshold) {
            const short maxExprIndex = 13; // number of expressed levels.
            const short indexShift = 38;
            double[][] M = (double[][])(NumTable.Matrix);
            int cellNr = NumTable.Rows;
            int geneNr = NumTable.Columns;
            IList<int> selectedCells = NumTable.IndexOfRows(selectedIds);
            IList<int> selectedGenes = NumTable.IndexOfColumns(selectedIds);
            List<string> expressedId = new List<string>();

            if (threshold == 0) { // mark the genes/cells which have higher expression than the average of selected genes/cells.
                if (GeneToCell) { // Finding expressed cells for selected genes.  Gene->Cell operation.
                    double[] expression = new double[cellNr]; // total expression of each cell.
                    MT.Loop(0, cellNr, row => {
                        foreach (int col in selectedGenes)
                            expression[row] += M[row][col];
                    });

                    double meanExp = expression.Sum() / cellNr;
                    double maxExp = expression.Max();
                    double step = (maxExp - meanExp) / maxExprIndex;

                    this.Genes = selectedGenes.Count;
                    this.Cells = 0;
                    for (int i = 0; i < cellNr; i++) { // for each cells.
                        double delta = expression[i] - meanExp;
                        int bIdx = row2bodyIdx[i];
                        if (delta > 0) {
                            short v = Math.Min(maxExprIndex, (short)(delta / step));
                            BodyList[bIdx].Type = (short)(indexShift + v);
                            this.Cells++;
                            expressedId.Add(BodyList[bIdx].Id);
                        } else
                            BodyList[bIdx].Type = OrgBodies[bIdx].Type;
                    }
                } else {
                    double[] expression = new double[geneNr]; // total expression of each gene.
                    MT.Loop(0, geneNr, col => {
                        foreach (int row in selectedCells)
                            expression[col] += M[row][col];
                    });
                    double meanExp = expression.Sum() / geneNr;
                    double maxExp = expression.Max();
                    double step = (maxExp - meanExp) / maxExprIndex;

                    // Finding expressed genes for selected cells. Cell->Gene operation.
                    this.Cells = selectedCells.Count;
                    this.Genes = 0;
                    for (int i = 0; i < geneNr; i++) {  // for each gene.
                        double delta = expression[i] - meanExp;
                        int bIdx = col2bodyIdx[i];
                        if (delta > 0) {
                            int v = Math.Min(maxExprIndex, (int)(delta / step));
                            BodyList[bIdx].Type = (short)(indexShift + v);
                            this.Genes++;
                            expressedId.Add(BodyList[bIdx].Id);
                        } else
                            BodyList[bIdx].Type = OrgBodies[bIdx].Type;
                    }
                }
            } else { // Mark all those cells/genes which have above global average expressions.
                if (GeneToCell) {                    
                    short[] expressed = new short[cellNr]; // count of expressed genes for each cell.
                    MT.Loop(0, cellNr, row => {
                        foreach (int col in selectedGenes) {
                            if (M[row][col] > threshold)
                                expressed[row] += 1;
                        }
                    });

                    this.Genes = selectedGenes.Count;
                    this.Cells = expressed.Count(v => v > 0);
                    for (int i = 0; i < cellNr; i++) {
                        short v = Math.Min(maxExprIndex, expressed[i]);
                        int bIdx = row2bodyIdx[i];
                        BodyList[bIdx].Type = (short) ( (v > 0) ? (indexShift + v) : OrgBodies[bIdx].Type );
                        if ( v>0 )
                            expressedId.Add(BodyList[bIdx].Id);
                    }                    
                } else {
                    short[] expressed = new short[geneNr];  // count of expressed cells for each gene.
                    MT.Loop(0, geneNr, col => {
                        foreach (int row in selectedCells) {
                            if (M[row][col] > threshold)
                                expressed[col] += 1;
                        }
                    });

                    this.Cells = selectedCells.Count;
                    this.Genes = expressed.Count(v => v > 0);
                    for (int i = 0; i < geneNr; i++) {
                        short v = Math.Min(maxExprIndex, expressed[i]);
                        int bIdx = col2bodyIdx[i];
                        BodyList[bIdx].Type = (short) ( (v > 0) ? (indexShift + v) : OrgBodies[bIdx].Type );
                        if (v > 0)
                            expressedId.Add(BodyList[bIdx].Id);
                    }
                }
            }
            return expressedId;
        }

        #region methods to set feature maps.
        double minExpression;
        double stepExpression;
        dynamic featureMap;

        public void SetExpressionTable(INumberTable numTable, IForm featureView) {
            if ((featureView is IMapSnapshot) || (featureView is IMdsCluster))
                featureMap = featureView;
            else {
                MsgBox.Alert("Only parnet window is not a map-snapshot or mds-cluster view.");
                return;
            }

            NumTable = numTable;
            OrgBodies = featureMap.BodyList;
            featureMap.GlyphSet = "Ordered Glyphs";
            featureMap.Redraw();

            minExpression = double.MaxValue;
            double maxExpression = double.MinValue;            
            MT.ForEach(NumTable.Matrix, R => {
                double minV = double.MaxValue;
                double maxV = double.MinValue;
                foreach (var v in R) {
                    minV = Math.Min(minV, v);
                    maxV = Math.Max(maxV, v);
                }
                lock (this) {
                    minExpression = Math.Min(minExpression, minV);
                    maxExpression = Math.Max(maxExpression, maxV);
                }
            });
            stepExpression = (maxExpression - minExpression) / 16;

            SingleCellPlugin.App.ItemsSelected += App_ItemsSelected;
            featureView.TheForm.FormClosing += (s, e) => {
                SingleCellPlugin.App.ItemsSelected -= App_ItemsSelected;
            };
        }

        private void App_ItemsSelected(object sender, Lib.ItemListEventArgs e) {
            List<string> idList = e.Items as List<string>;
            if( (featureMap == null) || (idList == null) || (idList.Count==0) )
                return;

            // Calculate the squared mean of columns into m[].
            int columns = NumTable.Columns;
            var rowList = NumTable.IndexOfRows(idList);
            double[] m = new double[columns];
            MT.ForEach(rowList, row => {
                var R = NumTable.Matrix[row];
                for (int col = 0; col < columns; col++) {
                    double v = R[col];
                    m[col] += v * v;
                }
            });
            for (int col = 0; col < columns; col++)
                m[col] = Math.Sqrt(m[col] / rowList.Count);

            for (int i = 0; i < m.Length; i++) {
                short t = (short)((m[i] - minExpression) / stepExpression);
                OrgBodies[i].Type = Math.Max((short)0, Math.Min((short)15, t));
            }
            featureMap.RedrawBodiesType();
        }
        #endregion
    }
}
