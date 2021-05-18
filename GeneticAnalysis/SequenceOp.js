//File: SequenceOp.js
//  Provide various sequence related services.
//MenuLabels Randomize Reverse Revert Flip CpGRatio atRatio Hide Show HideShow ToRegion1 ToRegion2 SeRegion1 SeRegion2 SeRegion3 SeRegion4 RevSelection Reset MergeSelections FlipSelections DynoFlip RightAlignment LeftAlignment SortRowsByLength BuildItems CpGDensity ExtSelection

var cs = New.CsObject("RandomizeSeq", "\
	public void Randomize(byte[] s, int idx0, int idx1) {\
		var rg = new Random(321);\
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));\
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));\
		for(int i=idx0; i<=idx1; i++) {\
			if ( s[i] == 4 ) continue;\
			int j = 0;\
			int n = 0;\
			for(n=0; n<20; n++) {\
				j = idx0 + rg.Next(idx1 + 1 -idx0);\
				if ( s[j] != 4 ) break;\
			}\
			if ( n < 20 ) {\
				byte tmp = s[j];\
				s[j] = s[i];\
				s[i] = tmp;\
			}\
		}\
	}\
\
	public void Reverse(byte[] s, int idx0, int idx1) {\
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));\
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));\
		int N = idx1 - idx0 + 1;\
		int N2 = N/2;\
		for(int i=0; i<N2; i++) {\
			int j = N - 1 - i;\
			byte tmp = s[idx0+i];\
			s[idx0+i] = s[idx0+j];\
			s[idx0+j] = tmp;\
		}\
	}\
\
	public void Revert(byte[] s, int idx0, int idx1) {\
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));\
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));\
		for(int i=idx0; i<=idx1; i++) {\
			if ( s[i] < 4 ) s[i] ^= 0x3;\
		}\
	}\
\
	public void Flip(byte[] s, int idx0, int idx1) {\
		Reverse(s, idx0, idx1);\
		Revert(s, idx0,idx1);\
	}\
\
	public int CpGCount(byte[] s, int idx0, int idx1) {\
		int cnt = 0 ;\
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));\
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));\
		for(int i=idx0; i<=(idx1-1); i++) {\
			if ((s[i]==1) && (s[i+1]==2)) {\
				cnt++;\
				i++;\
			}\
		}\
		return cnt;\
	}\
\
	public int atCount(byte[] s, int idx0, int idx1) {\
		int cnt = 0 ;\
		for(int i=idx0; i<=(idx1-1); i++) {\
			if ((s[i]==0) && (s[i+1]==3)) {\
				cnt++;\
				i++;\
			}\
		}\
		return cnt;\
	}\
\
	public void Hide(byte[] s, int idx0, int idx1) {\
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));\
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));\
		for(int i=idx0; i<=idx1; i++) {\
			if ( (i>=0) && (i<s.Length) ) s[i] |= 0x08;\
		}\
	}\
\
	public void Show(byte[] s, int idx0, int idx1) {\
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));\
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));\
		for(int i=idx0; i<=idx1; i++) {\
			if ( (i>=0) && (i<s.Length) ) s[i] &= 0xF7;\
		}\
	}\
\
	public void HideShow(byte[] s, int idx0, int idx1) {\
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));\
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));\
		for(int i=idx0; i<=idx1; i++) {\
			if ( (i>=0) && (i<s.Length) ) {\
				s[i] ^= 0x08;\
			}\
		}\
	}\
\
	public void RightAlignment(byte[] s, int start, int columns) {\
		int shift = 0;\
		for(int i=start+columns-1; i>=start; i--) {\
			if ( s[i] < 4) break;\
			shift++;\
		}\
		for(int i=start+columns-1; i>=start; i--) {\
			int i2 = i - shift;\
			s[i] = (i2>=start) ? s[i2] : (byte)4;\
		}\
	}\
\
	public void LeftAlignment(byte[] s, int start, int columns) {\
		int shift = 0;\
		for(int i=start; i<start+columns; i++) {\
			if ( s[i] < 4) break;\
			shift++;\
		}\
		int endIndex = start+columns-1;\
		for(int i=start; i<start+columns; i++) {\
			int i2 = i+shift;\
			s[i] = (i2<=endIndex) ? s[i2] : (byte)4;\
		}\
	}\
\
	public bool SortRows(byte[] s, int rows, int columns, ISequenceMap pp) {\
		int[] sizeList = new int[rows];\
		int[] idxList = new int[rows];\
		byte[] s2 = new byte[s.Length];\
		for(int row=0; row<rows; row++) {\
			idxList[row] = row;\
			sizeList[row] = 0;\
			int n0 = columns*row;\
			for(int n=n0+columns-1; n>=n0; n--) {\
				if ( s[n] < 4 ) {\
					sizeList[row] = n - n0;\
					break;\
				}\
			}\
		}\
		Array.Sort(sizeList, idxList);\
		for(int i=0; i<s2.Length; i++) s2[i] = 4;\
		for(int row=0; row<rows; row++)\
			Array.Copy(s, idxList[row]*columns, s2, row*columns, columns);\
		Array.Copy(s2, s, s2.Length);\
\
		var itemSections = pp.AllItemSections();\
		if ( itemSections.Count == rows ) {\
			for(int row=0; row<rows; row++) {\
				if ( itemSections[row].Begin != (row*columns) ) {\
					return false;\
				}\
			}\
			var allItems = pp.AllItems;\
			for(int r=0; r<rows; r++) {\
				int k = idxList[r]; /* moves k-th row to r-th row. */\
				var sec = itemSections[k];\
				pp.SetItemSection(allItems[k], r*columns, r*columns+sec.Length-1);\
			}\
		}\
		return true;\
	}\
\
	public List<SequenceInterval> BuildSections(byte[] seq) {\
		var sList = new List<SequenceInterval>();\
		bool inSec = false;\
		int idxBegin = -1;\
		for(int i=0; i<seq.Length; i++) {\
			if ( inSec ) {\
				if ( seq[i] >= 4 ) {\
					if( idxBegin>=0 ) {\
						sList.Add( new SequenceInterval(idxBegin, i-1) );\
					}\
					inSec = false;\
				}\
			} else {\
				if ( seq[i] < 4 ) {\
					idxBegin = i;\
					inSec = true;\
				}\
			}\
		}\
		if ( inSec && (idxBegin < seq.Length) ) {\
			sList.Add( new SequenceInterval(idxBegin, seq.Length-1) );\
		}\
		return sList;\
	}\
	public float[] FindPattern(byte[] seq, string p) {\
		const string S = \"ACGT\";\
		float[] R = new float[seq.Length-p.Length+1];\
		if ( p.Length == 1 ) {\
			int a = S.IndexOf(p[0]);\
			for(int i=0; i<seq.Length; i++)\
				R[i] = (seq[i]==a) ? 1 : 0;\
		} else if (p.Length == 2) {\
			int a = (S.IndexOf(p[0])*4) + S.IndexOf(p[1]);\
			for(int i=1; i<seq.Length; i++)\
				R[i-1] = ((seq[i-1]*4+seq[i])==a) ? 1 : 0;\
		} else {\
			int a = (S.IndexOf(p[0])*16) + S.IndexOf(p[1])*4 + S.IndexOf(p[2]);\
			for(int i=2; i<seq.Length; i++)\
				R[i-2] = ((seq[i-2]*16 + seq[i-1]*4 + seq[i])==a) ? 1 : 0;\
		}\
		return R;\
	}\
	public double[] GetMoment(byte[] seq) {\
		double[] mem = new double[4];\
		for(int i=0; i<seq.Length; i++) {\
			if ( seq[i] < 4 ) {\
				mem[seq[i]] += i;\
			}\
		}\
		return mem;\
	}\
");
// =================================================================================

