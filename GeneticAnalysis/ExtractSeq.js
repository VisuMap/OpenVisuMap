var ds = vv.Dataset;
var blobName = ds.ColumnSpecList[0].Name;
var sm = vv.FindPluginObject("SeqAnalysis").OpenSequence(blobName);

var cs = New.CsObject(`
	Random rg = new Random();
	public string RandomizeSeq(string s, double p) {
		char[] s2 = new char[s.Length];
		string a = \"AGCT\";
		for(int i=0; i<s.Length; i++)
			s2[i] = ( rg.NextDouble() < p ) ? a[rg.Next()%4] : s[i];
		return new String(s2);
	}

	public IList<double> GetFreqVector(dynamic sm, int len, int winSize, int stepSize) {
		IList<double> ret = New.NumberArray();
		IList<int> freq = New.IntArray(0,0,0,0);
		IList<int> win = New.IntArray(); 
		for(int i=0; i<winSize; i++) 
			win.Add(0);
		
		for(int i=0; i<winSize; i++) {
		    int idx = sm.GetLetter();
		    if ( (idx>=0) && (idx<4) ) freq[idx]++;
		    win[i] = idx;    
		}
		int wIdx = 0;
		for(int i=winSize; i<len; i++) {
		    if ( (i-winSize) % stepSize == 0 ) {
		       double a = ((double)freq[0])/winSize;
			double c = ((double)freq[1])/winSize;
			double g = ((double)freq[2])/winSize;
			double t = ((double)freq[3])/winSize;
			ret.Add(c);
			ret.Add(g);
			ret.Add(a);
			ret.Add(t);
		    }
		
		    int idx = sm.GetLetter();
		    if ( (idx>=0) && (idx<4) ) freq[idx]++;
		
		    int idx2 = win[wIdx];
		    if ( (idx2>=0) && (idx2<4) )  freq[idx2]--;
		    win[wIdx] = idx;
		    wIdx++; if ( wIdx == winSize ) wIdx = 0;
		}
		return ret;
	}

	public void CalculateFreq(dynamic ds, dynamic sm, int winSize, int stepSize, int columnNr	) {
		int c0 = ds.Columns - columnNr;
		int rows = ds.Rows;

		for(int row=0; row<rows; row++) {
			uint idx = (uint)ds.GetValueAt(row, 0);
			int len = (int)ds.GetValueAt(row, 1);
			sm.Seek(idx);
			IList<double> freq = GetFreqVector(sm, len, winSize, stepSize);

		  	for(int col=0; col<columnNr; col++)
		   		ds.SetDataAt(row, c0+col, ( col < freq.Count ) ? freq[col] : 0);    
		       if (row>0) {
		       	if ( row % 100 == 0 ) vv.Echo2(row + ", ");
		       	if ( row % 1000 == 0 ) vv.Echo("");
		    	}
	  	}

	}
`);


//================================================================================

function RandomizeSeq( p ) {
	var seqColumn = ds.IndexOfColumn("Seq");
	for(var row=0; row<ds.Rows; row++) {
		var s = ds.GetValueAt(row, seqColumn);
		s = cs.RandomizeSeq(s, p);
		ds.SetDataAt(row, seqColumn, s);
	}
	ds.CommitChanges();
	vv.Echo("Finished!");
}

function ExtractVector(winSize, stepSize, columnNr) {
  var cIdx = ds.IndexOfColumn("C0");
  if ( cIdx >= 0 ) {
     var cIdList = New.StringArray();
     for(var col=cIdx; col<ds.Columns; col++) cIdList.Add(ds.ColumnSpecList[col].Id);
     ds.RemoveColumns(cIdList);
  }
  ds.AddColumns(true, ds.Columns, columnNr);

  cs.CalculateFreq(ds, sm, winSize, stepSize, columnNr);
  ds.CommitChanges();
  vv.Echo("Finished!");
}

//================================================================================

var seqColumn;
var lenColumn;

function CheckSeqColumn() {
	seqColumn = ds.IndexOfColumn("Seq");
	if ( seqColumn < 0 ) {
		seqColumn = 2;
		ds.AddColumn("Seq", 0, "", seqColumn);
	}
	lenColumn = ds.IndexOfColumn("Len");
	if ( lenColumn < 0 ) {
		lenColumn = 3;
		ds.AddColumn("Len", 1, 0, lenColumn);
	}

}


