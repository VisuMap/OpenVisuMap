// SeparateRows.js
var R = pp.Regions[4];
R.Color = New.Color("White");
R.RegionStyle = 4;
R.Add(New.SequenceInterval(0, pp.SequenceTable.Length-1));
pp.Redraw();
