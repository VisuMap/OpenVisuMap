// File: MarkIntegerColumns.js
//
// Description: Find columns with integer values and mark them
// with the column group index 15.
//
// Usage: Load a dataset into VisuMap; then run this script. 
// 
// Notice: Integer attributes are often just identifies or flags.
// Those columns should be changed to Enumerate type for visualization purpose.
// To do so, we first open the table editor in attribute mode, then run this script; 
// Table editor will automatically highlight all integer columns as selected. We can then 
// change all those columns to enumerate column through the context menu "Change Column Types to".
//
// The heatmap view can be used to view column groups.
//
var nt = vv.GetNumberTable();
var ds = vv.Dataset;
var bodies = ds.BodyList;
var intColumns = New.StringArray();
for(var col=0; col<nt.Columns; col++) {
  var row;
  for(row=0; row<nt.Rows; row++) {
    var v = nt.Matrix[row][col];
    if ( v - Math.ceil(v) != 0 ) {
      break;
    }
  }
  if ( row == nt.Rows ) {
    intColumns.Add( nt.ColumnSpecList[col].Id );
  }
}

for(var id in intColumns) {
  ds.ColumnSpecList[ds.IndexOfColumn(id)].Group = 15;
}

ds.CommitChanges();
vv.EventManager.RaiseItemsSelected(intColumns);