function ExtractSeq(maxLen) {
	CheckSeqColumn();

	for(var row=0; row<ds.Rows; row++) {
		var idx = ds.GetValueAt(row, 0);
		var len = ds.GetValueAt(row, 1);
		if ( len > maxLen ) len = maxLen;
		ds.SetDataAt(row, seqColumn, sm.FetchSeq(idx, len));
		ds.SetDataAt(row, lenColumn, len);
	}
	ds.CommitChanges();
	vv.Echo("Finished!");
}

function ExtractSeq2(maxLen) {
	CheckSeqColumn();

	for(var row=0; row<ds.Rows; row++) {
		var idx = ds.GetValueAt(row, 0);
		var len = ds.GetValueAt(row, 1);
		if ( len > maxLen ) len = maxLen;
		len += Math.round(Math.random() * len);
		//if ( (len % 2) != 0 ) len--;
		ds.SetDataAt(row, seqColumn, sm.FetchSeq(idx, len));
		ds.SetDataAt(row, lenColumn, len);
	}
	ds.CommitChanges();
	vv.Echo("Finished!");
}

function ExtractSeq3(maxLen) {
	CheckSeqColumn();

	for(var row=0; row<ds.Rows; row++) {
		var idx = ds.GetValueAt(row, 0);
		var len = ds.GetValueAt(row, 1);
		if ( len > maxLen ) len = maxLen;
		var seq = sm.FetchSeq(idx, len);
		seq = seq.Replace('A', 'G');
		seq = seq.Replace('T', 'G');
		ds.SetDataAt(row, seqColumn, seq);
		ds.SetDataAt(row, lenColumn, len);
	}
	ds.CommitChanges();
	vv.Echo("Finished!");
}

function ExtractSeqBin(maxLen) {
	CheckSeqColumn();

	for(var row=0; row<ds.Rows; row++) {
		var idx = ds.GetValueAt(row, 0);
		var len = ds.GetValueAt(row, 1);
		if ( len > maxLen ) len = maxLen;
		var seq = sm.FetchSeq(idx, len);
		seq = seq.Replace("G", "00");
		seq = seq.Replace("C", "10");
		seq = seq.Replace("A", "01");
		seq = seq.Replace("T", "11");
		ds.SetDataAt(row, seqColumn, seq);
		ds.SetDataAt(row, lenColumn, seq.Length);
	}

	ds.CommitChanges();
	vv.Echo("Finished!");
}

function ExtractSeqBin2(maxLen) {
	ds.AddColumns(true, 2, 2*maxLen);

	for(var row=0; row<ds.Rows; row++) {
		var idx = ds.GetValueAt(row, 0);
		var seq = sm.FetchSeq(idx, maxLen);
		for(var i=0; i<seq.Length; i++) {
			var col = 2+2*i;
			switch(seq[i]) {
			case 'G':
				ds.SetDataAt(row, col, "0");
				ds.SetDataAt(row, col+1, "0");
				break;
			case 'C':
				ds.SetDataAt(row, col, "1");
				ds.SetDataAt(row, col+1, "0");
				break;
			case 'A':
				ds.SetDataAt(row, col, "0");
				ds.SetDataAt(row, col+1, "1");
				break;
			case 'T':
				ds.SetDataAt(row, col, "1");
				ds.SetDataAt(row, col+1, "1");
				break;
			default:
				ds.SetDataAt(row, col, "0");
				ds.SetDataAt(row, col+1, "0");
				break;
			}
		}
		if ( (row%10) == 0) vv.Echo("Row: " + row);
	}

	ds.CommitChanges();
	vv.Echo("Finished!");
}

function ExtractRandomBin(maxLen) {
	var cIdxList = New.StringArray();
	for(var col=2; col<ds.Columns; col++) cIdxList.Add(ds.ColumnSpecList[col].Id);
	ds.RemoveColumns( cIdxList );

	ds.AddColumns(true, 2, maxLen);

	for(var row=0; row<ds.Rows; row++)
	for(var col=2; col<ds.Columns; col++)
		ds.SetDataAt(row, col, (Math.random()>0.5)?"1":"0");

	ds.CommitChanges();
	vv.Echo("Finished!");
}

ExtractVector(200, 20, 750);

//ExtractSeq(17000);
//RandomizeSeq(0.7);
//ExtractSeq2(100);
//ExtractSeq3(17000);
//ExtractSeqBin(17000);
//ExtractSeqBin2(5882);
//ExtractRandomBin(20);

