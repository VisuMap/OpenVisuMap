// File: ApiTest.js
//MenuLabels GetModelInfo TrainModel ApplyModel Interpolate Interpolate2 Interpolate3 Interpolate100
//=========================================================

var ms = vv.FindPluginObject("DMScript");
var nt = vv.GetSelectedNumberTable();
if ( nt.Rows * nt.Columns == 0 ) nt = vv.GetNumberTable();


var mdName = ms.GetDefaultModelName();
if ( mdName == null ) mdName = "ApiTestModel";

var targetDim = vv.Map.Dimension;
var maxEpochs = 2000;
var logLevel = 1;
var refreshFreq = 500;

eval(vv.EventSource.Item+"()");

//===========================================================

function GetModelInfo() {
	var modelInfo = ms.GetModelInfo(mdName);
	var lnk = modelInfo.Link;
	vv.Message(mdName + ": " + lnk.InputVariables.Count + " => " + lnk.OutputVariables);
}

function TrainModel() {
	var bodyList = vv.Map.SelectedBodies;
	if ( bodyList.Count == 0 )bodyList = vv.Dataset.BodyList;
	ms.TrainModel(mdName, nt, targetDim, bodyList, vv.Map, maxEpochs, logLevel, refreshFreq);
}

function ApplyModel() {
	var bList = ms.ApplyModel2(mdName, nt);
	New.MapSnapshot2(bList, vv.Map).Show().ResetSize();
}

//==================================================================

function SetUniqueId(bList) {
	var s = "ABCEDEFGHIJKLMNLOPQUabcedfg";
	var prefix = s[(new Date()).getMilliseconds() % s.Length] + "";
	for(var i=0; i<bList.Count; i++) 
		bList[i].Id = prefix + i;
	return bList;
}

function InterpolatePair(bStart, bEnd) {
	var L = Math.min(bStart.Count, bEnd.Count);
	if ( L <= 0 ) {
	    vv.Message("Please select some start and end points!");
	    return null;
	}

	var N = 1000;
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
	for(var col=0; col<nt.Columns; col++)
		ntNew.ColumnSpecList[col].CopyFrom(tb.ColumnSpecList[col]);
	return ntNew;
}

function InterpolateCall(ntNew) {
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

//==================================================================

function Interpolate() {
	RemoveExisting();
	var bStart = pp.BodyListHighlighted();
	var bEnd = pp.GetSelectedBodies();
	InterpolateCall(InterpolatePair(bStart, bEnd));
}

function Interpolate100() {
	RemoveExisting();

	var bs = pp.BodyList;
	var bStart = New.BodyList();
	var bEnd = New.BodyList();

	for(var i=0; i<100; i++) {
		bStart.Add(bs[parseInt( Math.random()*bs.Count)])
		bEnd.Add(bs[parseInt( Math.random()*bs.Count)])
	}

	InterpolateCall(InterpolatePair(bStart, bEnd));

	vv.CurrentDirectory = vv.CurrentScriptDirectory;
	vv.StartProcess("cmd.exe", "/K python curvature.py", false)
}

function GetPair(g1, g2) {
	var gs = vv.Folder.LabelGroupList;
	var ds = vv.Dataset;
	return InterpolatePair(
		ds.BodyListForId(gs.GetGroupLabels(g1)),
		ds.BodyListForId(gs.GetGroupLabels(g2))
	);
}

function Interpolate3() {
	var p = New.NumberTable(nt.ColumnSpecList);

	p.Append(GetPair("ClusterA", "ClusterB"));
	p.Append(GetPair("ClusterA", "ClusterC"));
	p.Append(GetPair("ClusterA", "ClusterD"));

/*
	p.Append(GetPair("G1", "G2"));
	p.Append(GetPair("G2", "G3"));
	p.Append(GetPair("G3", "G4"));
	p.Append(GetPair("G4", "G5"));
	p.Append(GetPair("G5", "G1"));
*/

	InterpolateCall(p);
}

function Interpolate2() {
	var bs = pp.GetSelectedBodies();
	if ( bs.Count != 2 ) {
	    vv.Message("Please selected 2 data points");
	    return;
	}
	InterpolateCall(InterpolatePair(New.BodyList(bs[0]), New.BodyList(bs[1])));
}
