// DisplayOneSeq.js

var t = pp.GetSelectedNumberTable();
var seq = "";
var acgt = "ACGT";

if ( t.ColumnSpecList[0].Id == "SeqIdx" ) {
  var sm = vv.FindPluginObject("SeqAnalysis").OpenSequence(vv.Dataset.ColumnSpecList[0].Name);
  seq = sm.FetchSeq(t.Matrix[0][0], t.Matrix[0][1]);
} else {
  for(var col=0; col<t.Columns; col++) {
	var key = parseInt(t.Matrix[0][col], 10);	
	if ( (key >=1) && (key<=4) ) {
		key--;
		seq += acgt[key];
	}
  }
}

var seqLength = seq.Length;
var nt = New.NumberTable(1, seqLength);
for(var i=0; i<nt.Columns; i++) nt.Matrix[0][i] = 1 + acgt.IndexOf(seq[i]);
var hm = nt.ShowHeatMap();
var seqTitle = ( seqLength < 200 ) ? seq :	
	( seq.Substring(0, 100) + " ...... " + seq.Substring(seqLength-100, 100) );
hm.Title = t.RowSpecList[0].Id + " : " + seqTitle;
nt = null;

// Copy the sequence to clipboard
var seq2 = "";
for(var n=0; n<seqLength; n+=60) {
  seq2 += seq.Substring(n, Math.min(60, seq.Length-n)) + "\n";
}
vv.GuiManager.SetClipboard(">" + t.RowSpecList[0].Id + " : " + seq.Length + "\r\n" + seq2);

