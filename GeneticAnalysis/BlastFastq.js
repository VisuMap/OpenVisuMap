//File: BlastFastq.js
//
// Blast a fastq file against a blast db created by BlastDB.js
//
vv.CurrentDirectory = "C:\\Users\\JamesLi\\Desktop\\C.Elegan"
var fqFile = "Data\\2937.fastq";
//var fqFile = "Data\\sample.fastq";
var fso = new ActiveXObject("Scripting.FileSystemObject");
var shell = new ActiveXObject("WScript.Shell");

if ( vv.ModifierKeys.ControlPressed ) {	
	//
	// Create the query file.
	//
	var queryFile = fso.CreateTextFile("query.fa", 2);
	var fq = fso.OpenTextFile(fqFile, 1);
	var lineNr = 0;
	while(!fq.AtEndOfStream) {
		var line = fq.ReadLine();
		var k = lineNr%4;
		if ( k == 0 ) {
			var fs = line.Split(':');
			fs = fs[4].Split('#');
			queryFile.WriteLine(">Id" + fs[0]);	
		} else if ( k == 1 ) {
			queryFile.WriteLine(line);
		}
		lineNr++;
	}
	queryFile.Close();
	fq.Close();
	
	//
	// Do the megablast.
	//
	var cmd = "blastn -db db.fa -query query.fa -out result2.txt "
	        + "-reward 1 -penalty -4 "
	        + "-gapopen 7 -gapextend 2 "
	        + "-task megablast "
	        + "-word_size 15 "
	        + "-evalue 1e-10 -perc_identity 99 "
	        + "-dust no "
		 + "-num_threads 6 "
	        + "-outfmt \"6 nident sseqid sstart send sstrand qseqid qlen \"";
	var rtCode = shell.Run(cmd, 0, true);
	
	/*
	if ( rtCode != 0 ) {
	        vv.Message("blastn call failed: " + rtCode);
	        var errFile = fso.CreateTextFile("error.log");
	        errFile.WriteLine(cmd);
	        errFile.Close();
	        vv.Return();
	}
	*/
}

var regP = pp.Regions[4];
regP.RegionStyle = 1;
regP.Opacity = 0.1;
regP.Color = New.Color("Green");
regP.Clear();

var regM = pp.Regions[5];
regM.RegionStyle = 1;
regM.Opacity = 0.1;
regM.Color = New.Color("Red");
regM.Clear();

/*  The C# implementation of the following block is much faster.
var outFile = fso.OpenTextFile("result2.txt", 1);
while(! outFile.AtEndOfStream ) {
	var fs = outFile.ReadLine().Split('\t');
	if ( (fs[0] == "32")	 && (fs[1] == pp.SequenceName) ) {
		var idx = (fs[4]=="plus") ? (fs[2]-1) : (fs[3]-1);
		var reg = (fs[4]=="plus") ? regP : regM;
		reg.Add(idx-16, idx+16);
	}
}
outFile.Close();
*/

var cs = New.CsObject("\
	public void ExtractHits(string fileName, string chromName, ISequenceMap seqMap) {\
		List<double> seqWeight = new List<double>();\
		List<bool> strand = new List<bool>();\
		using(TextReader tr = new StreamReader(fileName)) {\
			while(true) {\
				string line = tr.ReadLine();\
				if ( line == null ) break;\
				string[] fs = line.Split('\t');\
				if (int.Parse(fs[0])!=32) continue;\
				if ( ! fs[1].Equals(seqMap.SequenceName) ) continue;\
				bool isPlus = fs[4].Equals(\"plus\");\
				int idx = int.Parse(fs[isPlus?2:3]) - 1;\
				seqMap.Regions[isPlus?4:5].Add(idx - 16, idx+16);\
				seqWeight.Add( double.Parse(fs[5].Substring(2)) );\
				strand.Add(isPlus);\
			}\
		}\
		for(int i=0; i<seqWeight.Count; ) {\
			int k = i;\
			for(; k<seqWeight.Count; k++) {\
				if ( seqWeight[k] != seqWeight[i] ) {\
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
");

cs.ExtractHits("result2.txt", pp.SequenceName, pp);

pp.Redraw();
