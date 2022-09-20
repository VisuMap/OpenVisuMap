// SaveSortedHeatmap.js
// Save a sorted heatmap into the current Atlas as a label item.
// --------------------------------------------------------------
// 
function SaveSortedTable() {
	var rowIds = New.StringArray();
	var colIds = New.StringArray();
	var nt = pp.GetNumberTable();
	for(var rs of nt.RowSpecList)
	  rowIds.Add(rs.Id);
	for(var cs of nt.ColumnSpecList)
	  colIds.Add(cs.Id);

	var at = New.Atlas();
	at.Show();
	var ti = at.NewLabelItem();
	var myId = ti.Id;
	if (myId.length>1) {
		var idx = myId.substr(1) - 0;
		ti.Top += 15*idx;
	}
	ti.Text = "Sorted Table " + myId;
	var lgl = vv.Folder.LabelGroupList;
   lgl.SetGroupLabels("SRows_"+myId, rowIds);
	lgl.SetGroupLabels("SColumns_"+myId, colIds);
	ti.Script = `!
var myId = vv.EventSource.Item.Id;
var rowIds = vv.Folder.LabelGroupList.GetGroupLabels("SRows_"+myId);
var colIds = vv.Folder.LabelGroupList.GetGroupLabels("SColumns_"+myId);
nt = vv.GetNumberTable();
nt = nt.SelectRowsById2(rowIds);
nt = nt.SelectColumnsById2(colIds, 0);
nt.ShowHeatMap();`;
	at.Close();
}

SaveSortedTable();








