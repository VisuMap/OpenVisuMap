// HideSelected.js
var cs = New.CsObject("HideSection", "\
	public void Hide(byte[] seq, int idx0, int idx1) { \
		for(int i=idx0; i<=idx1; i++) { \
			if ( (i>=0) && (i<seq.Length) ) seq[i] |= 0x08; \
		} \
	} \
	public void Show(byte[] seq, int idx0, int idx1) { \
		for(int i=idx0; i<=idx1; i++) { \
			if ( (i>=0) && (i<seq.Length) ) seq[i] &= 0x77; \
		} \
	} \
	public void Revert(byte[] seq, int idx0, int idx1) { \
		for(int i=idx0; i<=idx1; i++) { \
			if ( (i>=0) && (i<seq.Length) ) { \
				seq[i] ^= 0x08; \
			} \
		} \
	}\
");

var seq = pp.SequenceTable;

if (vv.ModifierKeys.ControlPressed && false) {
	cs.Hide(seq, 0, seq.Length-1);
	for(var s in pp.SelectedSections()) {	
	   cs.Show(seq, s.Begin, s.End);
	}
} else if ( vv.ModifierKeys.ControlPressed ) {
	cs.Revert(seq, 0, seq.Length - 1);	
} else {
	for(var s in pp.SelectedSections()) {
	   cs.Hide(seq, s.Begin, s.End);
	}
}
pp.Redraw();

