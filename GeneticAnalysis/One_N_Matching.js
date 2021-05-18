// One_N_Matching.js

var cs = New.CsObject("\
	public void ExtractHits(string fileName, string chromName, ISequenceMap seqMap) {\
		List<double> seqWeight = new List<double>();\
		List<bool> strand = new List<bool>();\
		using(TextReader tr = new StreamReader(fileName)) {\
			while(true) {\
				string line = tr.ReadLine();\
				if ( line == null ) break;\
				string[] fs = line.Split('\t');\
				int L = int.Parse(fs[6]);\
				if ( (L-int.Parse(fs[0])) > 2 ) continue;\
				if ( ! fs[1].Equals(seqMap.SequenceName) ) continue;\
				bool isPlus = fs[4].Equals(\"plus\");\
				int idx = int.Parse(fs[isPlus?2:3]) - 1;\
				seqMap.Regions[isPlus?4:5].Add(idx, idx+L-1);\
				seqWeight.Add( double.Parse(fs[5].Substring(1)) );\
				strand.Add(isPlus);\
			}\
		}\
		for(int i=0; i<seqWeight.Count; ) {\
			int k = i;\
			for(; k<(seqWeight.Count+1); k++) {\
				if ( (k==seqWeight.Count) || (seqWeight[k] != seqWeight[i]) ) {\
					for(int ii=i; ii<k; ii++) {\
						seqWeight[ii] = 1.0/(k-i);\
					}\
					break;\
				}\
			}\
			i = k;\
		}\
		List<double> wPlus = new List<double>();\
		List<double> wMinus = new List<double>();\
		for(int i=0; i<seqWeight.Count; i++) (strand[i]?wPlus:wMinus).Add(seqWeight[i]);\
		vv.SetObject(\"wPlus\", wPlus);\
		vv.SetObject(\"wMinus\", wMinus);\
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

var L = 25;
vv.CurrentDirectory = vv.CurrentDataDir2;
var fso = new ActiveXObject("Scripting.FileSystemObject");

var inFile = fso.CreateTextFile("query3.fa");
for(var i=0; i<(sq.Length-L); i++) {
	inFile.WriteLine(">Q"+i);
	inFile.WriteLine(sq.Substring(i, L));
	//if ( i%100 == 0) vv.Echo2("i: " + i);
}
inFile.Close();

var cmd = "blastn -db db.fa -query query3.fa -out result3.txt "
        + "-reward 1 -penalty -3 "
        + "-gapopen 5 -gapextend 2 "
        + "-task megablast "
        + "-word_size 15 "
        + "-evalue 1 -perc_identity 100 "
        + "-dust no "
	 + "-max_hsps 30 "
	 + "-num_threads 6 "
        + "-outfmt \"6 nident sseqid sstart send sstrand qseqid qlen evalue\"";
var errFile = fso.CreateTextFile("cmd.log");
errFile.WriteLine(cmd);
errFile.Close();

var shell = new ActiveXObject("WScript.Shell");
var rtCode = shell.Run(cmd, 1, true);

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


cs.ExtractHits("result3.txt", pp.SequenceName, pp);

var sa = vv.FindPluginObject("SeqAnalysis");
for(var i=4; i<=5; i++) pp.Regions[i].SetRange(sa.UnionIntervals(pp.Regions[i]));


pp.AddSelections(pp.Regions[4]);
pp.AddSelections(pp.Regions[5]);
pp.FlushingSelection();

pp.Redraw();
