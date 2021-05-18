// RepeatedSeq.js
var dbDir = "Chr1";
var qChrName = "Chr1";
vv.CurrentDirectory = vv.CurrentDataDir2 + "/" + dbDir;

var fso = new ActiveXObject("Scripting.FileSystemObject");
var shell = new ActiveXObject("WScript.Shell");
var sa = vv.FindPluginObject("SeqAnalysis");
var seqBlob = sa.OpenSequence(qChrName);
if ( seqBlob == null ) {
	vv.Echo("Chronsome blob: " + qChrName + " not found!");
	vv.Return(1);
}
var seqAll = seqBlob.FetchSeq(0, seqBlob.Length);
var seqBegin = 0;  // (seqBegin; seqLen) set the scanned interval.
//var seqLen = 50*1000; 
var seqLen = seqAll.Length - seqBegin;
var L = 30;  // seed sequence length
var queries = 5000; // number of short sequences per fasta query file
var minRepreats = 30;
var minGap = L;
var wList = New.ObjectArray(null, null, null, null, null, null);  // worker list.
var W = wList.Count;
var w = 0; // worker index.

if ( fso.FileExists("spectrum.txt") ) fso.DeleteFile("spectrum.txt");
if ( fso.FileExists("repeats.txt") ) fso.DeleteFile("repeats.txt");

var cmd = "blastn";
var arg = " -db db.fa -query query3_WW.fa -out result3_WW.txt "
        + "-reward 1 -penalty -3 "
        + "-gapopen 5 -gapextend 2 "
        + "-task blastn-short "
        + "-word_size 15 "
        + "-evalue 1 -perc_identity 100 "
        + "-dust no "
	 + "-max_hsps 30 "
        + "-outfmt \"6 nident sseqid sstart send sstrand qseqid qlen evalue\"";

//=========================================================================================
var cs = New.CsObject("\
	public int ExtractMatches(string inFile, string outFile, string repFile,\
			BitArray mask, int minRepeats, int minGap, int lastPeak) {\
		string qid = null;\
		int cnt = 0;\
		HashSet<string> motifs = new HashSet<string>();\
		using(TextWriter tw = new StreamWriter(outFile, true))\
		using(TextReader tr = new StreamReader(inFile)) {\
			string[] fs=null;\
			while(true) {\
				string line = tr.ReadLine();\
				if ( line == null ) break;\
				fs = line.Split('\t');\
				int L = int.Parse(fs[6]);\
				if ( (L-int.Parse(fs[0])) > 2 ) continue;\
				if (fs[5] != qid) {\
					if ( (qid != null) && (cnt>=minRepeats) ) {\
						motifs.Add(fs[5]);\
						int peak = int.Parse(fs[5].Substring(1));\
						if ( (peak - lastPeak) > minGap ) {\
							tw.WriteLine(fs[5] + '\t' + fs[2] + '\t' + fs[3] + '\t' + cnt);\
						}\
						lastPeak = peak;\
					}\
					cnt = 1;\
					qid = fs[5];\
				} else {\
					cnt++;\
				}\
			}\
			if (cnt>=minRepeats) {\
				int peak = int.Parse(fs[5].Substring(1));\
				if ( (peak - lastPeak) > minGap ) {\
					tw.WriteLine(fs[5] + '\t' + fs[2] + '\t' + fs[3] + '\t' + cnt);\
				}\
				motifs.Add(fs[5]);\
				lastPeak = peak;\
			}\
\
		}\
\
		using(TextReader tr = new StreamReader(inFile)) {\
		using(TextWriter tw = new StreamWriter(repFile, true))\
			while(true) {\
				string line = tr.ReadLine();\
				if ( line == null ) break;\
				var fs = line.Split('\t');\
				int L = int.Parse(fs[6]);\
				if ( (L-int.Parse(fs[0])) > 2 ) continue;\
				if ( motifs.Contains(fs[5]) ) {\
					int idx = int.Parse(fs[5].Substring(1));\
					if (! mask[idx] ) {\
						tw.WriteLine(idx.ToString() + '\t' + (idx+minGap-1));\
						mask[idx] = true;\
					}\
				}\
			}\
		}\
		return lastPeak;\
	}\
	public int MakeFasta(string faName, string seq, int queries, BitArray mask,\
			int begin, int end, int L) {\
		int qs = 0;\
		using(TextWriter tw = new StreamWriter(faName)) {\
			for(int k=begin; k<end; k++) {\
				if ( ! mask[k] ) {\
					tw.WriteLine(\">Q\"+k);\
					tw.WriteLine(seq.Substring(k, L));\
					qs++;\
					if ( qs >= queries ) return (k+1);\
				}\
			}\
		}\
		return (qs==0)?-1:end;\
	}\
	public BitArray CreateMask(string seq, int seqBegin, int seqLen, int L) {\
		int seqEnd = seqBegin + seqLen;\
		var mask = new BitArray(seqEnd+1);\
		for(int i=seqEnd-L+1; i<seqEnd; i++) mask[i] = true;\
		int n = seqBegin;\
		char[] agct = new char[] {'A', 'C', 'G', 'T'};\
		while ( true ) {\
			int iBegin = seq.IndexOf('N', n, seqEnd-n);\
			if ( iBegin < 0 ) break;\
			int iStart = Math.Max(seqBegin, iBegin-L);\
			int iEnd = seq.IndexOfAny(agct, iBegin+1, seqEnd-iBegin-1);\
			if (iEnd<0) iEnd = seqEnd;\
			for(int i=iStart; i<iEnd; i++) mask[i] = true;\
			n = iEnd; \
		}\
		return mask;\
	}\
");

var mask=cs.CreateMask(seqAll, seqBegin, seqLen, L);

function CheckJobs() {
	for(w=0; w<W; w++) {
		if ( (wList[w] != null) && wList[w].HasExited ) 
			break;
	}

	if ( w < W ) {
		cs.ExtractMatches("result3_" + w + ".txt", 
			"spectrum.txt", "repeats.txt", mask, minRepreats, minGap, 0);
		wList[w] = null;
	}

	vv.Sleep(10);

	for(w=0; w<W; w++) 
		if (wList[w] != null) 
			return true;
	return false; // no more active jobs?
}

//=========================================================================================


var begin = seqBegin;
var seqEnd = seqBegin + seqLen;
while(begin < seqEnd) {
	for(w=0; w<W; w++) 
		if (wList[w] == null) 
			break;  // find a free worker!

	if ( w < W ) {
		var faFile = "query3_" + w + ".fa";
		begin = cs.MakeFasta(faFile, seqAll, queries, mask, begin, seqEnd, L);
		vv.Title = "Repeats - Index: " + begin.ToString("# ### ###");
		wList[w] = vv.StartProcess(cmd, arg.Replace("WW", ""+w), true);
	} 
	CheckJobs();
}

while(true) 
	if (! CheckJobs() ) 
		break;


