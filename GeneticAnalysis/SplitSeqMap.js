// SplitViews.js
//

var cs = New.CsObject("\
	public void ReverseStrand(byte[] seq) {\
		for(int i=0; i<seq.Length; i++)\
			if ( seq[i] < 4 ) seq[i] ^= 0x3;\
	}\
");

var seq = pp.SequenceTable.Clone();

var sm = New.SequenceMap(seq, pp.MapRows, pp.MapColumns);
sm.Regions[0].CopyFrom(pp.Regions[0]);
sm.Regions[1].CopyFrom(pp.Regions[1]);
sm.Show();

seq = seq.Clone();
cs.ReverseStrand(seq);

sm = New.SequenceMap(seq, pp.MapRows, pp.MapColumns);
sm.Regions[2].CopyFrom(pp.Regions[2]);
sm.Regions[3].CopyFrom(pp.Regions[3]);
sm.Show();

