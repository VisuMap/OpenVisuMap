using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections;

using VisuMap.Plugin;
using VisuMap.Script;

namespace VisuMap.GeneticAnalysis {
    public class SeqAnalysis : IPluginObject {

        public SeqAnalysis() {
        }

        public string Name {
            get { return "SeqAnalysis"; }
            set { }
        }

        public SeqView OpenSequence(string blobName) {
            var app = GeneticAnalysis.App.ScriptApp;
            try {
                return new SeqView(blobName, app.Dataset);
            } catch (Exception ex) {
                app.LastError = "ERROR: " + ex.Message;
                return null;
            }
        }

        public SeqBlob CreateSequenceBlob(string blobName) {
            var app = GeneticAnalysis.App.ScriptApp;
            try {
                return new SeqBlob(blobName);
            } catch (Exception ex) {
                app.LastError = "ERROR: " + ex.Message;
                return null;
            }
        }

        public void Flip(byte[] byteSeq, int idx0, int idx1) { 
		    idx0 = Math.Min(byteSeq.Length-1, Math.Max(0, idx0)); 
		    idx1 = Math.Min(byteSeq.Length-1, Math.Max(0, idx1));
		    int N = idx1 - idx0 + 1;
		    int N2 = N/2;
		    for(int i=0; i<N2; i++) {
			    int j = N - 1 - i;
			    byte tmp = byteSeq[idx0+i];
			    byteSeq[idx0+i] = byteSeq[idx0+j];
			    byteSeq[idx0+j] = tmp;
		    }
		    for(int i=idx0; i<=idx1; i++) {
			    if ( byteSeq[i] < 4 ) byteSeq[i] ^= 0x3;
		    }
	    }

        public string FlipSequence(string seq) {
            byte[] s = SequenceToBytes(seq);
            Flip(s, 0, seq.Length - 1);
            return BytesToSequence(s, 0, s.Length);
        }

        IList<SequenceInterval> MaskToIntervals(BitArray mask) {
            var newList = new List<SequenceInterval>();

            int i = 0;
            for (; i < mask.Length; i++) {
                if (mask[i]) break;
            }
            if (i >= mask.Length) {
                return newList; // return empty list.
            }

            int n0 = i;   // the begin of the next interval
            int n1 = -1;  // the end of the next interval.
            bool inSelection = true;

            for (; i < mask.Length; i++) {
                if (inSelection) {
                    if (!mask[i]) { // terminating a selected section.
                        n1 = i - 1;
                        if (n0 <= n1) {
                            newList.Add(new SequenceInterval(n0, n1));
                        }
                        inSelection = false;
                    }
                } else {
                    if (mask[i]) { // starting a selected section.
                        n0 = i;
                        inSelection = true;
                        n1 = -1;
                    }
                }
            }

            if (inSelection) {
                newList.Add(new SequenceInterval(n0, mask.Length - 1));
            }
            return newList;
        }

        public int TotalLengthIntervals(IList<SequenceInterval> secList) {
            return secList.Sum(sec => sec.Length);
        }

        public IList<SequenceInterval> SubstractIntervals(IList<SequenceInterval> secList1, IList<SequenceInterval> secList2) {
            int minIdx = int.MaxValue;
            int maxIdx = int.MinValue;
            foreach (var sec in secList1) {
                minIdx = Math.Min(minIdx, sec.Begin);
                maxIdx = Math.Max(maxIdx, sec.End);
            }
            int N = maxIdx - minIdx + 1;

            BitArray mask = new BitArray(N);
            mask.SetAll(false);
            foreach (var sec in secList1) {
                for (int i = sec.Begin; i <= sec.End; i++) {
                    int idx = i - minIdx;
                    if ((idx >= 0) && (idx < N)) {
                        mask.Set(idx, true);
                    }
                }
            }
            foreach (var sec in secList2) {
                for (int i = sec.Begin; i <= sec.End; i++) {
                    int idx = i - minIdx;
                    if ((idx >= 0) && (idx < N)) {
                        mask.Set(idx, false);
                    }
                }
            }

            var newList = MaskToIntervals(mask);
            for (int i = 0; i < newList.Count; i++) newList[i] = newList[i].Shift(minIdx);
            return newList;
        }

