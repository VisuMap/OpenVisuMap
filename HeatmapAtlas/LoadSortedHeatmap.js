//
// LoadSortedHeatmap.js
//
vv.Import("AtlasHelp.js")

function LoadSortedHeatmap() {
	var tbItem = vv.EventSource.Item;
	var vs = New.StringSplit(tbItem.Name);
	var dsName = vs[0];
	var rows = vs[1] - 0;
   var columns = vs.Count-2-rows;
	var rowIds = vs.GetRange(2, rows);	
	var colIds = vs.GetRange(2 + rows, columns);
	if ( dsName != vv.Dataset.Name )
		vv.Folder.OpenDataset(dsName);
	var nt = vv.GetNumberTable();
	nt = nt.SelectRowsById2(rowIds);
	nt = nt.SelectColumnsById2(colIds, 0);
	cfg.hm = nt.ShowHeatMap();;
}

LoadSortedHeatmap();
