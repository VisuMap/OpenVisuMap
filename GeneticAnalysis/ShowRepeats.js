// ShowRepeats.js
var dbDir = "Chr1";
var rpFile = "repeats.txt";
vv.CurrentDirectory = vv.CurrentDataDir2;

var cs = New.CsObject("\
	public HashSet<string> FrequentMotifs(string fileName) {\
		var cnt = new Dictionary<string, int>();\
		using(TextReader tr = new StreamReader(fileName)) {\
			while(true) {\
				string line = tr.ReadLine();\
				if ( line == null ) break;\
				var qPos = line.Split('\t')[3];\
				if ( cnt.ContainsKey(qPos) ) {\
					cnt[qPos]++;\
				} else {\
					cnt[qPos]=1;\
				}\
			}\
		}\
		var hCount = new HashSet<string>();\
		foreach(var key in cnt.Keys) {\
			if ( cnt[key] >=30 )\
				hCount.Add(key);\
		}\
		return hCount;\
	}\
\
	public int MaskRepeats(string fileName, byte[] seq) {\
		for(int i=0; i<seq.Length; i++) seq[i] |= 0x08;\
		int motifs = 0;\
		using(TextReader tr = new StreamReader(fileName)) {\
			while(true) {\
				string line = tr.ReadLine();\
				if ( line == null ) break;\
				var fs = line.Split('\t');\
				int iBegin = int.Parse(fs[0]);\
				int iEnd = int.Parse(fs[1]);\
				motifs++;\
				for(int k=iBegin; k<=iEnd; k++)\
					if( k<seq.Length )\
						seq[k] &= 0x77;\
			}\
		}\
		return motifs;\
	}\
");

var n = cs.MaskRepeats(dbDir + "\\" + rpFile, pp.SequenceTable);
pp.Title = "Motifs #: " + n;

//pp.Regions[0].Clear(); pp.Regions[2].Clear();
pp.Regions[0].RegionStyle = pp.Regions[2].RegionStyle = 2;
pp.Regions[1].RegionStyle = pp.Regions[3].RegionStyle = 6;

pp.Redraw();
pp.Title = "db Chromsome: " + dbDir;