        public IList<SequenceInterval> ComplementaryIntervals(IList<SequenceInterval> secList, int baseBegin, int baseEnd) {
            int N = baseEnd-baseBegin+1;
            BitArray mask = new BitArray(N);
            mask.SetAll(true);
            foreach (var sec in secList) {
                for (int i = sec.Begin; i <= sec.End; i++) {
                    int idx = i - baseBegin;
                    if ((idx >= 0) && (idx < N)) {
                        mask.Set(idx, false);
                    }
                }
            }

            var newList = MaskToIntervals(mask);
            for (int i = 0; i < newList.Count; i++) newList[i] = newList[i].Shift(baseBegin);
            return newList;
        }

        public IList<SequenceInterval> IntersectionIntervals(IList<SequenceInterval> secList1, IList<SequenceInterval> secList2) {
            // make the two lists sorted and no-overlapping.
            var list1 = UnionIntervals(secList1);
            var list2 = UnionIntervals(secList2);
            var intList = new List<SequenceInterval>();

            int j = 0;
            for (int i = 0; i < list1.Count; i++) {
                int begin = list1[i].Begin;
                int end = list1[i].End;
                for (; j < list2.Count; j++) {
                    int b = Math.Max(begin, list2[j].Begin);
                    int e = Math.Min(end, list2[j].End);
                    if (b <= e) intList.Add(new SequenceInterval(b, e));
                    if (list2[j].Begin > end) break;
                }
            }
            return intList;
        }

        public List<SequenceInterval> SortIntervalsOnLength(IList<SequenceInterval> secList) {
            List<SequenceInterval> secList2 = new List<SequenceInterval>(secList);
            secList2.Sort((s1, s2) => s1.Length - s2.Length);
            return secList2;
        }

        public IList<SequenceInterval> UnionIntervals2(IList<SequenceInterval> secList1, IList<SequenceInterval> secList2) {
            List<SequenceInterval> secList = new List<SequenceInterval>(secList1);
            secList.AddRange(secList2);
            return UnionIntervals(secList);
        }

        // merging overlapping intervals.
        public IList<SequenceInterval> UnionIntervals(IList<SequenceInterval> secList) {
            int minIdx = int.MaxValue;
            int maxIdx = int.MinValue;
            foreach(var sec in secList) {
                minIdx = Math.Min(minIdx, sec.Begin);
                maxIdx = Math.Max(maxIdx, sec.End);
            }
            int N = maxIdx - minIdx + 1;
            BitArray mask = new BitArray(N);
            mask.SetAll(false);
            foreach (var sec in secList) {
                for (int i = sec.Begin; i <= sec.End; i++) {
                    mask.Set(i - minIdx, true);
                }
            }
            var newList = MaskToIntervals(mask);
            for (int i = 0; i < newList.Count; i++) newList[i] = newList[i].Shift(minIdx);
            return newList;
        }

        public byte[] SequenceToBytes(string sequence) {
            if (sequence == null)
                return new byte[0];

            byte[] bs = new byte[sequence.Length];
            for (int i = 0; i < bs.Length; i++) {
                int k = FastaNt.ACGT.IndexOf(sequence[i]);
                bs[i] = (byte)( (k >= 0) ? k : 4 );
            }
            return bs;
        }

        public string BytesToSequence(byte[] bValues, int index, int length) {
            const string S = "ACGT";
            StringBuilder sb = new StringBuilder();
    		for(int i=index; i<(index+length); i++) 
                sb.Append( (bValues[i] >= 4) ? 'N' : S[bValues[i]&3] );
            return sb.ToString();
        }

        bool allowMismatch = false;

        public bool AllowMismatch {
            get { return allowMismatch; }
            set { allowMismatch = value; }
        }

