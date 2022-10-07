//
// LoadSortedHeatmap.js
//
vv.Import("AtlasHelp.js")

function LoadSortedHeatmap() {
   //var t0 = (new Date()).getTime();
	var tbItem = vv.EventSource.Item;
	var vs = New.StringSplit(tbItem.Tag);

	var fs = New.StringSplit(vs[0], '&');
	if ( fs.Count == 1 ) {
		var dsName = vv.Dataset.Name;
		var rows = fs[0] - 0;
	}
   if ( fs.Count >= 2 ){
		var dsName = fs[0];
		var rows = fs[1] - 0;
	}

   var columns = vs.Count-1-rows;
	var rowIds = vs.GetRange(1, rows);	
	var colIds = vs.GetRange(1 + rows, columns);
	if ( dsName != vv.Dataset.Name )
		vv.Folder.OpenDataset(dsName);
	var nt = vv.GetNumberTableView(false);
	nt = nt.SelectRowsById2(rowIds);
	nt = nt.SelectColumnsById2(colIds, 0);
	cfg.hm = New.HeatMap(nt);
	cfg.hm.SelectionMode=2;
	if ( fs.Count >= 4 ) {		
		cfg.hm.Width = fs[2] - 0;
		cfg.hm.Height = fs[3] - 0;
	}
	cfg.hm.Show2();

   //var t1 = (new Date()).getTime();
   //cfg.hm.Title = "Time: " + (t1 - t0)/1000;
}




LoadSortedHeatmap();