// =================================================================================
var seq = pp.SequenceTable;
var label = vv.EventSource.Item;
var ss = pp.SelectedSections();
var s = ( ss.Count >= 1 ) ? ss[0] : New.SequenceInterval(0,seq.Length-1);
var sa = vv.FindPluginObject("SeqAnalysis");

switch( label ) {
	case "Randomize":
		cs.Randomize(seq, s.Begin, s.End);
		break;

	case "Reverse":
		cs.Reverse(seq, s.Begin, s.End);
		break;

	case "Revert":
		cs.Revert(seq, s.Begin, s.End);
		break;

	case "Flip":
		cs.Flip(seq, s.Begin, s.End);
		break;

	case "FlipSelections":
		var sList = sa.UnionIntervals(ss);
		for(var s in sList) {
			cs.Flip(seq, s.Begin, s.End);
		}
		break;

	case "DynoFlip":
		var m0 = cs.GetMoment( sa.SequenceToBytes(pp.GetSequence(0, pp.MapColumns-1)) );
		for(var row=1; row<pp.MapRows; row++) {
			var rBegin = row*pp.MapColumns;
			var rEnd = (row+1)*pp.MapColumns - 1;
			var R = sa.SequenceToBytes(pp.GetSequence(rBegin, rEnd));
			var m = cs.GetMoment( R );
			cs.Flip(R, 0, R.Length-1);
			var mf = cs.GetMoment( R );

			var d = 0;
			var df = 0;
			for(var k=0; k<4; k++) {
				d += Math.abs(m[k] - m0[k]);
				df += Math.abs(mf[k] - m0[k]);
			}
			if ( df < d ) {
				sa.Flip(seq, rBegin, rEnd);
			}
		}
		break;

	case "CpGRatio":
		var cgCnt=cs.CpGCount(seq, s.Begin, s.End);
		var ratio = 100*2*cgCnt/(s.End-s.Begin+1);
		vv.Message("CpG Ratio: " + ratio.ToString("f3") + "%");
		break;

	case "atRatio":
		var atCnt=cs.atCount(seq, s.Begin, s.End);
		var ratio = 100*2*atCnt/(s.End-s.Begin+1);
		vv.Message("ApT Ratio: " + ratio.ToString("f3") + "%");
		break;

	case "Hide":
		if ( ss.Count == 0 ) {
			cs.Hide(seq, 0, seq.Length-1);
		} else {
			for(var s in ss)  cs.Hide(seq, s.Begin, s.End);	
		}
		break;

	case "Show":
		cs.Hide(seq, 0, seq.Length-1);
		for(var s in ss) cs.Show(seq, s.Begin, s.End);		
		break;

	case "HideShow":
		cs.HideShow(seq, s.Begin, s.End);
		break;

	case "ToRegion1":
		for(var s in ss) cs.Show(seq, s.Begin, s.End);
		pp.SelectionToRegion(0);
		break;

	case "ToRegion2":
		for(var s in ss) cs.Show(seq, s.Begin, s.End);
		pp.SelectionToRegion(1);
		break;

	case "SeRegion1":
	case "SeRegion2":
	case "SeRegion3":
	case "SeRegion4":
		if ( ! vv.ModifierKeys.ControlPressed )	pp.ClearSelection();
		var rIdx = label.Substring(label.Length-1) - 1;
		pp.AddSelections(pp.Regions[rIdx]);
		break;

	case "Reset":
		pp.ClearSelection();
		pp.ClearItems();
		for(var i=0; i<pp.Regions.Count; i++) pp.Regions[i].Clear();
		cs.Show(seq, 0, seq.Length-1);
		break;

	case "RevSelection":
		var currentSelection = pp.SelectedSections();
		var revSelection = sa.ComplementaryIntervals(currentSelection, 0, pp.SequenceTable.Length-1);
		pp.ClearSelection();
		pp.AddSelections(revSelection);

		break;

	case "MergeSelections":
		var sList = pp.SelectedSections();
		//sList = sa.UnionIntervals(sList);

		if ( sList.Count == 0 ) {
			vv.Message("Empty Intersections.");
			vv.Return();
		}

		var maxLength = 0;
		var totalLength = 0;
		for(var i=0; i<sList.Count; i++) {
			maxLength = Math.max(maxLength, sList[i].Length);
			totalLength += sList[i].Length;
		}

		if ( ! vv.ModifierKeys.ControlPressed ) {
			var sm = New.SequenceMap(null, sList.Count, maxLength);
			for(var i=0; i<sList.Count; i++) {
				sm.SetSequence(pp.GetSequence(sList[i].Begin, sList[i].End), i*maxLength)
			}
			sm.Show();
		} else {
			var gapSize = Math.floor(0.02*totalLength/sList.Count);
			gapSize = Math.max(50, gapSize);
			totalLength += gapSize * (sList.Count - 1);
			var rows = Math.min(100, sList.Count);
			var columns = Math.floor(totalLength / rows) + 1;
			var sm = New.SequenceMap(null, rows, columns);
			var loc = 0;

			for(var i=0; i<sList.Count; i++) {
				var seq = pp.GetSequence(sList[i].Begin, sList[i].End);
				sm.SetSequence(seq, loc)
				loc += seq.Length + gapSize;
			}
			sm.Show();
		}
		break;

	case "RightAlignment":
		for(var row=0; row<pp.MapRows; row++) {
			cs.RightAlignment(seq, row*pp.MapColumns, pp.MapColumns);
		}
		break;

	case "LeftAlignment":
		for(var row=0; row<pp.MapRows; row++) {
			cs.LeftAlignment(seq, row*pp.MapColumns, pp.MapColumns);
		}
		break;

	case "SortRowsByLength":
		cs.SortRows(seq, pp.MapRows, pp.MapColumns, pp);
		break;

	case "BuildItems": // build a region section for each consecutive sequence.
		var secList = cs.BuildSections(seq);
		var bv = New.BarView();
		pp.ClearItems();
		var b = pp.BaseLocation;
		var tb = New.FreeTable(secList.Count, 4);
		pp.Regions[5].Name = "Visible sequences";
		pp.Regions[5].Clear();
		for(var i=0; i<secList.Count; i++) {
			var s = secList[i];
			var id = "s" + i;
			pp.AddItem(id, b+s.Begin, b+s.End);
			pp.Regions[5].Add(s.Begin, s.End);

			bv.ItemList.Add(New.ValueItem(id, null, s.Length));
			tb.RowSpecList[i].Id = id;

			tb.Matrix[i][0] = sa.BytesToSequence(seq, s.Begin, Math.min(40, s.End - s.Begin+1));
			tb.Matrix[i][1] = "" + (b+s.Begin);
			tb.Matrix[i][2] = "" + (b+s.End);
			tb.Matrix[i][3] = "" + s.Length;
		}
		tb.ColumnSpecList[0].Id = "MotifSeq";
		tb.ColumnSpecList[1].Id = "SeqBegin";
		tb.ColumnSpecList[2].Id = "SeqEnd";
		tb.ColumnSpecList[3].Id = "SeqLength";
		for(var col=1; col<4; col++) tb.ColumnSpecList[col].DataType = 'n';
		bv.Show();
		tb.ShowAsTable();
		break;

	case "CpGDensity":
		var pList = 
			"ACG"
			//"AAA CCC GGG TTT"
			//"A C G T"
			//"AA CC GG TT"
			//"CG GC CC GG"
			//"AT TA CG GC"
			//"AC CA AG GA AT TA CG GC CT TC GT TG CC GG AA TT"
		;
		pList = pList.Split(' ');
		var vc = New.ViewContainer();
		for(var i=0; i<pList.Length; i++) {
			var p = pList[i];
			var values = cs.FindPattern(pp.SequenceTable, p);
			var bv = New.BigBarView(values);
			bv.ShortName = p;
			bv.TheForm.FormBorderStyle=0;
			bv.BaseLocation = pp.BaseLocation - p.Length + 1;
			vc.Add(bv);
			bv.Show();
			bv.TheForm.SetBounds(0,0, vc.Width-28, 16, 12); // 12: only set the size.
			bv.TheForm.Anchor = 13; // anchor to top,left and right.
			vc.Title = (i+1) + ": " + p ;
		}
		vc.TileWindows()
		break;

	case "ExtSelection": // Extends selections on both end with 500 nb.
		var L = 500;
		var nss = New.SequenceIntervalList();
		for(var s in ss) {
			nss.Add(New.SequenceInterval(s.Begin-L, s.End+L));
		}
		pp.ClearSelection();
		pp.AddSelections(nss);
		pp.Refresh();
		break;
}

pp.Redraw();

