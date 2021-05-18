//File: Blat.js
//vv.CurrentDirectory = vv.CurrentDataDir2 + "/ChrAll";
vv.CurrentDirectory = vv.CurrentDataDir2 + "/" + pp.SequenceName;
var fso = new ActiveXObject("Scripting.FileSystemObject");
var s = vv.GuiManager.GetClipboard();

if ( s.Length > 50 ) s = s.Substring(0, 50);
var mismatches = 2;
var minScore = s.Length - 2*(mismatches - 1);

var inFile = fso.CreateTextFile("query.fa");
inFile.WriteLine(">Q1");
inFile.WriteLine(s);
inFile.Close();

var shell = new ActiveXObject("WScript.Shell");
var cmd = "blat db.fa query.fa result.tex " 
	+ "-q=dna "
	+ "-tileSize=8 "
	+ "-oneOff=1 "
	+ "-minIdentity=80 "
	+ "-minScore=" + minScore + " "
	+ "-out=blast8 ";

var rtCode = shell.Run(cmd, 0, true);
if ( rtCode != 0 ) {
	vv.Message("blat call failed: " + rtCode);
	var errFile = fso.CreateTextFile("error.log");
	errFile.WriteLine(cmd);
	errFile.Close();
	vv.Return();
}

var outFile = fso.OpenTextFile("result.tex", 1, false);

var seqLen = s.Length;
var matchRegion = pp.Regions[4];
var matchRegionFlip = pp.Regions[5];
matchRegion.Name = "Sense Matches";
matchRegionFlip.Name = "AntiSense Matches";

matchRegion.Clear();
matchRegionFlip.Clear();
var matches = 0;
var maxLen = 0;
var chrName = pp.SequenceName;
var otherMatches = 0 ;


while(! outFile.AtEndOfStream ) {
	var line = outFile.ReadLine();
	var fs = line.Split("\t");
	
	if ( fs[1] != chrName) { otherMatches++; continue; }

	var matchLen = fs[3] - 0;
	if ( (seqLen - matchLen) <= mismatches ) {
		var seq;
		var idxBegin = fs[8] - 1;
		var idxEnd = fs[9] - 1;
		if( idxBegin>idxEnd ) {
			seq = New.SequenceInterval(idxEnd, idxBegin);
			matchRegionFlip.Add(seq.Shift(-pp.BaseLocation));
		} else {
			seq = New.SequenceInterval(idxBegin, idxEnd);
			matchRegion.Add(seq.Shift(-pp.BaseLocation));
		}
		maxLen = Math.max(maxLen, seq.Length);
		matches++;
	}
}
outFile.Close();

if ( matches == 0 ) {
	var msg = "No matching sequence found!"
	if ( otherMatches > 0 ) {
		msg += "\nBut " + otherMatches + " found in other chromosomes/groups.";
	}
	vv.Message(msg);
	pp.Title = msg;
	pp.Refresh();
	vv.Return(0);
}

pp.ClearSelection();
pp.AddSelections(matchRegion);
pp.AddSelections(matchRegionFlip);
var msg = "Matches: " + matchRegion.Count + "/" + matchRegionFlip.Count + "/" + otherMatches;
if ( ! vv.ModifierKeys.ControlPressed ) {
	pp.Title = msg;
	pp.FlushingSelection();
	vv.Return();
}
pp.FlushingSelection();

var mMap = New.SequenceMap(null, matches, maxLen);

for(var i=0; i<matchRegion.Count; i++) {
	var seq = matchRegion[i];
	mMap.SetSequence(pp.GetSequence(seq.Begin, seq.End), i*maxLen);
}

for(var i=0; i<matchRegionFlip.Count; i++) {
	var seq = matchRegionFlip[i];
	mMap.SetSequence(pp.GetSequence(seq.Begin, seq.End), (i+matchRegion.Count)*maxLen);
}

var sa = vv.FindPluginObject("SeqAnalysis");
for(var i=0; i<matchRegionFlip.Count; i++) {
	var seq = matchRegionFlip[i];
	var i0 = (i+matchRegion.Count)*maxLen;
	sa.Flip(mMap.SequenceTable, i0, i0+maxLen-1);
	mMap.Regions[0].Add(New.SequenceInterval(i0, i0+maxLen-1))
}
mMap.ReadOnly = true;
mMap.Regions[0].RegionStyle = 3;
mMap.Regions[0].Color = New.Color("Green");
mMap.Title = msg;
mMap.Show();

