using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Linq;

using VisuMap.Plugin;
using VisuMap.Script;

namespace CustomImporter {
    class FcsCytometry : IFileImporter {
        bool showResult;
        int saveMetaInfo;
        bool logTransform;
        bool autoCompensation;
        string glyphName;
        FcsInfo fcsInfo;

        public class ColumnInfo {
            public string Name;
            public string ShortName;
            public int Bits;
            public int Bytes;
            public long Range;
            public bool IsLinear;
            public bool LogTrans;  // do logarithmic translation for display.
            public uint RangeMask;
        }

        public class FcsInfo {
            public int Rows;
            public int Columns;
            public ColumnInfo[] ColumnInfo;
            public enum DataTypes {
                Integer, Float, Double, None
            };
            public DataTypes DataType;
            public bool BigEndian;
            public string version;

            IFreeTable textTable;

            string Field(string rowId) {
                int idx = textTable.IndexOfRow(rowId);
                return (idx >= 0) ? textTable.Matrix[textTable.IndexOfRow(rowId)][0] : "";
            }

            public FcsInfo(IFreeTable textTable, char[] header) {
                this.textTable = textTable;
                version = new string(header, 0, 6);
                Rows = int.Parse(Field("$TOT"));
                Columns = int.Parse(Field("$PAR"));
                ColumnInfo = new ColumnInfo[Columns];
                string dt = Field("$DATATYPE");
                switch (dt) {
                    case "I" : DataType = DataTypes.Integer; break;
                    case "F" : DataType = DataTypes.Float; break;
                    case "D" : DataType = DataTypes.Double; break;
                    default: DataType = DataTypes.None; break;
                }
                string byteOrder = Field("$BYTEORD");
                if ( (byteOrder == "1,2,3,4") || (byteOrder == "1,2") ) {
                    BigEndian = true;
                } else if ((byteOrder == "4,3,2,1") || (byteOrder == "2,1")) {
                    BigEndian = false;
                } else {
                    throw new Exception("Unsupported Endiannes: " + byteOrder); 
                }

                for (int col = 1; col <= Columns; col++) {
                    ColumnInfo ci = new ColumnInfo();
                    ci.Name = Field("$P" + col + "S");
                    ci.ShortName = Field("$P" + col + "N");
                    ci.Bits = int.Parse(Field("$P" + col + "B"));
                    ci.Range = (long) double.Parse(Field("$P" + col + "R"));
                    ci.RangeMask = Int2Mask(ci.Range);
                    string dsp = Field("P" + col + "DISPLAY");
                    ci.LogTrans = dsp == "LOG";
                    ci.IsLinear = Field("$P" + col + "E") == "0,0";
                    ci.Bytes = (ci.Bits + 7) / 8;

                    ColumnInfo[col-1] = ci;
                }
            }

            public uint Int2Mask(long range) {
                uint mask = 1;
                long v = range >> 1;

                while (v > 0) {
                    v >>= 1;
                    mask <<= 1;
                }
                if (mask < range) {
                    // range is not a pow of 2.
                    mask <<= 1;
                }
                return mask - 1;
            }
        }

        public bool ImportFile(string fileName) {
            VisuMap.Script.IVisuMap app = CustomImporter.App.ScriptApp;
            showResult = app.GetProperty("CustomImporter.FCS.ShowResult", "0").Equals("1");
            saveMetaInfo = int.Parse(app.GetProperty("CustomImporter.FCS.SaveMetaInfo", "1"));
            glyphName = app.GetProperty("CustomImporter.FCS.GlyphName", "36 Clusters");
            logTransform = app.GetProperty("CustomImporter.FCS.LogTransform", "1").Equals("1");
            autoCompensation = app.GetProperty("CustomImporter.FCS.AutoCompensation", "1").Equals("1");

            try {
                return ImportFile0(fileName);
            } catch (Exception ex) {
                MessageBox.Show("Cannot import file " + fileName + ": " + ex.Message);
                return false;
            }
        }

        // Shift a section of bits to the byte boundary.
        void ShiftBits(byte[] buf, int bitOffset, int bits) {
            if (buf.Length > 2) {
                throw new Exception("Larger partial bytes not supported.");
            }

            // Copy the buf to a ushort for shifting. 
            ushort v = 0;
            if (buf.Length == 2) {
                v = (ushort)((buf[1] << 8) | buf[0]);   // x86 are all BigEndian.
            } else {
                v = buf[0];
            }

            v <<= 16 - bitOffset - bits;
            v >>= 16 - bits;

            if (buf.Length == 1) {
                buf[0] = (byte)(v & 0xFF);
            } else {
                if (fcsInfo.BigEndian) {
                    buf[1] = (byte)(v >> 8);
                    buf[0] = (byte)(v & 0xFF);
                } else {
                    buf[0] = (byte)(v >> 8);
                    buf[1] = (byte)(v & 0xFF);
                }
            }
        }

