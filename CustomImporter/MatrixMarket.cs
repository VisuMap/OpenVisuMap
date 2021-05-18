using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.IO.Compression;

using VisuMap.Plugin;
using VisuMap.Script;

namespace CustomImporter {
    public class MatrixMarket : IFileImporter {
        public bool ImportFile(string fileName) {
            IVisuMap app = CustomImporter.App.ScriptApp;
            string fname = fileName.ToLower();
            if (!(fname.EndsWith(".mtx") || fname.EndsWith(".mtx.gz"))) {
                return false;
            }

            double[][] matrix = null;
            int rows = 0;
            int columns = 0;
            int nozeros = 0;
            string topLine = null;

            using (StreamReader tr0 = new StreamReader(fileName)) {
                StreamReader tr = tr0;
                if (fname.EndsWith(".gz"))
                    tr = new StreamReader(new GZipStream(tr0.BaseStream, CompressionMode.Decompress));
                while (!tr.EndOfStream) {
                    string line = tr.ReadLine();
                    if (line.StartsWith("%")) {
                        topLine = line;
                        continue;
                    }

                    string[] fs = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (matrix == null) {
                        if (fs.Length != 3) {
                            MessageBox.Show("Mtx file misses top information line.");
                            return false;
                        }
                        rows = int.Parse(fs[0]);
                        columns = int.Parse(fs[1]);
                        nozeros = int.Parse(fs[2]);
                        matrix = new double[rows][];
                        for (int k = 0; k < rows; k++)
                            matrix[k] = new double[columns];
                        continue;
                    }
                    int row = int.Parse(fs[0])-1;
                    int col = int.Parse(fs[1])-1;
                    double v = 1.0;
                    if (fs.Length == 3)
                        v = double.Parse(fs[2]);
                    matrix[row][col] = v;
                }
            }

            if (topLine != null) {
                if (topLine.Trim().EndsWith("symmetric") && (rows==columns)) {
                    for (int row = 0; row < rows; row++)
                        for (int col = row + 1; col < columns; col++)
                            matrix[row][col] = matrix[col][row];
                }
            }

            var nt = app.New.NumberTable(matrix);

            FileInfo fInfo = new FileInfo(fileName);
            string shortName = fInfo.Name.Substring(0, fInfo.Name.LastIndexOf('.'));
            if (shortName.EndsWith(".mtx"))
                shortName = shortName.Substring(0, shortName.Length - 4);
            string dsName = nt.SaveAsDataset(shortName, topLine);
            if (dsName == null) {
                MessageBox.Show("Cannot import the data: " + app.LastError);
                return false;
            }
            app.Folder.OpenDataset(dsName);

            return true;
        }

        public string FileNameFilter {
            get { return "MatrixMarket Format (*.mtx,*.mtx.gz)|*.mtx;*.mtx.gz"; }
        }
    }
}
