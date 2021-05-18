using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

using VisuMap.Script;

namespace VisuMap.SingleCell {
    using IMetric = VisuMap.Plugin.IMetric;
    using IDataset = VisuMap.Script.IDataset;
    using IFilterEditor = VisuMap.Plugin.IFilterEditor;
    using FastPca = VisuMap.LinearAlgebra.FastPca;

    public class DualPca : IMetric {
        IDataset dataset;
        float[][] P, dtP;
        double stepSize;  // dsitance between two adjucent points on the PCA axis.
        int[] toIdx = null;        // map a dataset wide index into the range of [0, enabled_bodies-1].        
        const int pcaSamples = 24; // number of points sampled from the main PCA axis.
        const int pcaMax = 10;     // maximal number of PC components used for approximations.

        public DualPca() {
            ;
        }

        public void Initialize(IDataset dataset, XmlElement filterNode) {
            if (!ValidateDataset(dataset))
                return;
            this.dataset = dataset;
            dtP = null;
        }

        public double Distance(int i, int j) {
            if (i == j) {
                if (i == 0) {
                    if (dtP == null)
                        PreCalculate();
                    else if ((dataset != null) && (dataset.BodyList.Count(b => !b.Disabled) != (dtP.Length + pcaSamples)))
                        PreCalculate();
                } else if (i == 1)
                    dtP = null;
                return 0;
            }
            i = toIdx[i];
            j = toIdx[j];
            int N = P.Length;
            if ((i < N) && (j < N)) {
                return (j < i) ? dtP[i][j] : dtP[j][i];
            } else if ((i >= N) && (j >= N)) {
                return stepSize * Math.Abs(i-j);
            } else
                return (i < N) ? P[i][j - N] : P[j][i - N];
        }

        void PreCalculate() {
            if (dataset == null)
                return;

            // Extract the relevant data table.
            var bs = dataset.BodyList;
            INumberTable nt = dataset.GetNumberTableView();
            toIdx = Enumerable.Range(0, bs.Count).Where(i => !bs[i].Disabled).ToArray();    // Indexes of enabled bodies.
            int N = nt.Rows - pcaSamples;
            int[] enabledRows = toIdx.Where(i => i < N).ToArray();

            if (enabledRows.Length == 0) 
                throw new TException("No data available!");
            P = new float[enabledRows.Length][];
            MT.Loop(0, P.Length, row => {
                float[] R = P[row] = new float[nt.Columns];
                double[] dsR = nt.Matrix[enabledRows[row]] as double[];
                for (int col = 0; col < nt.Columns; col++)
                    R[col] = (float)dsR[col];
            });

            // Reverse toIdx;
            int[] rIdx = Enumerable.Repeat(-1, bs.Count).ToArray();
            for (int i = 0; i < toIdx.Length; i++) rIdx[toIdx[i]] = i;
            toIdx = rIdx;

            using (var gpu = new VisuMap.DxShader.GpuDevice())
                dtP = DualAffinity.DotProduct(gpu, P, false);

            float[] singValues = new float[pcaMax];
            float[][] PC = FastPca.DoFastReduction(P, pcaMax, singValues, true); 
            P = VisuMap.MathUtil.NewMatrix<float>(PC.Length, pcaSamples);  // P now links data points with the injected points on the main PCA axis.
            float span = 4.0f * singValues[0];
            stepSize = span / (pcaSamples - 1);
            float x0 = - 0.5f * span;
            MT.ForEach(PC, (R, row) => {
                double yy = R.Skip(1).Sum(v => v * v);
                for (int col = 0; col < pcaSamples; col++) {
                    double x = R[0] - (x0 + col * stepSize);
                    P[row][col] = (float)Math.Sqrt(x * x + yy);
                }
            });
        }

        static bool ValidateDataset(IDataset ds) {
            int i0 = ds.Rows - pcaSamples;
            if ((i0 > 0) && ds.BodyList.Skip(i0).All(b => b.IsDummy)) {
                for (int i = i0; i < ds.BodyList.Count; i++)
                    ds.BodyList[i].Disabled = false;
                return true;
            } else {
                var ret = MsgBox.YesNoTimed("Current dataset is invalid for dual metric. Do you want to adjust the dataset for dual metric?", "Dual Metric", 5);
                if (ret == DialogResult.Yes) {
                    i0 = ds.Rows;
                    var rg = new Random();
                    for (int i = 0; i < pcaSamples; i++) {
                        IBody b = ds.AddRow("." + i, null, (short)51, null);
                        b.IsDummy = true;
                        b.X = rg.Next((int)ds.CurrentMap.Width);
                        b.Y = rg.Next((int)ds.CurrentMap.Height);
                        if ((i == 0) || (i == (pcaSamples - 1))) 
                            b.Highlighted = true;
                        if (i == 0)
                            b.ShowId = true;
                    }
                    ds.CurrentMap.GlyphType = DualAffinity.GlyphSets;                    
                    ds.CurrentMap.RedrawAll();
                    SingleCellPlugin.App.ScriptApp.Folder.DataChanged = true;
                    Root.Data.Map.Metric = null;
                    return true;
                } else {
                    return false;
                }
            }
        }

        #region Other default members.
        public string Name { get => "Dual.PCA"; set { } }

        public IFilterEditor FilterEditor => null;

        public bool IsApplicable(IDataset dataset) {
            return true;
        }

        public bool IsApplicable(IDataset dataset, XmlElement filterNode) {
            return false;
        }
        #endregion
    }
}
