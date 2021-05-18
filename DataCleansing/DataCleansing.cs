using System;
using System.Windows.Forms;
using System.Collections.Generic;

using VisuMap.Plugin;
using VisuMap.Script;
using VisuMap.Lib;

namespace VisuMap.DataCleansing {
    [PluginMain]
    public class DataCleansing : IPlugin {
        public static IApplication App;
        string pluginHome;
        /// <summary>
        /// Ininitalize the plugin module.
        /// </summary>
        /// <param name="app">The object representing the running VisuMap application.</param>
        public void Initialize(IApplication app) {
            if (app.ScriptApp.ApplicationBuild < 884) {
                MessageBox.Show("Data cleasing plugin requirs VisuMap 3.5.884 or higher!\nSome service might fail.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            App = app;
            pluginHome = App.ScriptApp.ApplicationData + "\\plugins\\Data Cleansing\\";

            
            ToolStripMenuItem miCleansing = new ToolStripMenuItem("Data Cleansing");

            miCleansing.DropDownItems.Add("Mark NaN Values", null, MarkNaNValues);
            miCleansing.DropDownItems.Add("Mark Integer Columns", null, MarkIntColumns);
            miCleansing.DropDownItems.Add("Remove Constant Columns", null, RemoveConstantColumns);
            miCleansing.DropDownItems.Add("Remove All-Zeros Rows&&Columns", null, RemoveAllZeros);
            miCleansing.DropDownItems.Add("Remove corrupt rows", null, RemoveCorruptRows);
            miCleansing.DropDownItems.Add("Remove corrupt columns", null, RemoveCorruptColumns);
            ToolStripMenuItem miEditing = new ToolStripMenuItem("Direct Editing", null, OpenDirectEditing);
            miEditing.ToolTipText = "Open a text editor for the clipboard data.";
            miCleansing.DropDownItems.Add(miEditing);

            app.GetPluginMenu().DropDownItems.Add(miCleansing);

            App.InstallPluginObject(new TableFilter());
        }

        void MarkNaNValues(object sender, EventArgs e) {
            App.ScriptApp.New.ScriptEditor(pluginHome + "MarkNaNValues.js").Show();
        }

        void MarkIntColumns(object sender, EventArgs e) {
            App.ScriptApp.New.ScriptEditor(pluginHome + "MarkIntegerColumns.js").Show();
        }

        void RemoveAllZeros(object sender, EventArgs e) {
            IDataset ds = App.ScriptApp.Dataset;
            List<string> zRows = new List<string>();
            List<string> zColumns = new List<string>();

            var csList = ds.ColumnSpecList;
            var rsList = ds.BodyList;
            for (int row = 0; row < ds.Rows; row++) {
                bool allZero = true;
                for (int col = 0; col < ds.Columns; col++) {
                    if ( ! csList[col].IsNumber ) {
                        allZero = false;
                        break;
                    } else {
                        double v = (double) ds.GetValueAt(row, col);
                        if ( v != 0 ) {
                            allZero = false;
                            break;
                        }
                    }
                }
                if ( allZero ) zRows.Add(rsList[row].Id);
            }

            for (int col = 0; col < ds.Columns; col++) {
                if (!csList[col].IsNumber) continue;
                bool allZero = true;
                for (int row = 0; row < ds.Rows; row++) {
                    double v = (double)ds.GetValueAt(row, col);
                    if (v != 0) {
                        allZero = false;
                        break;
                    }
                }
                if (allZero) zColumns.Add(csList[col].Id);
            }

            if ((zRows.Count > 0) || (zColumns.Count > 0)) {
                var ret = MessageBox.Show("Found " + zRows.Count + " rows and " + zColumns.Count + " columns.\n"
                    + "Remove all of them?", "Remove All-Zeros Rows and Columns", MessageBoxButtons.YesNo);
                if (ret == DialogResult.Yes) {
                    ds.RemoveRows(zRows);
                    ds.RemoveColumns(zColumns);
                    ds.CommitChanges();
                }
            } else {
                MessageBox.Show("No all-zero rows or columns have been found!");                
            }
        }

        const double LargeValue = 1e15;

        void RemoveCorruptRows(object sender, EventArgs e) {
            IDataset ds = App.ScriptApp.Dataset;
            List<string> rowIdList = new List<string>();
            IList<IColumnSpec> csList = ds.ColumnSpecList;  // notice that ds.ColumnSpecList constructs the list on-fly.
            var bodyList = ds.BodyList;

            for (int row = 0; row < ds.Rows; row++) {
                for (int col = 0; col < ds.Columns; col++) {
                    if (csList[col].IsEnumerate)
                        continue;                
                    double v = (double)ds.GetValueAt(row, col);
                    if (double.IsNaN(v) || double.IsInfinity(v) || Math.Abs(v) > LargeValue) {
                        rowIdList.Add(bodyList[row].Id);
                        break;
                    }
                }
            }

            if (rowIdList.Count > 0) {
                var ret = MessageBox.Show("Found " + rowIdList.Count + " rows with corrupt values!\nRemove them?",
                    "Remove corrupt data", MessageBoxButtons.YesNo);
                if (ret == DialogResult.No)
                    return;
                int removed = ds.RemoveRows(rowIdList);
                ds.CommitChanges();
                MessageBox.Show("Removed " + removed + " rows!\nNew table dimension: "
                    + ds.Rows + " rows; " + ds.Columns + " columns.");
            } else {
                MessageBox.Show("No corrupt rows have been found.");
            }

        }

        void RemoveCorruptColumns(object sender, EventArgs e) {
            IDataset ds = App.ScriptApp.Dataset;
            List<string> columnIdList = new List<string>(ds.Columns);
            IList<IColumnSpec> csList = ds.ColumnSpecList;  // notice that ds.ColumnSpecList constructs the list on-fly.
            
            for (int col = 0; col < ds.Columns; col++) {
                if (csList[col].IsEnumerate)
                    continue;
                for (int row = 0; row < ds.Rows; row++) {
                    double v = (double) ds.GetValueAt(row, col);
                    if (double.IsNaN(v) || double.IsInfinity(v) || Math.Abs(v) > LargeValue) {
                        columnIdList.Add(csList[col].Id);
                        break;
                    }                    
                }
            }

            if (columnIdList.Count > 0) {
                var ret = MessageBox.Show("Found " + columnIdList.Count + " columns with corrupt values!\nRemove them?", 
                    "Remove corrupt data", MessageBoxButtons.YesNo);
                if (ret == DialogResult.No)
                    return;
                int removed = ds.RemoveColumns(columnIdList);
                ds.CommitChanges();
                MessageBox.Show("Removed " + removed + " columns!\nNew table dimension: "
                    + ds.Rows + " rows; " + ds.Columns + " columns.");
            } else {
                MessageBox.Show("No corrupt columns have been found.");
            }
        }

        void RemoveConstantColumns(object sender, EventArgs e) {
            IDataset ds = App.ScriptApp.Dataset;
            List<string> columnIdList = new List<string>(ds.Columns);
            Dictionary<object, bool> vTable = new Dictionary<object, bool>();
            IList<IColumnSpec> csList = ds.ColumnSpecList;  // notice that ds.ColumnSpecList constructs the list on-fly.

            for (int col = 0; col < ds.Columns; col++) {
                vTable.Clear();
                for (int row = 0; row < ds.Rows; row++) {
                    object v = ds.GetValueAt(row, col);
                    if (!vTable.ContainsKey(v)) {
                        vTable.Add(v, true);
                    }

                    if (vTable.Count > 1) {
                        break;
                    }
                }

                if ( vTable.Count == 1 ) {
                    columnIdList.Add(csList[col].Id);
                }
            }

            int removed =0;
            removed = ds.RemoveColumns(columnIdList);
            if (removed > 0) {
                ds.CommitChanges();
                MessageBox.Show("Removed " + removed + " columns!\nNew table dimension: "
                    + ds.Rows + " rows; " + ds.Columns + " columns.");
            } else {
                MessageBox.Show("No column have been removed");
            }
        }

        void OpenDirectEditing(object sender, EventArgs e) {
            (new DirectEditing()).Show();
        }


        /// <summary>
        /// Dispose the plugin object.
        /// </summary>
        public void Dispose() { }

        /// <summary>
        /// Gets the name of the plugin module.
        /// </summary>
        public string Name { get { return "DataCleansing"; } }
    }
}
