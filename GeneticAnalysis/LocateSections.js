// LocateSections.js
var sa = vv.FindPluginObject("SeqAnalysis");
var ds = vv.Dataset;
var gm = vv.FindFormList("SequenceMap")[0];

var R = gm.Region2;
R.Clear();

if ( pp.Name == "DataDetails" ) {
	var nt = pp.GetSelectedNumberTable();
	for(var row=0; row<nt.Rows; row++) {
		var Row = nt.Matrix[row]
		R.Add(New.SequenceInterval(Row[0], Row[1]));
		gm.AddItem(nt.RowSpecList[row].Id, Row[0], Row[1]);
	}
} else {
	for(var id in vv.SelectedItems) {
		var rowIdx = ds.IndexOfRow(id);
		var begin = ds.GetDataAt(rowIdx, 0) - 0;
		var length = ds.GetDataAt(rowIdx, 1) - 0;
		R.Add(New.SequenceInterval(begin, begin+length));
	}
}
gm.Redraw();

gm.Title = "Sections: " + R.Count;
