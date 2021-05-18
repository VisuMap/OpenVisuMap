//File: WordFrequency.js
//
// Description: Count word frequencies in selected text fields and convert
// the data to sparse set coding. The results will be shown in two table windows.
// 
// Usage: Open  a Details window, select the columns of texts; then run this script
//      
var tb = pp.GetSelectedRegion();
var cnt = New.StringDictionary();

for(var row=0; row<tb.Rows; row++) 
for(var col=0; col<tb.Columns; col++) {
	var wList = tb.Matrix[row][col].split(" ");
	for(var k in wList) {
		var w = wList[k];
		if ( w == "" ) continue;
		cnt[w] = cnt.ContainsKey(w) ? (cnt[w] + 1) : 1;			
	}
}

var wList = new Array();
for(var w in cnt.Keys) wList.push(w);
wList.sort(function(w1, w2) { return cnt[w2]-cnt[w1]; });

var wt = New.FreeTable(wList.length, 2);
for(var row=0; row<wList.length; row++) {
	wt.Matrix[row][0] = wList[row];
	wt.Matrix[row][1] = "" + cnt[wList[row]];
}
wt.ColumnSpecList[1].DataType='n';
wt.ShowAsTable();

var word2Idx = New.StringDictionary();
for(var i=0; i<wList.length; i++) word2Idx[wList[i]] = i;

var setTable = New.FreeTable(tb.Rows, tb.Columns);
for(var row=0; row<tb.Rows; row++) 
for(var col=0; col<tb.Columns; col++) {
	var wIdx = new Array();
	var ws = tb.Matrix[row][col].split(" ");
	for(var k in ws) {
		var w = ws[k];
		if ( w == "" ) continue;
		wIdx.push(word2Idx[ws[k]]);
	}
	wIdx.sort(function(a, b) { return a-b; });
	setTable.Matrix[row][col] = "" + wIdx;
	setTable.RowSpecList[row].CopyFrom(tb.RowSpecList[row]);
}

for(var col=0; col<tb.Columns; col++) setTable.ColumnSpecList[col].Name = "SparseSet";

setTable.ShowAsTable();

