using System;
using System.Linq;
using System.Collections;
using System.Windows.Forms;

using VisuMap.Script;

namespace VisuMap.DataModeling {
    public partial class VariableListView : Form {
        LiveModel liveModel;
        bool operationMode = false;
        INumberTable numTable = null;
        IHeatMap heatMap = null;

        public VariableListView(LiveModel md) {
            InitializeComponent();
            liveModel = md;
        }

        public void SetOperationMode() {
            operationMode = true;
            var app = DataModeling.App.ScriptApp;
            numTable = (app.SelectedItems.Count == 0) ? app.GetNumberTable() : app.GetSelectedNumberTable();
            this.Text = "Operation Node List";
            this.listView1.Columns[0].Text = "Name";
            this.listView1.Columns[1].Text = "Type";
        }

        public void AddRow(string name, string shape) {
            if (operationMode) {
                ListViewItem row = new ListViewItem(name);
                row.SubItems.Add(shape);
                this.listView1.Items.Add(row);
            } else {
                ListViewItem row = new ListViewItem(name);
                row.SubItems.Add(shape.Trim(new char[] { ',', '(', ')' }));
                this.listView1.Items.Add(row);
            }
        }


        void ShowNodeActivity() {
            if (listView1.SelectedIndices.Count <= 0) return;

            string opName = listView1.SelectedItems[0].SubItems[0].Text;
            string opType = listView1.SelectedItems[0].SubItems[1].Text;

            if (opType == "ConstStr") {
                string strValue = liveModel.ReadString(opName);
                MessageBox.Show(strValue, "String Constant");
                return;
            }

            var app = DataModeling.App.ScriptApp;
            var values = liveModel.EvalVariable((double[][]) numTable.Matrix, opName, true);
            var act = app.New.NumberTable(values);
            if (numTable.Rows == act.Rows) {
                for (int row = 0; row < numTable.Rows; row++)
                    act.RowSpecList[row].CopyFrom(numTable.RowSpecList[row]);
            }

            if ( (heatMap == null) || heatMap.TheForm.IsDisposed ) {
                heatMap = app.New.HeatMap(act);
                heatMap.Show();
            } else {
                heatMap.GetNumberTable().Copy(act);
                heatMap.Redraw();
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e) {
            try {
                if (operationMode) {
                    ShowNodeActivity();
                    return;
                }

                string varName = this.listView1.SelectedItems[0].SubItems[0].Text;
                var values = liveModel.ReadVariable(varName);
                var vmNew = DataModeling.App.ScriptApp.New;
                if (values != null) {
                    if (values[0].Length == 1) {
                        var sv = vmNew.SpectrumView(values.Select(row => row[0]).ToArray());
                        sv.ShowRange = true;
                        sv.Horizontal = false;
                        sv.Title = "Variable: " + varName;
                        if (values.Length == 1)
                            sv.Title += ": " + values[0][0].ToString("g3");
                        int n = 0;
                        foreach (var v in sv.ItemList) {
                            v.Id = "R" + n;
                            n++;
                        }
                        sv.Show();
                    } else {
                        var nt = vmNew.NumberTable(values);
                        int n = 0;
                        foreach (var rs in nt.RowSpecList) {
                            rs.Id = "R" + n;
                            n++;
                        }
                        var vw = vmNew.SpectrumBand(nt);
                        vw.Title = "Variable: " + varName;
                        vw.ShowRange = true;
                        vw.Show();
                    }
                }
            } catch(Exception ex) {
                MessageBox.Show("Operation failed: " + ex.Message);
            }
        }

        protected override void OnShown(EventArgs e) {
            this.listView1.Columns[0].Width = -1;
            base.OnShown(e);
        }

        class ListViewItemComparer : IComparer {
            private int col;
            public ListViewItemComparer() {
                col = 0;
            }
            public ListViewItemComparer(int column) {
                col = column;
            }
            public int Compare(object x, object y) {
                int returnVal = -1;
                returnVal = String.Compare(((ListViewItem)x).SubItems[col].Text,
                ((ListViewItem)y).SubItems[col].Text);
                return returnVal;
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e) {
            listView1.ListViewItemSorter = new ListViewItemComparer(e.Column);
            listView1.Sort();
        }
    }
}
