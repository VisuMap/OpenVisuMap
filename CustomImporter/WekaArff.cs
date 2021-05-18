using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text;

using VisuMap.Plugin;
using VisuMap.Script;


namespace CustomImporter {
    /// <summary>
    /// This class enables VisuMap to open and import a Weka ARFF file.
    /// </summary>
    public class WekaArff : IFileImporter {
        public bool ImportFile(string fileName) {
            IVisuMap app = CustomImporter.App.ScriptApp;

            // This importer object only recognizes files with name ending
            // with .arff
            if (!(fileName.ToLower().EndsWith(".arff"))) {
                return false;
            }

            StreamReader tr = new StreamReader(fileName);
            List<string> columnName = new List<string>();
            List<string> columnDesc = new List<string>();
            List<bool> isNumber = new List<bool>();

            int rows = 0;
            while (!tr.EndOfStream) {
                string line = tr.ReadLine();
                if (line.StartsWith("@")) {
                    string[] fs = line.Split(' ');
                    if (fs[0].ToLower() == "@attribute") {
                        string[] fields = line.Split(new char[] { ' ', '\t', '\"'}, StringSplitOptions.RemoveEmptyEntries);
                        columnName.Add(fields[1]);
                        if (fields.Length >= 4) {
                            StringBuilder sb = new StringBuilder();
                            for (int i = 2; i < (fields.Length - 1); i++) {
                                if (i != 2) {
                                    sb.Append(" ");
                                }
                                sb.Append(fields[i]);
                            }
                            columnDesc.Add(sb.ToString());
                        } else {
                            columnDesc.Add("");
                        }
                        string typeStr = fields[fields.Length - 1].ToLower();
                        if ( (typeStr =="numeric") || (typeStr=="real") ) {
                            isNumber.Add(true);
                        } else {
                            isNumber.Add(false);
                        }
                        continue;
                    }
                } else if (line.StartsWith("%") || string.IsNullOrEmpty(line)) {
                    ;
                } else {
                    rows++;
                }
            }

            //
            // Load the file into a table and save the table as 
            // a dataset into the current folder.
            //
            IFreeTable table = app.New.FreeTable();
            Dictionary<string, int> uniqId = new Dictionary<string, int>();
            for (int col = 0; col < columnName.Count; col++) {
                string id = columnName[col];
                if ( uniqId.ContainsKey(id) ) {
                    uniqId[id]++;
                    id += "_" + uniqId[id];
                } else {
                    uniqId[id] = 0;
                }
                table.AddColumn(id, columnDesc[col], isNumber[col]);
            }

            table.AddRows("r", rows);
            int row = 0;
            tr.BaseStream.Seek(0, SeekOrigin.Begin);
            while (!tr.EndOfStream) {
                string line = tr.ReadLine();
                if ( string.IsNullOrEmpty(line) || line.StartsWith("@") || line.StartsWith("%") ) {
                    continue;
                }
                string[] fs = line.Split(',', '\t');
                IList<string> r = table.Matrix[row];
                for (int col = 0; col < table.Columns; col++) {
                    if (fs[col].Equals("?")) {
                        r[col] = app.MissingValueReplacement.ToString();
                    } else {
                        r[col] = fs[col];
                    }
                }
                row++;
            }

            tr.Close();
            tr.Dispose();

            // Set row type to indicate the class attributes.
            int classCol = table.IndexOfColumn("class");
            if ( (classCol >= 0) && (table.ColumnSpecList[classCol].DataType == 'e') ) {
                Dictionary<string, short> uniType = new Dictionary<string, short>();
                IList<IRowSpec> rsList = table.RowSpecList;
                for (int rw = 0; rw < table.Rows; rw++) {
                    string sClass = table.Matrix[rw][classCol];
                    if (! uniType.ContainsKey(sClass) ) {
                        uniType.Add(sClass, (short)uniType.Count);
                    }
                    rsList[rw].Type = uniType[sClass];
                }
            }


            FileInfo fInfo= new FileInfo(fileName);
            string shortName = fInfo.Name.Substring(0, fInfo.Name.LastIndexOf('.'));
            string dsName = table.SaveAsDataset(shortName, "Dataset imported from ARFF file.");


            if (dsName == null) {
                MessageBox.Show("Cannot import the data: " + app.LastError);
            } else {
                app.Folder.OpenDataset(dsName);

                if (app.GetProperty("CustomImporter.Arff.DoPca", "1").Equals("1")) {
                    IPcaView pca = app.New.PcaView();
                    pca.Show();
                    pca.ResetView();
                    pca.CaptureMap();
                    app.Map.Redraw();
                    pca.Close();
                }
            }

            return true;
        }

        public string FileNameFilter {
            get { return "Weka Attribute-Relation File Format(*.arff)|*.arff"; }
        }

    }
}
