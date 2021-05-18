// File: Blast.js
// Search nucleotide sequence in a sequence map. The sequence must be stored
// in the Windows clipboard.
// 
// Notice: the blastn.exe must be installed in the Windows PATH environment variable.

vv.CurrentDirectory = vv.CurrentDataDir2 + "/" + pp.SequenceName;
//vv.CurrentDirectory = vv.CurrentDataDir2 + "/ChrAll";

var fso = new ActiveXObject("Scripting.FileSystemObject");
var s = vv.GuiManager.GetClipboard();

if ( s.Length > 50 ) s = s.Substring(0, 50);

var inFile = fso.CreateTextFile("query.fa");
inFile.WriteLine(">QuerySeq");
inFile.WriteLine(s);
inFile.Close();

var shell = new ActiveXObject("WScript.Shell");
var cmd = "blastn -db db.fa -query query.fa -out result.txt " 
	+ "-reward 1 -penalty -3 "
	+ "-gapopen 5 -gapextend 2 "
	+ "-task " + ((s.Length>=50) ? "blastn " : "blastn-short ")
	+ "-word_size " + ((s.Length<25) ? "7 " : (s.Length<50) ? "11 " : "20 ")
	+ "-evalue 1 -perc_identity 95 "
	+ "-dust no " 
	+ "-outfmt \"6 pident nident sstart send sstrand sseqid evalue\"";

var rtCode = shell.Run(cmd, 0, true);
if ( rtCode != 0 ) {
	vv.Message("blastn call failed: " + rtCode);
	var errFile = fso.CreateTextFile("error.log");
	errFile.WriteLine(cmd);
	errFile.Close();
	vv.Return();
}

var outFile = fso.OpenTextFile("result.txt", 1, false);

var seqLen = s.Length;
var mismatches = Math.floor(0.1 * s.Length);
var matchRegion = pp.Regions[0];
var matchRegionFlip = pp.Regions[3];
matchRegion.Clear();
matchRegionFlip.Clear();
var matches = 0;
var maxLen = 0;
var chrName = pp.SequenceName;
var otherMatches = 0 ;

while(! outFile.AtEndOfStream ) {
	var line = outFile.ReadLine();
	var fs = line.Split("\t");
	
	if ( fs[5] != chrName) { otherMatches++; continue; }

	if ( ((seqLen - fs[1]) < mismatches) 
	  || ((fs[0]=="100.000") && (fs[1]>=20) ) ) {
		var seq;
		if( fs[4] == "minus" ) {
			seq = New.SequenceInterval(fs[3]-1, fs[2]-1);
			matchRegionFlip.Add(seq.Shift(-pp.BaseLocation));
		} else {
			seq = New.SequenceInterval(fs[2]-1, fs[3]-1);
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
