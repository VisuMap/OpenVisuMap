using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.IO.Compression;

using VisuMap.Plugin;
using VisuMap.Script;

namespace CustomImporter {
    /// <summary>
    /// This class enables VisuMap to open and import a SDF (structure data file).
    /// </summary>
    public class CsvFilter : IFileImporter {
        string fileExtension;
        IVisuMap app;
        public CsvFilter() {
            app = CustomImporter.App.ScriptApp;
            fileExtension = app.GetProperty("CustomImporter.CsvFilter.Extension", "");
        }

        bool FilterDisabled {
            get { return string.IsNullOrEmpty(fileExtension); }
        }

        public bool ImportFile(string fileName) {            
            string fn = fileName.ToLower();
            fileExtension = app.GetProperty("CustomImporter.CsvFilter.Extension", "");

            if (FilterDisabled) return false;

            string[] extList = fileExtension.Split(new char[] {' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            bool accepted = false;
            foreach(string ex in extList) {
                if (fn.EndsWith(ex) || fn.EndsWith(ex + ".gz")) {
                    accepted = true;
                    break;
                }
            }
            if (!accepted) {
                return false;
            }

            string cmtPrefix = app.GetProperty("CustomImporter.CsvFilter.CommentPrefix", "//");
            string[] cmtPfList = cmtPrefix.Split(new char[] {' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int skipLines = int.Parse(app.GetProperty("CustomImporter.CsvFilter.SkipLines", "0"));

            FileInfo fi = new FileInfo(fileName);
            string dsName = fi.Name;
            dsName = dsName.EndsWith(".gz") ? dsName.Substring(0, dsName.Length - 3) : dsName;
            string tmpFile = Path.GetTempPath() + dsName;

            try {
                using (StreamReader tr0 = new StreamReader(fileName)) {
                    StreamReader tr = tr0;
                    if (fileName.EndsWith(".gz"))
                        tr = new StreamReader(new GZipStream(tr0.BaseStream, CompressionMode.Decompress));

                    using (StreamWriter sw = new StreamWriter(tmpFile)) {
                        while (true) {
                            string line = tr.ReadLine();
                            if (line == null) break;

                            if (skipLines > 0) {
                                skipLines--;
                                continue;
                            }

                            if (cmtPfList.Any(pf => line.StartsWith(pf)))
                                continue;

                            sw.WriteLine(line);
                        }
                    }
                }
                var csv = app.New.CsvImporterForm(tmpFile);
                csv.ShowDialog();
                File.Delete(tmpFile);
            } catch (Exception ex) {
                MessageBox.Show("CustomImporter: cannot apply CSV filter: " + ex.Message);
                return false;
            }
            return true;
        }

        public string FileNameFilter {
            get {
                fileExtension = app.GetProperty("CustomImporter.CsvFilter.Extension", "");
                return FilterDisabled ? "" : "Custom CSV file(*" + fileExtension + ")|*" + fileExtension; 
            }
        }
    }
}
