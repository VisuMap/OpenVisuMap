// FindSequnce.js
//var seq = pp.ScriptPanel.SelectedText;

var seq = "atgacatttcttgtaaatga".ToUpper();

var sm = vv.FindPluginObject("SeqAnalysis")
		.OpenSequence(vv.Dataset.ColumnSpecList[0].Name);

var idx = FindSeq(seq);

if (idx >=0 ) {
	vv.Message("Idx: " + idx + ": " + vv.Dataset.BodyList[idx].Id);
} else {
	vv.Message("No matching sequence found!");
}

function FindSeq(seq) {
	for(var row=0; row<vv.Dataset.Rows; row++) {
		sm.SeqParseInit(seq, false);
	
		var f = sm.SeqParse(row);
	     if ( f[0] != 0 ) {
			return row;
		}
	}
	return -1;
}