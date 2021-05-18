// File: CompSeqProfile.js
// ========================================================================================

var filter = vv.Folder.OpenSequence2Filter(vv.Map.Filter);

var selections = pp.SelectedSections();
if ( selections.Count < 1 ) {
	vv.Message("No sequence selected");
	vv.Return(-1);
}

var stepSize = 48;   // should be a factor of 3.
var refLength = 1200;
var phase = 0;

var selection = selections[0];
var seq = pp.GetSequence(selection.Begin, selection.End);
var minLength = refLength+stepSize;
if ( seq.Length < minLength ) {
	vv.Message("Selected sequence too short: " + seq.Length + " < " + minLength);
	vv.Return(-2);
}

var sa = vv.FindPluginObject("SeqAnalysis");
if ( sa == null ) {
	vv.Message("Please install GeneticAnalysis plugin." );
	vv.Return(-3);
}

var bodyList = null;
var refSeq = seq.Substring(phase, refLength);
//refSeq = RandomizeSeq(refSeq);
pp.Title = "Total tests: " + ( parseInt( (seq.Length - refLength)/stepSize, 10 ) + 1 );
for(var i=0; i<(seq.Length - refLength - phase); i+=stepSize) 
	DoTest(refSeq, seq.Substring(phase+i, refLength), i);

// ================================================================================

function DoTest(refSeq, targetSeq, seqIdx) {
	filter.Sequence = refSeq + "\r\n\r\n" + targetSeq;
	filter.StepSize = 0;
	filter.Save();
	vv.Map.SetMetricAndFilter(vv.Map.Metric, vv.Map.Filter);

	if ( filter.StepSize >= 0 ) { return;}

	var op = New.AffinityEmbedding(); op.CoolingSpeed = 0.2;
	//var op = New.TsneMap();  op.MaxLoops = 200;	
	op.TheForm.WindowState = 1;
	op.Is3D = true;
       op.Show();

	op.Reset().Start().Close();

	if ( bodyList == null ) {
		bodyList = New.BodyList();
		for(var body in vv.Dataset.BodyList) 
			if( !body.Disabled && (body.Type == 0) ) bodyList.Add(body);
	}
	sa.CalculateCurvature(null, "S"+seqIdx, 0, bodyList, -filter.StepSize, -filter.StepSize)
}

function RandomizeSeq(seq) {
	var cList = seq.ToCharArray();
	var N = cList.Length;
	for(var i=0; i<N; i++) {
		var j = (Math.random() * N) % N;
		var tmp = cList[i];
		cList[i] = cList[j];
		cList[j] = tmp;
	}
	return cList.join("");
}


