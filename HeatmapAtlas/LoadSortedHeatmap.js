//
// LoadSortedHeatmap.js
//
vv.Import("AtlasHelp.js")

function LoadSortedHeatmap() {
   //var t0 = (new Date()).getTime();
	var tbItem = vv.EventSource.Item;
	var vs = New.StringSplit(tbItem.Tag);
	var fs = New.StringSplit(vs[0], '&');

	var dsName = "";
	var rows = 0;
	if ( fs.Count == 2 ){
		dsName = fs[0];
		rows = fs[1] - 0;
	} else {
		dsName = vv.Dataset.Name;
		rows = fs[0] - 0;
	}

   var columns = vs.Count-1-rows;
	var rowIds = vs.GetRange(1, rows);	
	var colIds = vs.GetRange(1 + rows, columns);
	if ( dsName != vv.Dataset.Name )
		vv.Folder.OpenDataset(dsName);
	var nt = vv.GetNumberTableView(false);
	nt = nt.SelectRowsById2(rowIds);
	nt = nt.SelectColumnsById2(colIds, 0);
	cfg.hm = nt.ShowHeatMap();

   //var t1 = (new Date()).getTime();
   //cfg.hm.Title = "Time: " + (t1 - t0)/1000;
}



LoadSortedHeatmap();

