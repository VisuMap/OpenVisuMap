// GaHelp.js
// Help functions for GeneticAnalysis module.
//

function OpenSequenceMap(sa, ds) {
	var blobs = vv.Folder.GetBlobList();
	var nm = ds.Name.replace(".CDS","").replace("CDS", "");
	var idx = blobs.IndexOf(nm);
	nm = ( idx < 0 ) ? blobs[0] : blobs[idx];
	var sv = sa.OpenSequence(nm);
	
	var rows = 40;
	var N = sv.Length;
	var columns = parseInt(N / rows, 10) + ( (N%rows > 0) ? 1 : 0 );
	var NN = rows * columns;
	var seqTable = New.ByteArray(NN);	
	
	sv.FetchSeqIndex(0, sv.Length, seqTable, 0);            
	var sm = New.SequenceMap(seqTable, rows, columns).Show();
	sm.Title = sm.SequenceName = nm;
       sm.ReadOnly = true;
	return sm;
}

var cs = New.CsObject("RandomizeSeq", `
	public void ExtractGeneInfo(IDataset ds) {
		for(int row = 0; row<ds.Rows; row++) {
			string s = ds.GetDataAt(row, 2);
			int idx0 = s.IndexOf("[gene=") + 6;
			int idx1 = s.IndexOf(']', idx0);
			ds.BodyList[row].Name = s.Substring(idx0, idx1-idx0);
			idx0 = s.IndexOf("location=") + 9;
			idx1 = s.IndexOf(']', idx0);
			string loc = s.Substring(idx0, idx1-idx0);
			int strand = 1;
			if ( loc.StartsWith("complement") ) {
				loc = loc.Substring("complement".Length);
				strand = -1;
				loc = loc.Trim(new char[]{'(', ')'});
			}
			if ( loc.StartsWith("join") ) {
				loc = loc.Substring("join".Length);
				loc = loc.Trim(new char[]{'(', ')'});
			}
			string[] exList = loc.Split(',');
			char[] prefix = new char[] {'<', '>'};
			string sBegin = "";
			string sEnd = "";
			foreach(string ex in exList) {
				string[] fs = ex.Split('.');
				string sB = fs[0].TrimStart(prefix);
				string sE = (fs.Length == 3) ? fs[2].TrimStart(prefix) : sB;
				if( sBegin.Length > 0) {
					sBegin += ",";
					sEnd += ",";			
				}
				sBegin += sB;
				sEnd += sE;
			}

			ds.SetStringAt(row, 3, sBegin);
			ds.SetStringAt(row, 4, sEnd);
			ds.SetValueAt(row, 5, strand);
			if (strand == -1)
				ds.BodyList[row].Type = 1;
		}
	}

	public void Randomize(byte[] s, int idx0, int idx1) {
		var rg = new Random(321);
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));
		for(int i=idx0; i<=idx1; i++) {
			if ( s[i] == 4 ) continue;
			int j = 0;
			int n = 0;
			for(n=0; n<20; n++) {
				j = idx0 + rg.Next(idx1 + 1 -idx0);
				if ( s[j] != 4 ) break;
			}
			if ( n < 20 ) {
				byte tmp = s[j];
				s[j] = s[i];
				s[i] = tmp;
			}
		}
	}

	public void Reverse(byte[] s, int idx0, int idx1) {
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));
		int N = idx1 - idx0 + 1;
		int N2 = N/2;
		for(int i=0; i<N2; i++) {
			int j = N - 1 - i;
			byte tmp = s[idx0+i];
			s[idx0+i] = s[idx0+j];
			s[idx0+j] = tmp;
		}
	}

	public void Revert(byte[] s, int idx0, int idx1) {
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));
		for(int i=idx0; i<=idx1; i++) {
			if ( s[i] < 4 ) s[i] ^= 0x3;
		}
	}

	public void Flip(byte[] s, int idx0, int idx1) {
		Reverse(s, idx0, idx1);
		Revert(s, idx0,idx1);
	}

	public void FlipSeq(byte[] s, int idx0, int idx1) {
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));
		int N = idx1 - idx0 + 1;
		int N2 = N/2;
		for(int i=0; i<N2; i++) {
			int j = N - 1 - i;
			byte tmp = s[idx0+i];
			s[idx0+i] = s[idx0+j];
			s[idx0+j] = tmp;
		}
		for(int i=idx0; i<=idx1; i++) {
			if ( s[i] < 4 ) s[i] ^= 0x3;
		}
	}

	public int CpGCount(byte[] s, int idx0, int idx1) {
		int cnt = 0 ;
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));
		for(int i=idx0; i<=(idx1-1); i++) {
			if ((s[i]==1) && (s[i+1]==2)) {
				cnt++;
				i++;
			}
		}
		return cnt;
	}

	public int atCount(byte[] s, int idx0, int idx1) {
		int cnt = 0 ;
		for(int i=idx0; i<=(idx1-1); i++) {
			if ((s[i]==0) && (s[i+1]==3)) {
				cnt++;
				i++;
			}
		}
		return cnt;
	}

	public void Hide(byte[] s, int idx0, int idx1) {
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));
		for(int i=idx0; i<=idx1; i++) {
			if ( (i>=0) && (i<s.Length) ) s[i] |= 0x08;
		}
	}

	public void Show(byte[] s, int idx0, int idx1) {
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));
		for(int i=idx0; i<=idx1; i++) {
			if ( (i>=0) && (i<s.Length) ) s[i] &= 0xF7;
		}
	}

	public void HideShow(byte[] s, int idx0, int idx1) {
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));
		for(int i=idx0; i<=idx1; i++) {
			if ( (i>=0) && (i<s.Length) ) {
				s[i] ^= 0x08;
			}
		}
	}

	public void RightAlignment(byte[] s, int start, int columns) {
		int shift = 0;
		for(int i=start+columns-1; i>=start; i--) {
			if ( s[i] < 4) break;
			shift++;
		}
		for(int i=start+columns-1; i>=start; i--) {
			int i2 = i - shift;
			s[i] = (i2>=start) ? s[i2] : (byte)4;
		}
	}

	public void LeftAlignment(byte[] s, int start, int columns) {
		int shift = 0;
		for(int i=start; i<start+columns; i++) {
			if ( s[i] < 4) break;
			shift++;
		}
		int endIndex = start+columns-1;
		for(int i=start; i<start+columns; i++) {
			int i2 = i+shift;
			s[i] = (i2<=endIndex) ? s[i2] : (byte)4;
		}
	}

	public bool SortRows(byte[] s, int rows, int columns, ISequenceMap pp) {
		int[] sizeList = new int[rows];
		int[] idxList = new int[rows];
		byte[] s2 = new byte[s.Length];
		for(int row=0; row<rows; row++) {
			idxList[row] = row;
			sizeList[row] = 0;
			int n0 = columns*row;
			for(int n=n0+columns-1; n>=n0; n--) {
				if ( s[n] < 4 ) {
					sizeList[row] = n - n0;
					break;
				}
			}
		}
		Array.Sort(sizeList, idxList);
		for(int i=0; i<s2.Length; i++) s2[i] = 4;
		for(int row=0; row<rows; row++)
			Array.Copy(s, idxList[row]*columns, s2, row*columns, columns);
		Array.Copy(s2, s, s2.Length);

		var itemSections = pp.AllItemSections();
		if ( itemSections.Count == rows ) {
			for(int row=0; row<rows; row++) {
				if ( itemSections[row].Begin != (row*columns) ) {
					return false;
				}
			}
			var allItems = pp.AllItems;
			for(int r=0; r<rows; r++) {
				int k = idxList[r]; /* moves k-th row to r-th row. */
				var sec = itemSections[k];
				pp.SetItemSection(allItems[k], r*columns, r*columns+sec.Length-1);
			}
		}
		return true;
	}

	public List<SequenceInterval> BuildSections(byte[] seq) {
		var sList = new List<SequenceInterval>();
		bool inSec = false;
		int idxBegin = -1;
		for(int i=0; i<seq.Length; i++) {
			if ( inSec ) {
				if ( seq[i] >= 4 ) {
					if( idxBegin>=0 ) {
						sList.Add( new SequenceInterval(idxBegin, i-1) );
					}
					inSec = false;
				}
			} else {
				if ( seq[i] < 4 ) {
					idxBegin = i;
					inSec = true;
				}
			}
		}
		if ( inSec && (idxBegin < seq.Length) ) {
			sList.Add( new SequenceInterval(idxBegin, seq.Length-1) );
		}
		return sList;
	}

	public float[] FindPattern(byte[] seq, string p) {
		const string S = \"ACGT\";
		float[] R = new float[seq.Length-p.Length+1];
		if ( p.Length == 1 ) {
			int a = S.IndexOf(p[0]);
			for(int i=0; i<seq.Length; i++)
				R[i] = (seq[i]==a) ? 1 : 0;
		} else if (p.Length == 2) {
			int a = (S.IndexOf(p[0])*4) + S.IndexOf(p[1]);
			for(int i=1; i<seq.Length; i++)
				R[i-1] = ((seq[i-1]*4+seq[i])==a) ? 1 : 0;
		} else {
			int a = (S.IndexOf(p[0])*16) + S.IndexOf(p[1])*4 + S.IndexOf(p[2]);
			for(int i=2; i<seq.Length; i++)
				R[i-2] = ((seq[i-2]*16 + seq[i-1]*4 + seq[i])==a) ? 1 : 0;
		}
		return R;
	}

	public double[] GetMoment(byte[] seq) {
		double[] mem = new double[4];
		for(int i=0; i<seq.Length; i++) {
			if ( seq[i] < 4 ) {
				mem[seq[i]] += i;
			}
		}
		return mem;
	}

	public SequenceInterval LocateOneGene(IDataset ds, ISequenceMap hm, 
			IList<SequenceInterval> exomeRegions, IList<SequenceInterval> antiExomes, 
			string transId, bool antiSense) {
		List<int> iBegin = (List<int>) New.IntArray();
		List<int> iEnd = (List<int>) New.IntArray();
		int rowIdx = ds.IndexOfRow(transId); 
	
		if ( rowIdx < 0 ) {
			vv.Message("Invalid Id: " + transId);
			return New.SequenceInterval(0,0);
		}

		List<int> exBegins = (List<int>) New.IntArray(ds.GetDataAt(rowIdx, 3));
		List<int> exEnds = (List<int>) New.IntArray(ds.GetDataAt(rowIdx, 4));
	
		if ( exBegins.Count == 1 )
			return New.SequenceInterval(exBegins[0]-1, exEnds[0]-1);	
		iBegin.AddRange(exBegins);
		iEnd.AddRange(exEnds);

		if ( (exomeRegions != null) && (antiExomes!=null) )
			for(int exIdx=0; exIdx<exBegins.Count; exIdx++) {
				var exSec = New.SequenceInterval(exBegins[exIdx]-1, exEnds[exIdx]-1);
				exSec = exSec.Shift(-hm.BaseLocation);
				if ( antiSense )
					antiExomes.Add(exSec);
				else
					exomeRegions.Add(exSec);
			}
	
		for(int k=0; k<iBegin.Count; k++) { 
			iBegin[k]--; 
			iEnd[k]--; 
		}
		
		int minIdx = 1000 * 1000000;
		int maxIdx = -1000;
		for(int n=0; n<iBegin.Count; n++) {
			minIdx = Math.Min(minIdx, iBegin[n]);
			maxIdx = Math.Max(maxIdx, iBegin[n]);
			minIdx = Math.Min(minIdx, iEnd[n]);
			maxIdx = Math.Max(maxIdx, iEnd[n]);
		}
		return New.SequenceInterval(minIdx, maxIdx);
	}
`);

