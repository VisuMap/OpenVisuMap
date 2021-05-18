// GeneNeighborhood.js
//
// Creates a cGMS map by moving a CDS sequence along its neighborhood
// in the chromosome. The ds dataset must have the exon begin and end 
// positions in column 3 and 4.
//
//!import "c:/work/VisuMap/PluginModules/GeneticAnalysis/SeqUtil.js"

//var cdsId = "Id5939"; 
//var cdsId = "Id2223";
//var cdsId = "Id6657";
var cdsId = "Id4605";
var chrBlob = "Chr1.fa";
var ds = vv.Folder.ReadDataset("Chr1CDS.fa");
var sa = vv.FindPluginObject("SeqAnalysis");
var filter = vv.Folder.OpenSequence2Filter(vv.Map.Filter);

var sm = sa.OpenSequence(ds.ColumnSpecList[0].Name);
var cdsIdx = ds.BodyIndexForId(cdsId);
var cdsSeq = sm.FetchSeq(ds.GetDataAt(cdsIdx, 0), ds.GetDataAt(cdsIdx, 1));

if ( ds.GetDataAt(cdsIdx, 2) < 0 ) cdsSeq = InvertReverse(cdsSeq);

var pos = New.IntArray(ds.GetDataAt(cdsIdx, 3) +";" +ds.GetDataAt(cdsIdx, 4));
for(var k=0; k<pos.Count; k++) pos[k]--;
var minIdx = 1000 * 1000000;
var maxIdx = 0;
for(var p in pos) {
	minIdx = Math.min(minIdx, p);
	maxIdx = Math.max(maxIdx, p);
}
var sm2 = sa.OpenSequence(chrBlob);
var L = cdsSeq.Length;
minIdx = Math.max(0, minIdx - L);
maxIdx = Math.min(sm2.Length-1, maxIdx + L);
var nbSeq = sm2.FetchSeq(minIdx, maxIdx - minIdx);

sm = sm2 = null;

for(var k = 0; k<(nbSeq.Length-L-1); k+=10) 
	DoTest(cdsSeq, nbSeq.Substring(k, L), "P"+k);

//====================================================

function DoTest(refGene, seqEnv, idEnv) {
	filter.Sequence = refGene + "\r\n\r\n" + seqEnv;
	filter.StepSize = 0;
	filter.ScanningSize = 40;
	filter.Save();
	vv.Map.SetMetricAndFilter(vv.Map.Metric, vv.Map.Filter);

	if ( filter.StepSize >= 0 ) { return;}

	var op = New.AffinityEmbedding();
	//var op = New.TsneMap();

	op.TheForm.WindowState = 1;
	op.Is3D = true;
	op.CoolingSpeed = 0.2;
	op.RefreshFreq = 1000;
     op.Show();

	for(var k=0; k<1; k++) 
		op.Reset().Start(), CalculateCurvature(idEnv+"_" + k);

	op.Close();
}

function CalculateCurvature(idEnv) {
	var b = New.BodyList();
	for(var body in vv.Dataset.BodyList) 
		if( !body.Disabled && (body.Type == 0) ) b.Add(body);
	sa.CalculateCurvature(null, idEnv, 0, b, -filter.StepSize, -filter.StepSize)
}
