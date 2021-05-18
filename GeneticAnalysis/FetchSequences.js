// FetchSequences.js

var t = pp.GetSelectedNumberTable();
var seq = "";
if ( t.ColumnSpecList[0].Id == "SeqIdx" ) {
  var sm = vv.FindPluginObject("SeqAnalysis").OpenSequence(vv.Dataset.ColumnSpecList[0].Name);  
  for(var row=0; row<t.Rows; row++) {
  	seq += "["+ t.RowSpecList[row].Id + " : " + t.Matrix[row][1] + "]\r\n" 
	    + FormatSeq(sm.FetchSeq(t.Matrix[row][0], t.Matrix[row][1]));
  }
} else {
  var acgt = "ACGT";
  for (var row=0; row<t.Rows; row++) {
       var sRow = "";
	  for(var col=0; col<t.Columns; col++) {
		var key = parseInt(t.Matrix[0][col], 10);	
		if ( (key >=1) && (key<=4) ) {
			key--;
			sRow += acgt[key];
		}
	  }
	  seq += "["+ t.RowSpecList[row].Id + " : " + t.Columns + "]\r\n" 
		+ FormatSeq(sRow);
  }
}

vv.GuiManager.SetClipboard(seq);

// =============================================================

function FormatSeq(s) {
	var s2 = "";
	for(var n=0; n<s.Length; n+=100) {
		s2 += s.Substring(n, Math.min(100, s.Length-n)) + "\r\n";
	}
	return s2;
}