        byte[] ReadBits(BinaryReader br, int bits, ref int bitOffset) {
            if ( ((bits % 8) == 0) && (bitOffset==0) ) {
                return br.ReadBytes(bits / 8);
            }

            int bytes = (bits + 7) / 8;
            byte[] buf = br.ReadBytes(bytes);

            ShiftBits(buf, bitOffset, bits);
            
            // Calculate the next bit offset.
            bitOffset = 8 - (bytes * 8 - bits - bitOffset);
            if ( bitOffset == 8 ) {
                bitOffset = 0;
            }

            if ( bitOffset != 0 ) {
                br.BaseStream.Seek(-1, SeekOrigin.Current);  // put one byte back.
            }

            return buf;
        }


        public bool ImportFile0(string fileName) {
            IVisuMap app = CustomImporter.App.ScriptApp;
            if (!fileName.ToLower().EndsWith(".fcs")) {
                return false;  // Let other import handle it.
            }

            INumberTable dataTable = null;
            IFreeTable textTable = null;
            bool compensated = false;
            bool logScaled = false;

            using (StreamReader sr = new StreamReader(fileName)) {
                // 
                // Read the text segment.
                //
                char[] header = new char[42];
                sr.ReadBlock(header, 0, header.Length);
                int textBegin = int.Parse(new string(header, 10, 8));
                int textEnd = int.Parse( new string(header, 18, 8));
                int beginData = int.Parse(new string(header, 26, 8));
                int endData = int.Parse(new string(header, 34, 8));

                char[] line = new char[textEnd + 4];
                sr.ReadBlock(line, header.Length, line.Length - header.Length);
                string textSeg = new string(line, textBegin + 1, textEnd - textBegin);
                char delimiter = line[textBegin];

                textTable = app.New.FreeTable();
                textTable.AddColumn("Value", false);

                string[] textFields = textSeg.Split(delimiter);
                textTable.AddRows("P", textFields.Length / 2);
                IList<IRowSpec> rowSpecList = textTable.RowSpecList;
                for (int row = 0; row < textTable.Rows; row++) {
                    rowSpecList[row].Id = textFields[2 * row];
                    textTable.Matrix[row][0] = textFields[2 * row + 1];
                }

                //
                // Read in the data segement
                //
                fcsInfo = new FcsInfo(textTable, header);
                if( (beginData==0) && (textTable.IndexOfRow("$BEGINDATA") > 0) ) {
                    beginData = int.Parse(textTable.Matrix[textTable.IndexOfRow("$BEGINDATA")][0]);
                }
                if( (endData==0) && (textTable.IndexOfRow("$ENDDATA") > 0) ){
                    endData = int.Parse(textTable.Matrix[textTable.IndexOfRow("$ENDDATA")][0]);
                }
                dataTable = app.New.NumberTable(fcsInfo.Rows, fcsInfo.Columns);

                using (BinaryReader br = new BinaryReader(sr.BaseStream)) {
                    br.BaseStream.Seek(beginData, SeekOrigin.Begin);

                    Byte[] buf4 = new byte[4];
                    Byte[] buf8 = new byte[8];

                    int bitOffset = 0;
                    for (int row = 0; row < fcsInfo.Rows; row++) {
                        for (int col = 0; col < fcsInfo.Columns; col++) {
                            ColumnInfo ci = fcsInfo.ColumnInfo[col];
                            Byte[] data = ReadBits(br, ci.Bits, ref bitOffset);
                            if (data.Length < ci.Bytes) {
                                row = fcsInfo.Rows; // enforce premature loop-end.
                                break;
                            }
                            Byte[] buf = (fcsInfo.DataType == FcsInfo.DataTypes.Double) ? buf8 : buf4;
                            Array.Clear(buf, 0, buf.Length);
                            Array.Copy(data, 0, buf, 0, ci.Bytes);
                            if (! fcsInfo.BigEndian) {
                                // Intel CPU expects BigEndian format.
                                Array.Reverse(buf, 0, ci.Bytes);
                            }                           

                            switch( fcsInfo.DataType ) {
                                case FcsInfo.DataTypes.Integer:
                                    dataTable.Matrix[row][col] = (BitConverter.ToUInt32(buf, 0) & ci.RangeMask);
                                    break;

                                case FcsInfo.DataTypes.Float:
                                    dataTable.Matrix[row][col] = BitConverter.ToSingle(buf, 0);
                                    break;

                                case FcsInfo.DataTypes.Double:
                                    dataTable.Matrix[row][col] = BitConverter.ToDouble(buf, 0);
                                    break;
                            }
                        }
                    }
                }
            }

            // Post processing
            for (int col = 0; col < fcsInfo.Columns; col++) {
                IColumnSpec cs = dataTable.ColumnSpecList[col];
                ColumnInfo cInfo = fcsInfo.ColumnInfo[col];
                cs.Id = cInfo.ShortName;
                //cs.Name = cInfo.Name + ( cInfo.IsLinear ? ".Lin" : ".Log");
                cs.Name = cInfo.Name;

                if ((cs.Id == "TIME") || (cs.Id == "TIME1")) {
                    int timeStepIdx = textTable.IndexOfRow("$TIMESTEP");
                    if (timeStepIdx >= 0) {
                        double timeStep = double.Parse(textTable.Matrix[timeStepIdx][0]);
                        for (int row = 0; row < fcsInfo.Rows; row++) {
                            dataTable.Matrix[row][col] *= timeStep;
                        }
                    }
                }
            }

            IList<IRowSpec> rSpecList = dataTable.RowSpecList;
            for (int row = 0; row < fcsInfo.Rows; row++) {
                rSpecList[row].Id = row.ToString();
            }


            FileInfo fInfo = new FileInfo(fileName);
            string shortName = fInfo.Name.Substring(0, fInfo.Name.LastIndexOf('.'));

            if (saveMetaInfo >= 1) {
                SaveParameterTable(textTable, shortName);
                if (saveMetaInfo >= 2) {
                    textTable.SaveAsDataset(shortName + " (Text Seg)", "Text segement of data table " + shortName);
                }
            }

            if (autoCompensation) {
                try {
                    string sMatrix = null;
                    for (int i = 0; i < textTable.Rows; i++) {
                        string id = textTable.RowSpecList[i].Id.ToLower();
                        if (id.StartsWith("$")) id = id.Substring(1);
                        if ((id == "spill") || (id == "spillover")) {
                            sMatrix = textTable.Matrix[i][0];
                            break;
                        }
                    }

                    if (sMatrix == null) throw new Exception("");  // silently ignore compensation as no compensation matrix available.

                    string[] fs = sMatrix.Split(',');
                    int dimension = int.Parse(fs[0]);

                    if (fs.Length != (dimension * dimension + dimension + 1)) throw new Exception("Invalid spill over matrix.");
                    
                    INumberTable cMatrix = app.New.NumberTable(dimension, dimension);
                    List<string> parList = fs.Skip(1).Take(dimension).ToList();

                    int idx;
                    if (parList.Count(id => int.TryParse(id, out idx)) == parList.Count) {
                        // The columns are specified by a list of indexes. We convert them here to ids
                        for (int i = 0; i < parList.Count; i++) {
                            parList[i] = dataTable.ColumnSpecList[int.Parse(parList[i]) - 1].Id;   // index starts with 1 !
                        }
                    }

                    for (int col = 0; col < cMatrix.Columns; col++) {
                        cMatrix.ColumnSpecList[col].Id = parList[col];
                    }
                    int fsIdx = dimension + 1;
                    for (int row = 0; row < cMatrix.Rows; row++) {
                        for (int col = 0; col < cMatrix.Columns; col++) {
                            cMatrix.Matrix[row][col] = double.Parse(fs[fsIdx++]);
                        }
                    }

                    var cData = dataTable.SelectColumnsById(parList);
                    if (cData.Columns != parList.Count) {
                        if (dataTable.ColumnSpecList.Count(cl => cl.Id.Equals("<" + parList[0] + ">")) == 1) {
                            // siliently ignore aready compensated data.
                            throw new Exception("");
                        } else {
                            throw new Exception("Invalid spill over matrix: unknown names.");
                        }
                    }

                    var math = CustomImporter.App.GetMathAdaptor();
                    var m = math.InvertMatrix((double[][])cMatrix.Matrix);
                    for (int row = 0; row < cMatrix.Rows; row++) {
                        cMatrix.Matrix[row] = m[row];
                    }

                    cData = cData.Multiply(cMatrix);  // perform the comensation with the inverse matrix of the spill over matrix.
                    cData.CopyValuesTo(dataTable);
                    compensated = true;
                } catch (Exception ex) {
                    if (ex.Message != "") {
                        MessageBox.Show("Cannot perform compensation: " + ex.Message);
                    }
                }
            }

            if (logTransform) {
                double[][] m = (double[][])dataTable.Matrix;

                double T = 262144;
                double W = 1.0;
                double M = 4.5;
                string[] settings = app.GetProperty(
                    "CustomImporter.Logicle.Settings", "262144; 1.0; 4.5").Split(';');
                if (settings.Length == 3) {
                    double.TryParse(settings[0], out T);
                    double.TryParse(settings[1], out W);
                    double.TryParse(settings[2], out M);
                }
                var fastLogicle = new VisuMap.DataCleansing.FastLogicle(T, W, M);
                double maxVal = fastLogicle.MaxValue;
                double minVal = fastLogicle.MinValue;

                for (int col = 0; col < fcsInfo.Columns; col++) {
                    if ( fcsInfo.ColumnInfo[col].LogTrans ) {
                        for (int row = 0; row < fcsInfo.Rows; row++) {
                            m[row][col] = M*fastLogicle.scale(Math.Min(maxVal, Math.Max(minVal, m[row][col])));
                        }
                        logScaled = true;
                    }
                }
            }

            string msg = "Dataset imported from " + fileName + ". Version: " + fcsInfo.version;
            if (compensated) msg += ", compensated";
            if (logScaled) msg += ", log-scaled";
            string dsName = dataTable.SaveAsDataset(shortName, msg);            
            app.Folder.OpenDataset(dsName);
            INumberTable nTable = app.GetNumberTable();


            List<IColumnSpec> csList = nTable.ColumnSpecList as List<IColumnSpec>;
            int fsc = csList.FindIndex(cs => cs.Id == "FSC-A");
            int ssc = csList.FindIndex(cs => cs.Id == "SSC-A");
            if ((fsc >= 0) && (ssc >= 0)) {
                var xy = app.New.XyPlot(nTable);
                xy.Show();
                xy.XAxisIndex = fsc;
                xy.YAxisIndex = ssc;
                xy.AutoScaling = true;
                xy.Redraw();
                xy.CaptureMap();
                xy.Close();
                app.Map.Name = "FSC/SSC";
            } else {
                // We just do create a simple PCA view.
                IPcaView pca = nTable.ShowPcaView();
                pca.ResetView();
                pca.CaptureMap();
                app.Map.Redraw();
                pca.Close();
                app.Map.Name = "PCA-All";
            }

            app.Map.GlyphType = glyphName;
            app.Map.Redraw();

            if (showResult) {
                app.New.SpectrumBand(nTable).Show();                
            }

            fcsInfo = null;
            return true;
        }

