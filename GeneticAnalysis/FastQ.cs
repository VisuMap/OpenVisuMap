using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.GZip;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class FastQ : IFileImporter {
        public static string ACGT = "ACGTN";

        public bool ImportFile(string fileName) {
            IVisuMap app = GeneticAnalysis.App.ScriptApp;
            var fn = fileName.ToLower();
            if (!(
                 fn.EndsWith(".fastq") 
              || fn.EndsWith(".fq") 
              || fn.EndsWith(".fq.gz") 
              || fn.EndsWith(".fastq.gz")
              )) return false;

            FileInfo fInfo = new FileInfo(fileName);
            string shortName = fInfo.Name.Substring(0, fInfo.Name.LastIndexOf('.'));
            List<string> headList = new List<string>();
            List<long> seqIdx = new List<long>();
            List<long> seqLen = new List<long>();
            var seqBlob = new SeqBlob(shortName);

            using (StreamReader trH = new StreamReader(fileName)) {
                StreamReader tr = null;
                if (fn.EndsWith(".gz")) {
                    GZipInputStream gzStream = new GZipInputStream(trH.BaseStream);
                    tr = new StreamReader(gzStream);
                } else {
                    tr = trH;
                }

                int unknowLetters = 0;
                List<char> unknownList = new List<char>();

                while (!tr.EndOfStream) {
                    string line = null;
                    try {
                        line = tr.ReadLine();
                    } catch (ICSharpCode.SharpZipLib.SharpZipBaseException) {
                        break;
                    }

                    if (line.StartsWith("@")) {
                        string sHeader = line.Split(' ')[0];
                        sHeader = sHeader.Substring(1);
                        headList.Add(sHeader);
                        long sBegin = seqBlob.Length;
                        seqIdx.Add(sBegin);

                        line = tr.ReadLine();
                        foreach (char c in line) {
                            char cc = char.ToUpper(c);
                            int k = ACGT.IndexOf(cc);
                            if (k >= 0) {
                                seqBlob.AddLetter(k);
                            } else if (! char.IsWhiteSpace(c)) {
                                seqBlob.AddLetter(SeqBlob.UKNOWN_LETTER);  // unknown letter.
                                if ( unknownList.Count < 100 ) unknownList.Add(c);
                                unknowLetters++;
                            }
                        }
                        seqLen.Add(seqBlob.Length - sBegin); // add the length

                        // Ignore the quality lines
                        tr.ReadLine();
                        tr.ReadLine();  // the quality indicators.

                        if ((headList.Count % 1000) == 0) { app.Title = "N: " + headList.Count; Application.DoEvents(); }
                    }
                }

                if (unknowLetters > 0) {
                    StringBuilder sb = new StringBuilder();
                    foreach (char c in unknownList) { sb.Append(c); sb.Append(','); }
                    if (unknownList.Count == 100) sb.Append("...");
                    MessageBox.Show("Converted " + unknowLetters + " unknown/meta letters to 'N': " + sb.ToString());
                }
            }

            string oldPresion = app.Folder.NumberPrecision;
            app.Folder.NumberPrecision = "g10";
            IFreeTable table = app.New.FreeTable();
            table.AddColumn("SeqIdx", true);
            table.AddColumn("SeqLen", true);
            table.AddColumn("Header", false);
            table.ColumnSpecList[0].Name = seqBlob.Name;            
            table.AddRows("Id", headList.Count);
            for (int row = 0; row < headList.Count; row++) {
                var rs = table.RowSpecList[row];
                var R = table.Matrix[row];
                R[0] = seqIdx[row].ToString();
                R[1] = seqLen[row].ToString();
                R[2] = headList[row];
            }

            seqBlob.Dispose();
            string dsName = table.SaveAsDataset(shortName, "");
            if (dsName == null) {
                MessageBox.Show("Cannot import the data: " + app.LastError);
                return false;
            } else {
                var newDataset = app.Folder.OpenDataset(dsName);
            }
            app.Folder.NumberPrecision = oldPresion;

            // ValidateTable();
            return true;
        }
        
        public string FileNameFilter {
            get { return "FastQ Files(*.fq *.fastq *.fq.gz *.fastq.gz)|*.fq;*.fastq;*.fq.gz;*.fastq.gz"; }
        }
    }
}
