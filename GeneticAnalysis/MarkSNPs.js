// MarkSNPs.js
var sa = vv.FindPluginObject("SeqAnalysis");
var blobs = vv.Folder.GetBlobList();
var sv = sa.OpenSequence(blobs[0]);
var rows = 50;
var columns = parseInt(sv.Length / rows, 10) + ( (sv.Length%rows > 0) ? 1 : 0 );
var seqTable = New.ByteArray(rows * columns, 4);

var cs = New.CsObject("SetSNPList", "\
	public void SetSNPs(byte[] seq, INumberTable nt) { \
		for(int i=0; i<seq.Length; i++) seq[i] |= 0x08; \
		for(int row=0; row<nt.Rows; row++) { \
			int rIdx = (int)(nt.Matrix[row][0]); \
			seq[rIdx] &= 0xF7; \
		} \
	} \
");

sv.FetchSeqIndex(0, sv.Length, seqTable, 0);   

cs.SetSNPs(seqTable, vv.GetNumberTable());

New.SequenceMap(seqTable, rows, columns).Show();
seqTable = null;
