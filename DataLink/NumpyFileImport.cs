using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using VisuMap.Plugin;
using VisuMap.Script;   

namespace VisuMap.DataLink {
    // A simple importer for float32 numpy table or sequences.
    public enum NumpyDtype {
        DT_Float32, DT_Int32, DT_Float64, DT_Unknown
    }

    public class NumpyFileImport : IFileImporter {
        public bool ImportFile(string fileName) {
            using (FileStream f = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(f)) {
                byte[] magic = br.ReadBytes(6);
                if (magic[0] != 0x93)
                    return false;
                byte[] version = br.ReadBytes(2);
                int headLen = 0;
                if (version[0] == 1) {
                    headLen = br.ReadInt16();
                } else {
                    headLen = br.ReadInt32();
                }
                byte[] bHead = br.ReadBytes(headLen);
                string sHead = Encoding.UTF8.GetString(bHead);
                sHead = sHead.Trim(new char[] { '{', ',', '}' });
                string[] fs = sHead.Split(new char[] {' ', ',', ':', '\'', ')', '('}, StringSplitOptions.RemoveEmptyEntries);
                NumpyDtype dtype = NumpyDtype.DT_Unknown;

                bool isFortranOrder = false;
                List<int> dims = new List<int>();
                for(int i=0; i<fs.Length; i+=2) {
                    switch (fs[i]) {
                        case "descr":
                            switch(fs[i + 1]) {
                                case "<f4":
                                    dtype = NumpyDtype.DT_Float32;
                                    break;
                                case "<f8":
                                    dtype = NumpyDtype.DT_Float64;
                                    break;
                                case "<i4":
                                    dtype = NumpyDtype.DT_Int32;
                                    break;
                                default:
                                    dtype = NumpyDtype.DT_Unknown;
                                    break;
                            }
                            break;
                        case "fortran_order":
                            isFortranOrder = (fs[i + 1] == "True");
                            break;
                        case "shape":
                            dims.Add(int.Parse(fs[i + 1]));
                            for (int j = i + 2; j < fs.Length; j++)
                                if (char.IsDigit(fs[j][0]))
                                    dims.Add(int.Parse(fs[j]));
                            break;
                    }
                }
                if (dtype == NumpyDtype.DT_Unknown) {
                    MessageBox.Show("Only float32, float64 and int32 data types are supported!");
                    return false;
                }
                if( dims.Count == 0) {
                    MessageBox.Show("No shape defined!");
                    return false;
                }
                var appNew = DataLink.App.ScriptApp.New;
                if (dims.Count == 2) {
                    INumberTable nt = null;
                    if (isFortranOrder) {
                        nt = appNew.NumberTable(dims[1], dims[0]);
                        for (int row = 0; row < dims[1]; row++)
                            ReadRow(nt.Matrix[row] as double[], br, dtype);
                        nt.Transpose();
                    } else {
                        nt = appNew.NumberTable(dims[0], dims[1]);
                        for (int row = 0; row < dims[0]; row++)
                            ReadRow(nt.Matrix[row] as double[], br, dtype);
                    }
                    if (DataLink.App.ScriptApp.Dataset != null) {
                        var bs = DataLink.App.ScriptApp.Dataset.BodyListEnabled();
                        if (nt.Rows != bs.Count) {
                            bs = DataLink.App.ScriptApp.Map.SelectedBodies;
                            if (nt.Rows != bs.Count)
                                bs = null;
                        }
                        if (bs != null)
                            for (int i = 0; i < nt.Rows; i++)
                                nt.RowSpecList[i].CopyFromBody(bs[i]);
                    }
                    var hm = nt.ShowHeatMap();
                    hm.Title = "HeatMap: " + fileName;
                } else {
                    int len = 1;
                    foreach (int d in dims) len *= d;
                    float[] values = new float[len];
                    for (int i = 0; i < len; i++)
                        values[i] = (float) NextValue(br, dtype);
                    var bb = appNew.BigBarView(values).Show();                    
                    bb.Title = "BigBar View: " + fileName;
                }
            }
            return true;
        }

        double NextValue(BinaryReader br, NumpyDtype dtype) {
            return dtype.Equals(NumpyDtype.DT_Float32) ? br.ReadSingle() 
                : dtype.Equals(NumpyDtype.DT_Float64) ? br.ReadDouble() 
                : br.ReadInt32();
        }

        void ReadRow(double[] R, BinaryReader br, NumpyDtype dtype ) {
            switch(dtype) {
                case NumpyDtype.DT_Float32:                
                    float[] buf = new float[R.Length];
                    Buffer.BlockCopy(br.ReadBytes(R.Length * 4), 0, buf, 0, R.Length * 4);
                    for (int i = 0; i < R.Length; i++) R[i] = buf[i];
                    break;
                case NumpyDtype.DT_Float64:
                    Buffer.BlockCopy(br.ReadBytes(R.Length * 8), 0, R, 0, R.Length * 8);
                    break;
                default:
                    int[] buff = new int[R.Length];
                    Buffer.BlockCopy(br.ReadBytes(R.Length * 4), 0, buff, 0, R.Length * 4);
                    for (int i = 0; i < R.Length; i++) R[i] = buff[i];
                    break;
            }
        }

        public string FileNameFilter {
            get => "Numpy file (*.npy)|*.npy";
        }
    }
}