	    public uint[] KmerFrequency(byte[] seq, int k) {
		    uint N = (uint)(1<<(2*k));
		    uint mask = (uint)(N - 1);
            uint[] freq = new uint[N];
            uint[] freq2 = new uint[freq.Length];
            uint[] tmp;
            uint idx = 0;
		    uint kLen = 0;
		    for(uint i=0; i<seq.Length; i++) {
                byte nb = seq[i];
                if (nb == 4) {  // unknown nucliotide
                    kLen = idx = 0;
                    continue;
                }
				idx = ((idx<<2) & mask) | (uint) (nb & 3);
				kLen++;
				if ( kLen >= k ) freq[idx]++;
		    }

            Multithreading.StartLoops(0, (int)N, delegate(int n) {
                // Flip the index n to n3.
                uint n2 =  ((uint)n) ^ 0xFFFFFFFF;  // A->T, G->C
                uint n3 = 0;  // Reverse the bi-bits order
                for (int i = 0; i < k; i++) {
                    n3 <<= 2;
                    n3 |= (n2 & 0x3);
                    n2 >>= 2;
                }

                freq2[n] = freq[n] + freq[n3];
            });
            tmp = freq2; freq2 = freq; freq = tmp;


            if (!allowMismatch) return freq;


            // including the single nb mutation.
            Multithreading.StartLoops(0, (int)N, delegate(int n) {
                uint sum = freq[n];
                for (int i = 0; i < 2 * k; i += 2) 
                    sum += freq[n^(1<<i)] + freq2[n^(2<<i)] + freq2[n^(3<<i)];                
                freq2[n] = sum;
            });

            // including the single nb deletions.
            tmp = freq2; freq2 = freq; freq = tmp;
            Multithreading.StartLoops(0, (int)N, delegate(int n) {
                uint sum = freq[n];
                for (int i = 0; i < k; i++)
                    for (int nb = 0; nb < 4; nb++)
                        for (int leftFlank = 0; leftFlank < 2; leftFlank++) {
                            int i2 = 2 * i;
                            int m = 0;
                            int rMask = (1 << i2) - 1;  // mask for the section on the right side of the deleted nb.
                            if (leftFlank == 1) {
                                // the (k-1) will be extend to k-mer by add nb to the left side.
                                int rightSec = n & rMask;
                                int leftSec = (n >> 2) & ~rMask;
                                m = (nb << (2 * k - 2)) | leftSec | rightSec;
                            } else {
                                // the (k-1) will be extend to k-mer by add nb to the right side.
                                int rightSec = (n & rMask) << 2;
                                int leftSec = ((n >> (i2 + 2)) << (i2+2));
                                m = leftSec | rightSec | nb;
                            }
                            sum += freq[m];
                        }
                freq2[n] = sum;
            });

            return freq2;
	    }

	    static int FindNextBit(BitArray b, bool v, int start) {            
		    for(int i=start; i<b.Count; i++)
			    if ( b[i] == v )
				    return i;		    
		    return -1;
	    }

        public void ShowRareKmer(byte[] seq, int k) {
            uint[] freq = KmerFrequency(seq, k);
            uint minFreq = (uint)(0.01 * freq.Sum(v => v) / freq.Length);

            uint N = (uint)(1 << (2 * k));
            uint mask = (uint)(N - 1);
            BitArray showMask = new BitArray(seq.Length);
            uint idx = 0;
            uint kLen = 0;

            for (int i = 0; i < seq.Length; i++) {
                if (seq[i] < 4) {
                    idx = ((idx << 2) & mask) | seq[i];
                    kLen++;
                    if ((kLen >= k) && (freq[idx] < minFreq)) {
                        showMask[i + 1 - k] = true;
                    }
                } else {
                    kLen = idx = 0;
                }
            }

            // Make the (k-1) nucliotides after the leading one visible
            int begin = FindNextBit(showMask, true, 0);
            int end = -1;
            while (begin >= 0) {
                end = FindNextBit(showMask, false, begin + 1);
                if (end < 0) 
                    break;

                int end2 = Math.Min(end + k, seq.Length);
                for (int i = end; i < end2; i++) 
                    showMask[i] = true;
                if (end2 == seq.Length) 
                    break;
                begin = FindNextBit(showMask, true, end2 + 1);
            }

            // Convert showMask to seq[].
            for (int i = 0; i < seq.Length; i++) {
                if (showMask[i])
                    seq[i] &= 0xf7;
                else
                    seq[i] |= 0x08;
            }
        }

        public void ShowFrequentKmer(byte[] seq, int k, int minFreq, int maxFreq,  int minLen) {
            uint[] freq = KmerFrequency(seq, k);
		    uint N = (uint)(1<<(2*k));
		    uint mask = (uint)(N - 1);
		    BitArray showMask = new BitArray(seq.Length);
		    uint idx = 0;
		    uint kLen = 0;
            if (maxFreq == 0) maxFreq = int.MaxValue;

            // Select the frequent kmer into showMask
		    for(int i=0; i<seq.Length; i++) {
			    if( seq[i] == 4 ) {
                    kLen = idx = 0;
                } else {
				    idx = ((idx<<2) & mask) | (uint)(seq[i] & 0x3);
				    kLen++;
                    if ((kLen >= k) && (freq[idx] >= minFreq) && (freq[idx]<=maxFreq)) {
						    showMask[i+1-k] = true;
				    }
                }
		    }

            // Filter out the short kmers from showMask.
		    int repeats = minLen - k;
		    int begin = FindNextBit(showMask, true, 0);

            while (begin >= 0) {
                int end = FindNextBit(showMask, false, begin + 1);
                if (end < 0) end = showMask.Count;
                if ((end - begin) < repeats) {
                    for (int i = begin; i < end; i++) showMask[i] = false;
                }
                begin = FindNextBit(showMask, true, end + 1);
            }

            // Make the (k-1) nucliotides after the leading one visible
            begin = FindNextBit(showMask, true, 0);
            while(begin>=0) {
                int end = FindNextBit(showMask, false, begin + 1);
                if (end < 0) break;
                int end2 = Math.Min(end + k, seq.Length);
                for (int i = end; i < end2; i++) showMask[i] = true;
                if (end2 == seq.Length) break;
                begin = FindNextBit(showMask, true, end2 + 1);
            }

            // Convert showMask to seq[].
		    for(int i=0; i<seq.Length; i++) {
			    if ( showMask[i] ) {
				    seq[i] &= 0xf7;
			    } else {
				    seq[i] |= 0x08;
			    }
		    }
	    }

