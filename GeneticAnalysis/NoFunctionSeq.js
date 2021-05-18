//File: NoFunctionSeq.js
// Extract the left flank (5prime) no functional region.
var ds = vv.Dataset;
var seqList = New.SequenceIntervalList();
var senseList = New.IntArray();
var idxBegin = 0;
var maxLength = 0;
var MAX_LEN = 500;
var rightFlank = (vv.EventSource.Item.ToString() == "RightFlank");

if ( rightFlank ) {
	for(var i=0; i<ds.Rows; i++) {
		var antiSense = (ds.GetDataAt((i==ds.Rows)?(i-1):i,5) == "-1");
		if ( antiSense ) {
			var idxBegin = (i==0) ? 0 : ( ds.GetDataAt(i-1, 4) - 0 );
			var idxEnd = (i==ds.Rows)?  (pp.SequenceTable.Length-1) : (ds.GetDataAt(i, 3) - 2);
			if ( (idxEnd-idxBegin+1) > MAX_LEN ) idxBegin = idxEnd-MAX_LEN+1;
			idxEnd = Math.min(idxEnd+3, pp.SequenceTable.Length-1);
			senseList.Add(true);
		} else {
			var idxBegin = ds.GetDataAt(i, 4) - 0;
			var idxEnd = (i==(ds.Rows-1))?  (pp.SequenceTable.Length-1) : (ds.GetDataAt(i+1, 3) - 2);
			if ( (idxEnd-idxBegin+1) > MAX_LEN ) idxEnd = idxBegin+MAX_LEN-1;
			idxBegin = Math.max(0, idxBegin-3);
			senseList.Add(false);
		}
	
		try {
			var seq = New.SequenceInterval(idxBegin, idxEnd);
			seqList.Add(seq);
			maxLength = Math.max(maxLength, seq.Length);
		} catch(errMsg) {
			vv.Message(errMsg + ": " + i + ": " + idxBegin + " : " + idxEnd)
		}
	}
} else {
	for(var i=0; i<=ds.Rows; i++) {
		var antiSense = (ds.GetDataAt((i==ds.Rows)?(i-1):i,5) == "-1");
		if ( antiSense ) {
			var idxBegin = ds.GetDataAt(i, 4) - 0;
			var idxEnd = (i==ds.Rows)?  (pp.SequenceTable.Length-1) : (ds.GetDataAt(i+1, 3) - 2);
			if ( (idxEnd-idxBegin+1) > MAX_LEN ) idxEnd = idxBegin+MAX_LEN-1;
			idxBegin = Math.max(0, idxBegin-3);
			senseList.Add(false);
		} else {
			var idxBegin = (i==0) ? 0 : ( ds.GetDataAt(i-1, 4) - 0 );
			var idxEnd = (i==ds.Rows)?  (pp.SequenceTable.Length-1) : (ds.GetDataAt(i, 3) - 2);
			if ( (idxEnd-idxBegin+1) > MAX_LEN ) idxBegin = idxEnd-MAX_LEN+1;
			idxEnd = Math.min(idxEnd+3, pp.SequenceTable.Length-1);
			senseList.Add(true);
		}
	
		var seq = New.SequenceInterval(idxBegin, idxEnd);
		seqList.Add(seq);
		maxLength = Math.max(maxLength, seq.Length);
	}
}

var sa = vv.FindPluginObject("SeqAnalysis");
var sm = New.SequenceMap(null, seqList.Count, maxLength);
for(var i=0; i<seqList.Count; i++) {
	sm.SetSequence(pp.GetSequence(seqList[i].Begin, seqList[i].End), i*maxLength)
	if(senseList[i] == rightFlank) {
		sa.Flip(sm.SequenceTable, i*maxLength, i*maxLength + seqList[i].Length-1);	
	}
}

sm.Show();
sm.ClickContextMenu("Seq/SortRows");
if ( !rightFlank ) sm.ClickContextMenu("Seq/RightAlignment");


