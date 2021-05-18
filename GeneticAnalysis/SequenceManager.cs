using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class SequenceManager : IPluginObject {
        IBlob blob = null;
        IDataset table;

        public SequenceManager() {
        }

        public string Name {
            get { return "SequenceManager"; }
            set { }
        }

        public string FetchSeq(int rowIndex) {
            int seqIdx = (int) (double) table.GetValueAt(rowIndex, 0);
            int seqLen = (int) (double) table.GetValueAt(rowIndex, 1);
            return FetchSeq(seqIdx, seqLen);
        }

        public string FetchSeq(int seqIdx, int seqLen) {
            if (blob == null) {
                GeneticAnalysis.App.ScriptApp.LastError = "Sequence manager not opened.";
                return null;
            }

            StringBuilder sb = new StringBuilder();
            var stream = blob.Stream;
            stream.Seek(seqIdx / 4, SeekOrigin.Begin);
            int initOffset = seqIdx % 4;
            while (seqLen > 0) {
                int v = stream.ReadByte();
                if (v < 0) break;

                byte buf = (byte)(v & 0xFF);
                buf >>= initOffset * 2;
                for (int i = 0; i < (4 - initOffset); i++) {
                    sb.Append(FastaNt.AGCT[(byte)(buf & 0x03)]);
                    seqLen--;
                    buf >>= 2;
                    if (seqLen == 0) break;
                }
                initOffset = 0;
            }
            return sb.ToString();
        }

        private Regex[] reg = null;
        public void SeqParseInit(params string[] patternList) {
            if ((patternList == null) || (patternList.Length == 0)) {
                reg = null;
                return;
            }
            reg = new Regex[patternList.Length];
            for (int i = 0; i < reg.Length; i++) {
                reg[i] = new Regex(patternList[i], RegexOptions.Compiled);
            }            
        }

        public int[] SeqParse(int rowIndex) {
            string s = FetchSeq(rowIndex);
            int[] freq = new int[reg.Length];
            //Parallel.For(0, reg.Length, i => { freq[i] = reg[i].Matches(s).Count; });
            for(int i=0; i<reg.Length; i++) freq[i] = reg[i].Matches(s).Count; 
            return freq;
        }

        public int[] FetchSeqFreq(int rowIndex, int wordSize) {
            if (blob == null) {
                GeneticAnalysis.App.ScriptApp.LastError = "Sequence manager not opened.";
                return null;
            }
            int seqIdx = (int)(double)table.GetValueAt(rowIndex, 0);
            int seqLen = (int)(double)table.GetValueAt(rowIndex, 1);
            int[] freq = new int[1<<2*wordSize];

            var stream = blob.Stream;
            stream.Seek(seqIdx / 4, SeekOrigin.Begin);
            int initOffset = seqIdx % 4;
            uint idx = 0;
            int prefixSize = wordSize - 1;
            uint idxSize = (uint) ( (1 << wordSize * 2) - 1 );
            while (seqLen > 0) {
                int v = stream.ReadByte();
                if (v < 0) break;

                byte buf = (byte)(v & 0xFF);
                buf >>= initOffset * 2;
                for (int i = 0; i < (4 - initOffset); i++) {
                    byte code = (byte)(buf & 0x03);
                    idx = idxSize & ((idx << 2) | code);
                    if (prefixSize <= 0) {
                        freq[idx]++;
                    } else {
                        prefixSize--;
                    }
                    seqLen--;
                    buf >>= 2;
                    if (seqLen == 0) break;
                }
                initOffset = 0;
            }
            return freq;
        }

        public bool OpenSequence(string blobName) {
            var app = GeneticAnalysis.App.ScriptApp;
            table = app.Dataset;
            try {
                blob = app.Folder.OpenBlob(blobName, false);
            } catch (Exception ex) {
                app.LastError = "ERROR: " + ex.Message;
                return false;
            }
            return true;
        }

        public void Close() {
            if (blob != null) blob.Close();
            blob = null;
        }

    }
}