        public void SaveParameterTable(IFreeTable textTable, string shortName) {
            int columns = 0;
            List<string> columnIdList = new List<string>();
            for (int row = 0; row < textTable.Rows; row++) {
                var id = textTable.RowSpecList[row].Id;
                if (id.StartsWith("P2") || id.StartsWith("$P2")) {
                    char cAttribute = id[id.StartsWith("P2") ? 2 : 3];
                    if (char.IsDigit(cAttribute)) {
                        continue;   // this field is $P20X, $P21X, etc.
                    }
                    columns++;
                    columnIdList.Add(id.Substring(id.StartsWith("P2") ? 2 : 3));
                }
            }

            IFreeTable pTable = CustomImporter.App.ScriptApp.New.FreeTable(fcsInfo.Columns, columns);
            int startIdx = (textTable.RowSpecList as List<IRowSpec>).FindIndex(
                rs => rs.Id.StartsWith("$P1") || rs.Id.StartsWith("P1"));

            for (int row = startIdx; row < textTable.Rows; row++) {
                var id = textTable.RowSpecList[row].Id;
                if (id.StartsWith("P") || id.StartsWith("$P")) {
                    id = id.Substring( id.StartsWith("P") ? 1 : 2 );
                    int alphaIdx = 0;
                    for (; char.IsDigit(id[alphaIdx]); alphaIdx++)
                        ;
                    if (alphaIdx == 0) {
                        continue; // this field is not of $PnN type, we ignore it.
                    }
                    int parIdx = int.Parse(id.Substring(0, alphaIdx));
                    string colId = id.Substring(alphaIdx);
                    int colIdx = columnIdList.IndexOf(colId);
                    if ((parIdx > 0) && (colIdx >= 0) && (parIdx<pTable.Rows) ) {
                        pTable.Matrix[parIdx - 1][colIdx] = textTable.Matrix[row][0];
                    }
                }
            }
            for(int col=0; col< pTable.Columns; col++) {
                pTable.ColumnSpecList[col].Id = columnIdList[col];
            }
            for (int row = 0; row < pTable.Rows; row++) {
                pTable.RowSpecList[row].Name = "P" + (row+1).ToString();
                pTable.RowSpecList[row].Id = fcsInfo.ColumnInfo[row].ShortName;
            }

            for (int row = 0; row < pTable.Rows; row++) {
                for (int col = 0; col < pTable.Columns; col++) {
                    if (pTable.Matrix[row][col] == null) {
                        pTable.Matrix[row][col] = "";
                    }
                }
            }

            pTable.SaveAsDataset(shortName + " (Parameters)", "The parameters information" + shortName);
        }

        public string FileNameFilter {
            get { return "Flow Cytometry Standard (*.fcs)|*.fcs"; }
        }
    }
}
