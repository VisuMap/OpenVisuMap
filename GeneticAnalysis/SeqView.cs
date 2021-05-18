using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class SeqView {
        IDataset table;
        byte[] seqBuf;
        private Regex[] reg = null;

        uint seqSize; // The number of letters in sequence.
        byte wd;     // The current letter buffer.
        uint wdIdx;   // The index of wd in the the seqBuf;
        int wdLen;   // number of letters in the letter buffer.

        uint initIdx = 0;  // The index of the first letter in seqBuf[].
        static bool shownWarning = false;


        public SeqView(string blobName, IDataset table) {
            this.table = table;
            var app = GeneticAnalysis.App.ScriptApp;
            table = app.Dataset;
            var blob = app.Folder.OpenBlob(blobName, false);
            int len = 0;
            const long maxLen = 1000000000;
            if (blob.Length> maxLen) {
                if (!shownWarning) {
                    MessageBox.Show("Sequence too long (" + 2 * blob.Length + ")! Trucated the sequence to 2 billion nucliotides");
                    shownWarning = true;
                }
                len = (int) maxLen;
            } else {
                len = (int)blob.Length;
            }
            seqBuf = new byte[len];
            blob.Stream.Read(seqBuf, 0, len);
            seqSize = (uint)(2 * len);
            
            if (!string.IsNullOrEmpty(blob.ContentType)) {
                string[] fs = blob.ContentType.Split(' ');
                if (fs.Length == 2) {
                    long actualSize = long.Parse(fs[1]);
                    if (actualSize < seqSize)
                        seqSize = (uint)actualSize;
                }
            }
            
            blob.Close();
            Seek(0);
        }

        private SeqView() {
        }

        public void Close() {
            table = null;
            seqBuf = null;
            reg = null;
            seqSize = 0;
        }

        public SeqView SubSeq(uint beginIdx, uint length) {
            if ( (beginIdx<0) || (length<=0) || ( (beginIdx+length) > seqSize) ) return null;
            var sv = new SeqView();
            sv.table = table;
            sv.seqBuf = seqBuf;
            sv.seqSize = length;
            sv.initIdx = initIdx + beginIdx;
            sv.Seek(0);
            return sv;
        }

        public byte[] AllAsBytes() {
            byte[] ret = new byte[seqSize];
            uint I = seqSize / 2 + seqSize % 2;
            for(int i=0; i<I; i++) {
                ret[2 * i] = (byte) (seqBuf[i] & 0x0f);
                if ( (2 * i + 1) < seqSize )
                    ret[2 * i + 1] = (byte)(seqBuf[i]>>4);
            }
            return ret;
        }

        public bool Seek(uint offset) {
            if (offset >= seqSize) return false;
            uint idx = offset + initIdx;
            wdIdx = idx / 2;
            wd = seqBuf[wdIdx];
            wd >>= (byte)((idx % 2) * 4);
            wdLen = (int)(2 - idx % 2);
            return true;
        }

        public int GetLetter() {
            if (wdLen <= 0) {
                wdIdx++;
                uint remaining = (uint)(initIdx + seqSize - wdIdx * 2);
                if (remaining > 0) {  // remaining letter in the buffer.
                    wd = seqBuf[wdIdx];
                    wdLen = (int)Math.Min(2, remaining);
                } else {
                    return -1;
                }
            }
            int letter = wd & 0x0F;
            wd >>= 4;
            wdLen--;
            return letter;
        }

        public uint Length {
            get { return seqSize; }
        }
       

        public string FetchSeq(uint seqIdx, uint seqLen) {
            StringBuilder sb = new StringBuilder();
            Seek(seqIdx);
            while (seqLen-- > 0) {
                int letter = GetLetter();
                if (letter < 0) return null;                 
                sb.Append(FastaNt.ACGT[letter]);
            }
            return sb.ToString();
        }

        public string ReadSeq(int length) {
            StringBuilder sb = new StringBuilder();
            while (length-- > 0) {
                int letter = GetLetter();
                if (letter < 0) return sb.ToString();
                sb.Append(FastaNt.ACGT[letter]);
            }
            return sb.ToString();
        }

        public string FetchSeq(int rowIndex) {
            uint seqIdx = (uint)(double)table.GetValueAt(rowIndex, 0);
            uint seqLen = (uint)(double)table.GetValueAt(rowIndex, 1);
            return FetchSeq(seqIdx, seqLen);
        }

        public void FetchSeqIndex(uint seqIdx, uint seqLen, byte[] values, int startIndex) {
            Seek(seqIdx);
            while (seqLen-- > 0) {
                int letter = GetLetter();
                if (letter < 0) return;
                values[startIndex++] = (byte)letter;
            }
        }

        string FuzzyPattern(string p) {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < p.Length; i++) {
                if (sb.Length > 0) sb.Append("|");

                if (i > 0) sb.Append(p.Substring(0, i));
                sb.Append("[ACGT]");
                if (i < (p.Length - 1)) sb.Append(p.Substring(i + 1, p.Length - i - 1));
            }

            return sb.ToString();
        }

        public void SeqParseInit(string patterns, bool fuzzy) {
            if (patterns == null) {
                reg = null;
                return;
            }
            string[] patternList = patterns.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (patternList.Length == 0) {
                reg = null;
                return;
            }

            if (fuzzy) {
                for (int i = 0; i < patternList.Length; i++) patternList[i] = FuzzyPattern(patternList[i]);
            }

            reg = new Regex[patternList.Length];
            for (int i = 0; i < reg.Length; i++) {
                reg[i] = new Regex(patternList[i], RegexOptions.Compiled);
            }
        }

        public int SeqParseDimension() {
            return ( reg == null ) ? 0 : reg.Length;
        }

        public int[] SeqParse(int rowIndex) {
            string seq = FetchSeq(rowIndex);
            int[] freq = new int[reg.Length];
            Multithreading.StartLoops(0, reg.Length,
                i => { freq[i] = reg[i].Matches(seq).Count; });
            return freq;
        }

        public int[] SeqParse2(uint seqIdx, uint seqLen) {
            string seq = FetchSeq(seqIdx, seqLen);
            int[] freq = new int[reg.Length];
            if (seq == null) return freq;           
            Multithreading.StartLoops(0, reg.Length,
                i => { freq[i] = reg[i].Matches(seq).Count; });
            return freq;
        }

        public void FetchFreqTable(int wordSize, int columnOffset) {
            for (int row = 0; row < table.Rows; row++) {
                int[] freq = FetchSeqFreq(row, wordSize);
                for (int col = 0; col < freq.Length; col++) {
                    table.SetValueAt(row, columnOffset + col, freq[col]);
                }
            }
        }

        public int[] FetchSeqFreq(int rowIndex, int wordSize) {
            uint seqIdx = (uint)(double)table.GetValueAt(rowIndex, 0);
            uint seqLen = (uint)(double)table.GetValueAt(rowIndex, 1);
            int[] freq = new int[1 << 2 * wordSize];

            uint idxFreq = 0;  // index for the frequency counts.
            uint idxSize = (uint)((1 << wordSize * 2) - 1);
            int prefixSize = wordSize - 1;  // we need to read-in first prefixSize letters without updating counts.

            Seek(seqIdx);
            while (seqLen-- > 0) {
                int letter = GetLetter();
                if (letter < 0) return null;                
                idxFreq = idxSize & ((idxFreq << 2) | (uint)letter);  
                if (prefixSize <= 0) {
                    freq[idxFreq]++;
                } else {
                    prefixSize--;
                }

            }
            return freq;
        }
    }
}
