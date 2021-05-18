//File: ShowGeneFlanks.js
// Show the 5'-flank of 1000 nb's.
var flankLength = 500;

var sList = New.SequenceIntervalList();
var allEnds = New.Hashtable();
var antiGeneIdx;

var leftFlank = ! vv.EventSource.Item.ToString().EndsWith("3P");

if ( leftFlank ) {
	for(var s in pp.Regions[0]){
		if ( ! allEnds.ContainsKey(s.Begin) ) {
			sList.Add( New.SequenceInterval(s.Begin - flankLength, s.Begin+2));
			allEnds.Add(s.Begin, 1);
		}
	}
	antiGeneIdx = sList.Count;	
	for(var s in pp.Regions[2]) {
		if ( ! allEnds.ContainsKey(s.End) ) {
			sList.Add( New.SequenceInterval(s.End-2, s.End + flankLength));
			allEnds.Add(s.End, 1);
		}
	}
} else {  // extract the right 3' flank
	for(var s in pp.Regions[0]){
		if ( ! allEnds.ContainsKey(s.End) ) {
			sList.Add( New.SequenceInterval(s.End-2, s.End + flankLength));
			allEnds.Add(s.End, 1);
		}
	}
	antiGeneIdx = sList.Count;	
	for(var s in pp.Regions[2]) {
		if ( ! allEnds.ContainsKey(s.Begin) ) {
			sList.Add( New.SequenceInterval(s.Begin - flankLength, s.Begin+2));
			allEnds.Add(s.Begin, 1);
		}
	}
}

var maxLength = flankLength + 3;
var sa = vv.FindPluginObject("SeqAnalysis");
var sm = New.SequenceMap(null, sList.Count, maxLength);
for(var i=0; i<sList.Count; i++) {
	sm.SetSequence(pp.GetSequence(sList[i].Begin, sList[i].End), i*maxLength)
	if(i >= antiGeneIdx) {
		sa.Flip(sm.SequenceTable, i*maxLength, (i+1)*maxLength-1);
	}
}
sm.Show();

