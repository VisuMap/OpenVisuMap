// ShowMeanVector.js
//
// Show the mean vector of selected rows or columns.
//
var bv = New.BarView(pp.GetSelectedNumberTable());
bv.Horizontal = false;
bv.AggregationMethod = 0;
bv.AggregationOrientation = 0;
bv.Show();
bv.Tag = pp;

function UpdateMeanView() {
    var nt =  pp.Tag.GetSelectedNumberTable();
    if ( (nt.Rows * nt.Columns == 0)  || (nt.Columns != pp.ItemList.Count) )
		return;
    var dt = nt.ColumnMean();
	 pp.ItemList.Clear();
    pp.ItemList.AddRange(dt);
    pp.Redraw();
    pp.Title = "Selected rows: " + nt.Rows;
}

vv.EventManager.OnItemsSelected("!UpdateMeanView()", bv);
