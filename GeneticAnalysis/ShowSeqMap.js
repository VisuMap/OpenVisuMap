// File: ShowSeqMap.js
//
// Display selected DNA sequences in a sequence map.
//
// Usage: In order to use this service the current data set must be a table
// of DNA sequences with seqence index and length in the first and second column.
// To call this service, first select a set of sequences in the main map, then call
// this script from the context menu.
//
// Each selected sequence will become a named item in the sequence map.
// When the control-key was pressed, the sequences will be packed in a compact way.
//
//================================================================================
//

if ( pp.SelectedItems.Count == 0 ) {
	vv.Message("No sequence selected");
	vv.Return(0);
}

var ds = vv.Dataset;

var strandColumn = ds.IndexOfColumn("Strand");
function FlipStrand(rowNr) {
	return (strandColumn >= 0) && (ds.GetDataAt(rowNr, strandColumn) == "-1");
}
var cs = New.CsObject("\
	public void FlipSeq(byte[] s, int idx0, int idx1) {\
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
		for(int i=idx0; i<=idx1; i++) {\
			if ( s[i] < 4 ) s[i] ^= 0x3;\
		}\
	}\
");


var maxLen = 0;
for(var id in pp.SelectedItems) {
	var row = ds.IndexOfRow(id);
	maxLen = Math.max(maxLen, ds.GetDataAt(row, 1)-0);
}

var MAX_LEN = vv.ModifierKeys.ControlPressed ? 0 : 20000; 
var maxLen = Math.min(maxLen, MAX_LEN);

var blobName = ds.ColumnSpecList[0].Name;
var sm = vv.FindPluginObject("SeqAnalysis").OpenSequence(blobName);
var sBegin = New.IntArray();
var sEnd = New.IntArray();

var UTR5 = New.SequenceIntervalList();
var UTR3 = New.SequenceIntervalList();

var sMap;
if ( MAX_LEN == 0 ) {
	var rows = 40;
	var size = 0;
	for(var id in pp.SelectedItems)size +=ds.GetDataAt(ds.IndexOfRow(id), 1) - 0;
	var gap = Math.round(size/(rows*150));
	size += gap*pp.SelectedItems.Count;
	var seqTable = New.ByteArray(size, 4);
	var dstIdx = 0;
	for(var id in pp.SelectedItems) {
		var row = ds.IndexOfRow(id);
		var idx=ds.GetDataAt(row, 0) - 0;
		var len=ds.GetDataAt(row, 1) - 0;
		sm.FetchSeqIndex(idx, len, seqTable, dstIdx);
		if ( FlipStrand(row) ) cs.FlipSeq(seqTable, dstIdx, dstIdx+len-1);
		sBegin.Add(dstIdx); 
		sEnd.Add(dstIdx+len-1);
		MarkUTR(row, dstIdx);
		dstIdx += len + gap;
	}
	var columns = Math.ceil(seqTable.Length / rows);
	sMap = New.SequenceMap(seqTable, rows, columns);
} else {
	var rows = pp.SelectedItems.Count;
	var seqTable = New.ByteArray(rows * maxLen, 4);
	var seqRow = 0;
	for(var id in pp.SelectedItems) {
		var row = ds.IndexOfRow(id);
		var idx=ds.GetDataAt(row, 0) - 0;
		var len=ds.GetDataAt(row, 1) - 0;
		len = Math.min(len, maxLen);
		sm.FetchSeqIndex(idx, len, seqTable, seqRow*maxLen);
		if ( FlipStrand( row ) ) cs.FlipSeq(seqTable, seqRow*maxLen, seqRow*maxLen+len-1);
		sBegin.Add(seqRow*maxLen); sEnd.Add(seqRow*maxLen+len-1);
		seqRow++;
	}
	sMap = New.SequenceMap(seqTable, rows, maxLen);
}

sMap.Regions[0].AddRange(UTR5);
sMap.Regions[1].AddRange(UTR3);

sMap.Title = blobName;
sMap.Show();

for(var k=0; k<sBegin.Count; k++) sMap.AddItem(pp.SelectedItems[k], sBegin[k], sEnd[k]);

seqTable = null;

//===========================================================================
function MarkUTR(rowIdx, dstBegin) {
	if ( ds.Columns < 10 ) return;

	var baseLoc = ds.GetDataAt(rowIdx, 5) - 0;

	if ( ds.GetDataAt(rowIdx, 6) != "0" ) {
		var startList = New.IntArray(ds.GetDataAt(rowIdx, 6));
		var endList =  New.IntArray(ds.GetDataAt(rowIdx, 7));

		for(var i=0; i<startList.Count; i++) {
			var iStart = startList[i];
			var iEnd = endList[i];
			if ( iStart >= iEnd ) {
				var tmp = iStart;
				iStart = iEnd;
				iEnd = tmp;
			}
			UTR5.Add( New.SequenceInterval(iStart + dstBegin - baseLoc, iEnd + dstBegin - baseLoc) );
		}
	}

	if ( ds.GetDataAt(rowIdx, 8) != "0" ) {
		var startList = New.IntArray(ds.GetDataAt(rowIdx, 8));
		var endList =  New.IntArray(ds.GetDataAt(rowIdx, 9));

		for(var i=0; i<startList.Count; i++) {
			var iStart = startList[i];
			var iEnd = endList[i];
			if ( iStart >= iEnd ) {
				var tmp = iStart;
				iStart = iEnd;
				iEnd = tmp;
			}
			UTR3.Add( New.SequenceInterval(iStart + dstBegin - baseLoc, iEnd + dstBegin - baseLoc) );
		}
	}

}

function AssemblySeq(row, seqTable, idx0) {
	var begin = New.IntArray(ds.GetDataAt(row, 3));
     var end = New.IntArray(ds.GetDataAt(row, 4));
	begin.Sort();
	end.Sort();
	for(var i=0; i<begin.Count; i++) {
		begin[i]--;
		end[i]--;
	}
	idx0 = 0;
	var msg = "";
	for(var i=0; i<begin.Count; i++) {
		var len = end[i]-begin[i];
		// vv.Message(i+":" + idx0 + ":" + maxLen + ":" + len);
		//sm.FetchSeqIndex(begin[i], len, seqTable, idx0);
		idx0 += len;
		msg += begin[i] + " - " + end[i] + "\n";
	}
	vv.Message("N:" + idx0 + "\n" + msg);
}
