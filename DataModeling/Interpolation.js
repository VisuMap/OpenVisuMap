// File: Interpolation.js
// Usage: 1. Train the default model with "Shape" or "Shape&Color" as target.
//        2. Open a MapSnapshot or "3D Animation" view. 
//        3. Highlight some data points as starting points of interpolation;
//        4. Select some data points as ending points of the interpolation;
//        5. Run this script from the context menu. 
//=========================================================
var N = 1000;   // The number of interpolation points.

function InterpolatePair(bStart, bEnd) {
	var L = Math.min(bStart.Count, bEnd.Count);
	if ( L <= 0 ) {
	    vv.Message("Please select some start and end points!");
	    vv.Return();
	}
	var tb = vv.GetNumberTable();
	var ntNew = New.NumberTable(N*L, tb.Columns);
	for(var p = 0; p<L; p++) {
		var m0 = tb.Matrix[tb.IndexOfRow(bStart[p].Id)];
		var m1 = tb.Matrix[tb.IndexOfRow(bEnd[p].Id)];
		for(var row=0; row<N; row++) {
			var f = (row + 1.0) / (N+1);
			var g = 1.0 - f;
			var m = ntNew.Matrix[p * N + row];
			for(var col=0; col<tb.Columns; col++) 
				m[col] = g*m0[col] + f * m1[col];
			var rs = ntNew.RowSpecList[p * N + row];
			rs.Type = 24+p;
			rs.Name = "xx";
		}
	}
	for(var col=0; col<tb.Columns; col++)
		ntNew.ColumnSpecList[col].CopyFrom(tb.ColumnSpecList[col]);
	return ntNew;
}

function SetUniqueId(bList) {
	var s = "ABCEDEFGHIJKLMNLOPQUabcedfg";
	var prefix = s[(new Date()).getMilliseconds() % s.Length] + "";
	for(var i=0; i<bList.Count; i++) 
		bList[i].Id = prefix + i;
	return bList;
}

function InterpolateCall(ntNew) {
	var ms = vv.FindPluginObject("DMScript");
	var mdName = ms.GetDefaultModelName();
	var bs = ms.ApplyModel2(mdName, ntNew, null);
	pp.BodyList.AddRange(SetUniqueId(bs));
	pp.RedrawAll();
}

function RemoveExisting() {
	var org = New.BodyList();
	for(var b in pp.BodyList)
		if ( b.Name != "xx" )
			org.Add(b);
	pp.BodyList.Clear();
	pp.BodyList.AddRange(org);
	pp.RedrawAll();
}

RemoveExisting();
var bStart = pp.BodyListHighlighted();
var bEnd = pp.GetSelectedBodies();
var ntNew = InterpolatePair(bStart, bEnd);
InterpolateCall(ntNew);
