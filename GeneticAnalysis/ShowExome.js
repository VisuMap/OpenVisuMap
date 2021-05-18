// ShowExome.js
//!import "c:/work/VisuMap/PluginModules/GeneticAnalysis/SeqUtil.js"

var ds = vv.Dataset;
var sa = vv.FindPluginObject("SeqAnalysis");

var blobList = vv.Folder.GetBlobList();
var sm = sa.OpenSequence(blobList[0]);

var iBegin = New.IntArray();
var iEnd = New.IntArray();
var iTrans = New.IntArray();  // transcript indexes.
var merging = vv.ModifierKeys.ControlPressed;

for(var id in vv.SelectedItems) {
	var rowIdx = ds.IndexOfRow(id);
	var exIdx = vv.SelectedItems.IndexOf(id);
	for(var n in New.IntArray(ds.GetDataAt(rowIdx, 3)))
		iBegin.Add(n - 1);
	for(var n in New.IntArray(ds.GetDataAt(rowIdx, 4))){
		iEnd.Add(n - 1);
		iTrans.Add(exIdx);
	}
}	

var minIdx = 1000 * 1000000;
var maxIdx = -1000;
for(var n=0; n<iBegin.Count; n++) {
	minIdx = Math.min(minIdx, iBegin[n]);
	maxIdx = Math.max(maxIdx, iBegin[n]);
	minIdx = Math.min(minIdx, iEnd[n]);
	maxIdx = Math.max(maxIdx, iEnd[n]);
}

var columns = maxIdx - minIdx + 1;
var rows = vv.SelectedItems.Count;

if ( merging ) { 
	// mergin all rows to a single row and fold it 
	// to able table with maximal of 500K columns
	var newColumns = Math.min(columns, 50000);
	rows = parseInt(columns / newColumns, 10) + 1;
	columns = newColumns;
}

if ( rows*columns > 2000*1000*1000 ) {
	vv.Message("Range too big: " + (rows*columns).ToString("### ### ###"));
	vv.Return(0);
}

var seqTable = New.ByteArray(rows*columns, 4);

for(var exIdx=0; exIdx<iBegin.Count; exIdx++) {
	var c0 = iBegin[exIdx];
	var c1 = iEnd[exIdx];
	var seqIdx = c0 - minIdx;

	if ( merging ) {
		sm.FetchSeqIndex(c0, c1-c0+1, seqTable, seqIdx);
	} else {
		sm.FetchSeqIndex(c0, c1-c0+1, seqTable, iTrans[exIdx]*columns + seqIdx);
	}
}

var span = maxIdx-minIdx;
var title = vv.SelectedItems.Count +" transcripts, " + iBegin.Count 
	+ " exomes, " + span.ToString("# ### ###") + " bp: ";
span = parseInt(span/100000, 10);
for(var i=0; i<span; i++) {
	title += "▲";
	if ( (i+1)%10 == 0 ) title += "   ";
}

var hm = New.SequenceMap(seqTable, rows, columns)
hm.BaseLocation = minIdx;
hm.Title = title;
hm.Show();
