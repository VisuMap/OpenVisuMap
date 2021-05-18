using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using VisuMap.Plugin;
using VisuMap.Script;

namespace CustomImporter {
    /// <summary>
    /// This class enables VisuMap to open and import a SDF (structure data file).
    /// </summary>
    public class SdfFile : IFileImporter {
        public bool ImportFile(string fileName) {
            IVisuMap app = CustomImporter.App.ScriptApp;
            string fn = fileName.ToLower();
            if (!(fn.EndsWith(".sdf") || fn.EndsWith(".mol"))) {
                return false;  // Let other import handle it.
            }

            char[] sep = new char[] { ' ', '\t' };  // field separators.
            //
            // Frequently used atoms will have fixed body types.
            //
            Dictionary<string, short> typeNames = new Dictionary<string, short>(); // used to determine body types.
            short atomIdx = 0;
            typeNames["C"] = atomIdx++;
            typeNames["O"] = atomIdx++;
            typeNames["H"] = atomIdx++;
            typeNames["N"] = atomIdx++;
            typeNames["S"] = atomIdx++;
            typeNames["P"] = atomIdx++;
            typeNames["F"] = atomIdx++;
            typeNames["Na"] = atomIdx++;
            typeNames["Cl"] = atomIdx++;
            typeNames["Br"] = atomIdx++;

            try {
                using (StreamReader tr = new StreamReader(fileName)) {
                    string header = tr.ReadLine();
                    string cmmt = tr.ReadLine() + ";" + tr.ReadLine();
                    string[] info = tr.ReadLine().Split(sep, StringSplitOptions.RemoveEmptyEntries);
                    int atoms = int.Parse(info[0]);
                    int bonds = int.Parse(info[1]);

                    IFreeTable table = app.New.FreeTable();
                    table.AddColumn("X", true);
                    table.AddColumn("Y", true);
                    table.AddColumn("Z", true);
                    for (int i = 0; i < atoms; i++) {
                        table.AddColumn("A" + i, true);
                    }
                    table.AddRows("A", atoms);

                    bool is2D = true;

                    for (int row = 0; row < atoms; row++) {
                        string[] fs = tr.ReadLine().Split(sep, StringSplitOptions.RemoveEmptyEntries);
                        table.RowSpecList[row].Id =
                        table.ColumnSpecList[3 + row].Id = fs[3] + "." + (1 + row);
                        table.RowSpecList[row].Name =
                        table.ColumnSpecList[3 + row].Name = fs[3];
                        if (!typeNames.ContainsKey(fs[3])) {
                            typeNames[fs[3]] = atomIdx++;
                        }
                        table.RowSpecList[row].Type = table.ColumnSpecList[row + 3].Group = typeNames[fs[3]];

                        table.Matrix[row][0] = fs[0];
                        table.Matrix[row][1] = fs[1];
                        table.Matrix[row][2] = fs[2];

                        if (double.Parse(fs[2]) != 0) {
                            is2D = false;
                        }
                    }

                    for (int j = 0; j < bonds; j++) {
                        string[] fs = tr.ReadLine().Split(sep, StringSplitOptions.RemoveEmptyEntries);
                        int atom1 = int.Parse(fs[0]) - 1;
                        int atom2 = int.Parse(fs[1]) - 1;
                        table.Matrix[atom1][atom2 + 3] =
                        table.Matrix[atom2][atom1 + 3] = fs[2];
                    }

                    FileInfo fInfo = new FileInfo(fileName);
                    string shortName = fInfo.Name.Substring(0, fInfo.Name.LastIndexOf('.'));
                    string dsName = table.SaveAsDataset(shortName, header + "; " + cmmt);

                    if (dsName == null) {
                        MessageBox.Show("Cannot import the data: " + app.LastError);
                        return false;
                    } else {
                        app.Folder.OpenDataset(dsName);
                        double cX = app.Map.Width / 2;
                        double cY = app.Map.Height / 2;
                        app.Map.Depth = is2D ? 0 : Math.Max(app.Map.Width, app.Map.Height);
                        double cZ = app.Map.Depth / 2;

                        INumberTable numTable = app.Dataset.GetNumberTable();
                        double minX = double.MaxValue;
                        double minY = double.MaxValue;
                        double minZ = double.MaxValue;
                        double maxX = double.MinValue;
                        double maxY = double.MinValue;
                        double maxZ = double.MinValue;
                        for (int row = 0; row < numTable.Rows; row++) {
                            IList<double> R = numTable.Matrix[row];
                            minX = Math.Min(minX, R[0]);
                            minY = Math.Min(minY, R[1]);
                            minZ = Math.Min(minZ, R[2]);
                            maxX = Math.Max(maxX, R[0]);
                            maxY = Math.Max(maxY, R[1]);
                            maxZ = Math.Max(maxZ, R[2]);
                        }
                        double cx = (maxX + minX) / 2;
                        double cy = (maxY + minY) / 2;
                        double cz = (maxZ + minZ) / 2;
                        double mSize = Math.Max(maxX - minX, Math.Max(maxY - minY, maxZ - minZ));
                        double factor = Math.Min(app.Map.Width, app.Map.Height) * 0.85 / mSize;

                        for (int row = 0; row < numTable.Rows; row++) {
                            IBody body = app.Dataset.BodyList[row];
                            IList<double> R = numTable.Matrix[row];
                            body.ShowName = true;
                            body.X = (R[0] - cx) * factor + cX;
                            body.Y = (R[1] - cy) * factor + cY;
                            body.Z = (R[2] - cz) * factor + cZ;
                        }

                        // The following stmt will cause the window to refresh.
                        app.Map.GlyphType = "Colored Glyphs";
                        app.Map.MapType = is2D ? "Rectangle" : "Cube";
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Failed to load SDF/MOL file: " + ex.Message);
            }
            return true;
        }

        public string FileNameFilter {
            get { return "Structure data file(*.sdf;*.mol)|*.sdf;*.mol"; }
        }
    }
}