        public IHeatMap CalculateCurvature(IHeatMap hm, string rowId, short rowType, IList<IBody> bodyList, int scansPerNode, int skipSize) {
            var b = bodyList;
            int N = scansPerNode;
            List<double> cv = new List<double>();
            List<string> idList = new List<string>();
	        for(var i=0; i<(b.Count - 2*N - 1); i+=skipSize) {
		        var iN = i+N;
		        var iN2 = i+2*N;

		        var x0 = b[iN].X - b[i].X;
		        var y0 = b[iN].Y - b[i].Y;
		        var z0 = b[iN].Z - b[i].Z;

		        var x1 = b[iN2].X - b[iN].X;
		        var y1 = b[iN2].Y - b[iN].Y;
		        var z1 = b[iN2].Z - b[iN].Z;

	
		        var x2 = x0+x1;
		        var y2 = y0+y1;
		        var z2 = z0+z1;

		        // K = 2*sin(A)/s, 
		        // where A is the angle between B[k+1]-B[k] and B[k+2]-B[k+1], 
		        // s is the length of B[k+2]-B[k].

		        var n0 = x0*x0+y0*y0+z0*z0;
		        var n1 = x1*x1+y1*y1+z1*z1;
		        var n2 = x2*x2+y2*y2+z2*z2;
		        if ( (n0>0) && (n1>0) && (n2>0) ) {
			        var dot = x0*x1+y0*y1+z0*z1;
			        var sinA2 = 1 - (dot*dot)/(n0*n1);   // sinA2 = sin(A)^2.
			        if (  sinA2>0 ) {
				        cv.Add(2*Math.Sqrt(sinA2/n2));
			        }
		        } else {
                    cv.Add(0);
		        }
		        idList.Add(b[iN].Id);
	        }

            var app = GeneticAnalysis.App.ScriptApp;
            if (hm == null) {
                var fs = app.FindFormList("HeatMap");
                if (fs.Count > 0) hm = fs[0] as IHeatMap;                
            }

            if (hm == null) {
                var tb = app.New.NumberTable(cv.ToArray());
                for (var col = 0; col < tb.Columns; col++) {
                    tb.ColumnSpecList[col].Id = idList[col];
                }
                tb.RowSpecList[0].Type = rowType;
                if (rowId != null) tb.RowSpecList[0].Id = rowId;
                hm = tb.ShowHeatMap();
                hm.ReadOnly = true;
                hm.SpectrumType = 2;
            } else {
                var tb = hm.GetNumberTable();
                if (tb.Columns < cv.Count) {
                    int newColumns = cv.Count - tb.Columns;
                    tb.AddColumns(newColumns);
                    for (var i = 0; i < newColumns; i++) {
                        int col = cv.Count - newColumns + i;
                        tb.ColumnSpecList[col].Id = idList[col];
                    }
                }
                var uf = (rowId != null) ? new UniqueNameFinder(tb.RowSpecList.Select(rs => rs.Id)) : null;

                tb.AddRows(1);
                for (var col = 0; col < cv.Count; col++) tb.Matrix[tb.Rows - 1][col] = cv[col];
                tb.RowSpecList[tb.Rows - 1].Type = rowType;
                if (rowId != null) {
                    tb.RowSpecList[tb.Rows - 1].Id = uf.LookupName(rowId);
                }
                hm.Title = "N: " + tb.Rows;
                hm.Redraw();
            }
            hm.CentralizeColorSpectrum();
            return hm;
        }
    }
}
