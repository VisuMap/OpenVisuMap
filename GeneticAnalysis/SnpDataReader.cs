using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class SnpDataReader : IPluginObject  {
        public SnpDataReader() {

        }

        public int LoadSnpFile(string fileName) {
            string[] columns = new string[] {"start", "locType", "rsOrientToChrom" };
            List<string> rsId = new List<string>();
            List<List<string>> vTable = new List<List<string>>();

            using (XmlReader xr = XmlReader.Create(fileName)) {
                while (xr.Read()) {
                    if (xr.NodeType == XmlNodeType.Element) {
                        if (xr.LocalName == "SnpInfo") {
                            rsId.Add(xr.GetAttribute("rsId"));
                        } else if (xr.LocalName.Equals("SnpLoc")) {
                            if (vTable.Count == (rsId.Count - 1)) {
                                var v = new List<string>();
                                for (int i = 0; i < columns.Length; i++) {
                                    v.Add(xr.GetAttribute(columns[i]));
                                }                                
                                vTable.Add(v);
                            }
                        }
                    }
                }
            }

            var app = GeneticAnalysis.App.ScriptApp;
            var tb = app.New.FreeTable(vTable.Count, columns.Length);
            for (int col = 0; col < columns.Length; col++) tb.ColumnSpecList[col].Id = columns[col];
            for (int row = 0; row < vTable.Count; row++) {
                tb.RowSpecList[row].Id = rsId[row];
                for (int col = 0; col < columns.Length; col++) tb.Matrix[row][col] = vTable[row][col];
            }
            FileInfo fInfo = new FileInfo(fileName);
            string fName = fInfo.Name;
            int idx = fName.LastIndexOf('.');
            if (idx > 0) {
                fName = fName.Substring(0, idx);
            }
            tb.SaveAsDataset(fName, "");
            return vTable.Count;
        }

        public string Name {
            get { return "SnpDataReader"; }
            set { }
        }

    }
}
