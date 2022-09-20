// SaveSortedHeatmap.js
// Save a sorted heatmap into the current Atlas as a label item.
// --------------------------------------------------------------
// 
function SaveSortedTable() {
	var nt = pp.GetNumberTable();
	var info = [];	
	info.push(nt.Rows.toString());	
	for(var rs of nt.RowSpecList) info.push(rs.Id);
	for(var cs of nt.ColumnSpecList) info.push(cs.Id);

	var at = New.Atlas().Show();
	var ii = at.NewHeatMapItem(New.HeatMap(New.NumberTable(1,1)));
	ii.Name = info.join('|');
	if ( ii.Id.length > 1 ) {
		var idx = ii.Id.substr(1) - 0;
		ii.Top += 30*idx;
		ii.Left+= 20*idx;
	}
	ii.IconHeight = ii.IconWidth = 40;
	ii.Script = `!
		var vs = New.StringSplit(vv.EventSource.Item.Name);
		var rows = vs[0] - 0;
		var rowIds = vs.GetRange(1, rows);
		var colIds = vs.GetRange(1+rows, vs.Count-1-rows);
		var nt = vv.GetNumberTable();
		nt = nt.SelectRowsById2(rowIds);
		nt = nt.SelectColumnsById2(colIds, 0);
		nt.ShowHeatMap();`;
	at.Close();
}

SaveSortedTable();

