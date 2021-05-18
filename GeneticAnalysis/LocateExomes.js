// LocateExomes.js
var ds = vv.Dataset;
var ss = pp.SelectedSections();
var sBegin, sEnd;
var s = ( ss.Count >= 1 ) ? ss[0] 	: New.SequenceInterval(0, pp.SequenceTable.Length - 1);
s = s.Shift( pp.BaseLocation );
var R = New.SequenceIntervalList();


for(var row=0; row<ds.Rows; row++) {
	var iBegin = New.IntArray(ds.GetDataAt(row, 3));
	var iEnd = New.IntArray(ds.GetDataAt(row, 4));
	for(var i=0; i<iBegin.Count; i++) {
		var ex = s.Intersect(iBegin[i], iEnd[i]);
		if (ex.Length > 0 ) R.Add(ex.Shift(-pp.BaseLocation));
	}
}

R = pp.UnionIntervals(R);

pp.ClearSelection(); pp.SetSelections(R);
//pp.Region1.AddRange(R);


pp.Redraw();
pp.Title = "Exomes: " + R.Count;


