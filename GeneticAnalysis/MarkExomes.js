// MarkExomes.js
// Show exomes of selected genes.
vv.Import("GaHelp.js")

var ds = vv.Dataset;
var sa = vv.FindPluginObject("SeqAnalysis");
var blobList = vv.Folder.GetBlobList();
var iBegin = New.IntArray();
var iEnd = New.IntArray();

for(var id of vv.SelectedItems) {
	var rowIdx = ds.IndexOfRow(id);
	for(var n of New.IntArray(ds.GetDataAt(rowIdx, 3))) 
		iBegin.Add(n - 1);
	for(var n of New.IntArray(ds.GetDataAt(rowIdx, 4))) 
		iEnd.Add(n - 1);
}

var hmList = vv.FindFormList("SequenceMap");
var hm = ( hmList.Count > 0 ) ? hmList[0] : OpenSequenceMap(sa, ds);
	
	
for(var k=0; k<3; k++) {
	hm.ClearSelection();
	hm.Refresh(); vv.Sleep(200);
	hm.AddSelections(iBegin, iEnd);
	hm.Refresh(); vv.Sleep(200);
}
