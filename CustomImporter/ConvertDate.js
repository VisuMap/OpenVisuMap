var dt = vv.Dataset;
var dColumn = dt.IndexOfColumn(vv.SelectedItems[0]);
dt.AddColumn("DateInHours", 1, "0", dt.Columns);

var minValue = 1e64;
for(var row=0; row<dt.Rows; row++) {
  var s = dt.GetDataAt(row, dColumn);
  var d = (new Date(s)).getTime()/1000.0/3600.0;
  minValue = Math.min(minValue, d);
}

for(var row=0; row<dt.Rows; row++) {
  var s = dt.GetDataAt(row, dColumn);
  var d = (new Date(s)).getTime()/1000.0/3600.0;
  dt.SetDataAt(row, dt.Columns - 1, d - minValue);
}


