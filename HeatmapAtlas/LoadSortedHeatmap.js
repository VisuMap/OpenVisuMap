//
// LoadSortedHeatmap.js
//
vv.Import("AtlasHelp.js")

function LoadSortedHeatmap() {
	var vs = New.StringSplit(vv.EventSource.Item.Name);
	var rows = vs[0] - 0;
	var rowIds = vs.GetRange(1, rows);
	var colIds = vs.GetRange(1+rows, vs.Count-1-rows);
	var nt = vv.GetNumberTable();
	nt = nt.SelectRowsById2(rowIds);
	nt = nt.SelectColumnsById2(colIds, 0);
	cfg.hm = nt.ShowHeatMap();;
}

LoadSortedHeatmap();
