// HideSelected.js
var cs = New.CsObject("RandomizeSeq", "\
	public void Randomize(byte[] s, int idx0, int idx1) { \
		var rg = new Random(321); \
		for(int i=idx0; i<=idx1; i++) { \
			if ( s[i] == 4 ) continue; \
			int j = 0;  \
			int n = 0; \
			for(n=0; n<20; n++) { \
				j = idx0 + rg.Next(idx1 + 1 -idx0); \
				if ( s[j] != 4 ) break; \
			} \
			if ( n < 20 ) { \
				byte tmp = s[j]; \
				s[j] = s[i]; \
				s[i] = tmp; \
			} \
		} \
	} \
\
	public void Reverse(byte[] s, int idx0, int idx1) { \
		int N = idx1 - idx0 + 1; \
		int N2 = N/2; \
		for(int i=0; i<N2; i++) { \
			int j = N - 1 - i; \
			byte tmp = s[idx0+i]; \
			s[idx0+i] = s[idx0+j]; \
			s[idx0+j] = tmp; \
		} \
	} \
\
	public void Flip(byte[] s, int idx0, int idx1) { \
		Reverse(s, idx0, idx1); \
		for(int i=idx0; i<=idx1; i++) { \
			if ( s[i] < 4 ) s[i] ^= 0x3; \
		} \
	} \
\
	public int CpGCount(byte[] s, int idx0, int idx1) { \
		int cnt = 0 ; \
		for(int i=idx0; i<=(idx1-1); i++) { \
			if ((s[i]==1) && (s[i+1]==2)) { \
				cnt++;\
				i++; \
			} \
		} \
		return cnt; \
	} \
\
	public int atCount(byte[] s, int idx0, int idx1) { \
		int cnt = 0 ; \
		for(int i=idx0; i<=(idx1-1); i++) { \
			if ((s[i]==0) && (s[i+1]==3)) { \
				cnt++;\
				i++; \
			} \
		} \
		return cnt; \
	} \
\
	public void Hide(byte[] seq, int idx0, int idx1) { \
		for(int i=idx0; i<=idx1; i++) { \
			if ( (i>=0) && (i<seq.Length) ) seq[i] |= 0x08; \
		} \
	} \
\
	public void Show(byte[] seq, int idx0, int idx1) { \
		for(int i=idx0; i<=idx1; i++) { \
			if ( (i>=0) && (i<seq.Length) ) seq[i] &= 0x77; \
		} \
	} \
\
	public void Revert(byte[] seq, int idx0, int idx1) { \
		for(int i=idx0; i<=idx1; i++) { \
			if ( (i>=0) && (i<seq.Length) ) { \
				seq[i] ^= 0x08; \
			} \
		} \
	}\
");

var seq = pp.SequenceTable;
var label = vv.EventSource.Item;
var ss = pp.SelectedSections();
var s = ( ss.Count >= 1 ) ? ss[0] : New.SequenceInterval(0,seq.Length-1);

//MenuLabels Randomize Reverse Flip CpGRatio atRatio Hide Show Revert ToRegion1 ToRegion2 SelectRegion1 SelectRegion2
switch( label ) {
	case "Randomize":
		cs.Randomize(seq, s.Begin, s.End);
		break;

	case "Reverse":
		cs.Reverse(seq, s.Begin, s.End);
		break;

	case "Flip":
		cs.Flip(seq, s.Begin, s.End);
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
		for(var s in ss)  cs.Hide(seq, s.Begin, s.End);		
		break;

	case "Show":
		cs.Hide(seq, 0, seq.Length-1);
		for(var s in ss) cs.Show(seq, s.Begin, s.End);		
		break;

	case "Revert":
		cs.Revert(seq, s.Begin, s.End);
		break;

	case "ToRegion1":
		pp.Region1.Clear();
		pp.SelectionToRegion(0);
		break;

	case "ToRegion2":
		pp.Region2.Clear();
		pp.SelectionToRegion(1);
		break;

	case "SelectRegion1":
		pp.SetSelections(pp.Region1);
		break;

	case "SelectRegion2":
		pp.SetSelections(pp.Region2);
		break;
}


pp.Redraw();

