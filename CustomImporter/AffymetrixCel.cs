using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using VisuMap.Plugin;
using VisuMap.Script;

namespace CustomImporter {
    /// <summary>
    /// This class enables VisuMap to import a Affymetrix CEL (version 3) file.
    /// </summary>
    public class AffyMetrixCel : IFileImporter {
        public bool ImportFile(string fileName) {
            IVisuMap app = CustomImporter.App.ScriptApp;
            string fn = fileName.ToLower();
            if ( !fn.EndsWith(".cel") ) {
                return false;  // Let other import handle it.
            }

            try {
                using (StreamReader tr = new StreamReader(fileName)) {
                    int columns = 0;
                    int rows = 0;
                    INumberTable nt = null;
                    string info = "";

                    if (tr.Peek() != 0x5B ) { // The magic char for CEL 3 format is '[' = 0x5B.
                        MessageBox.Show("Cannot import binary CEL format. Please use tool apt-cel-convert to convert the file to CEL 3 format.");
                        return true;
                        // return ImportFileCellBinary(tr);
                    }                    

                    while (true) {
                        string line = tr.ReadLine();

                        if (line == null) break;
                        if (line.StartsWith("Version")) {
                            if ( ! line.StartsWith("Version=3") ) return false;
                        } else if (line.StartsWith("Cols")) {
                            columns = int.Parse(line.Split('=')[1]);
                        } else if (line.StartsWith("Rows")) {
                            rows = int.Parse(line.Split('=')[1]);
                        } else if (line.StartsWith("[INTENSITY]")) {
                            if ((rows == 0) || (columns == 0)) {
                                return false;
                            }
                            tr.ReadLine();  tr.ReadLine();  // skip the two head lines.
                            nt = app.New.NumberTable(rows, columns);
                            while (true) {
                                line = tr.ReadLine();
                                if (line == null) break;
                                string[] fs = line.Split(new char[]{' ', '\t'}, StringSplitOptions.RemoveEmptyEntries );
                                if (fs.Length != 5) break;

                                int col = int.Parse(fs[0]);
                                int row = int.Parse(fs[1]);
                                nt.Matrix[row][col] = double.Parse(fs[2]);
                            }
                        } else if ( line.StartsWith("AlgorithmParameters") ) {
                            info = line.Split('=')[1];
                        }
                    }

                    if (nt == null) {
                        MessageBox.Show("Cannot import data from: " + fileName);
                        return false;
                    }

                    FileInfo fInfo = new FileInfo(fileName);
                    string shortName = fInfo.Name.Substring(0, fInfo.Name.LastIndexOf('.'));
                    string dsName = nt.SaveAsDataset(shortName, info);

                    if (dsName == null) {
                        MessageBox.Show("Cannot import the data: " + app.LastError);
                        return false;
                    }
                }
            } catch (Exception ex) {
                MessageBox.Show("Failed to load CEL file: " + ex.Message);
            }
            return true;
        }

        bool ImportFileCellBinary(StreamReader sr) {
            string line = sr.ReadLine();
            BinaryReader br = new BinaryReader(sr.BaseStream);
            var n = br.ReadBytes(4);
            n = br.ReadBytes(4);
            n = br.ReadBytes(4);
            return true;
        }

        public string FileNameFilter {
            get { return "Affymetrix CEL 3 (*.cel)|*.cel"; }
        }
    }
}
