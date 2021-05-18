// One_N_MatchingBlat.js

var cs = New.CsObject("\
	public void ExtractHits(string fileName, string chromName, ISequenceMap seqMap, int queryLen) {\
		List<double> seqWeight = new List<double>();\
		List<bool> strand = new List<bool>();\
		using(TextReader tr = new StreamReader(fileName)) {\
			while(true) {\
				string line = tr.ReadLine();\
				if ( line == null ) break;\
				string[] fs = line.Split('\t');\
				int L = queryLen;\
				if ( (L-int.Parse(fs[3])) > 2 ) continue;\
				if ( ! fs[1].Equals(seqMap.SequenceName) ) continue;\
				int idxBegin = int.Parse(fs[8])-1;\
				int idxEnd = int.Parse(fs[9])-1;\
				bool isPlus = (idxEnd>=idxBegin);\
				int idx = isPlus?idxBegin:idxEnd;\
				seqMap.Regions[isPlus?4:5].Add(idx, idx+L-1);\
				seqWeight.Add( double.Parse(fs[0].Substring(1)) );\
				strand.Add(isPlus);\
			}\
		}\
	}\
\
	public string FlipString(string s) {\
		char[] a = s.ToCharArray();\
		for(int i=0; i<a.Length; i++) {\
			var c = a[i];\
			a[i] = (c=='A')?'T':(c=='T')?'A':(c=='G')?'C':'G';\
		}\
		Array.Reverse(a);\
		return new string(a);\
	}\
");

function GetSeqById(id) {
	var ds = vv.Dataset;
	var rIdx = ds.IndexOfRow(id);
	var seqBegin = ds[rIdx, 0] - 0;
	var seqLen = ds[rIdx,1] - 0;
	var sa = vv.FindPluginObject("SeqAnalysis");
	var blob = sa.OpenSequence("ChrICDS");
	return blob.FetchSeq(seqBegin, seqLen);
}
//var sq = GetSeqById("Id2547");
var sq = vv.GuiManager.GetClipboard();
//var sq = pp.SelectedSequence();

var L = 30;
vv.CurrentDirectory = vv.CurrentDataDir2;
var fso = new ActiveXObject("Scripting.FileSystemObject");

var inFile = fso.CreateTextFile("query3.fa");
for(var i=0; i<(sq.Length-L); i++) {
	inFile.WriteLine(">Q"+i);
	inFile.WriteLine(sq.Substring(i, L));
	//if ( i%100 == 0) vv.Echo2("i: " + i);
}
inFile.Close();

var cmd = "blat.exe db.2bit query3.fa result3.tex "
	+ "-q=dna "
	+ "-tileSize=11 "
	+ "-out=blast8 ";

var shell = new ActiveXObject("WScript.Shell");
var rtCode = shell.Run(cmd, 0, true);


var regP = pp.Regions[4];
regP.RegionStyle = 1;
regP.Opacity = 0.75;
regP.Color = New.Color("Green");
regP.Clear();
regP.Name = "Sense Matches";

var regM = pp.Regions[5];
regM.RegionStyle = 1;
regM.Opacity = 0.75;
regM.Color = New.Color("Red");
regM.Name = "AntiSense Matches";
regM.Clear();


cs.ExtractHits("result3.tex", pp.SequenceName, pp, L);


var sa = vv.FindPluginObject("SeqAnalysis");
for(var i=4; i<=5; i++) pp.Regions[i].SetRange(sa.UnionIntervals(pp.Regions[i]));

pp.AddSelections(pp.Regions[4]);
pp.AddSelections(pp.Regions[5]);
pp.FlushingSelection();

pp.Redraw();

