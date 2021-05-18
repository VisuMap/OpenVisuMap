// PatternCount.js
//
// Counts frequency of different word patterns.
//==================================================
var normalizing = true;
var fuzyMatch = false;

var ds = vv.Dataset;
var sm = vv.FindPluginObject("SeqAnalysis")
	.OpenSequence(ds.ColumnSpecList[0].Name);

var pList = New.StringArray(
	"CCG CGC CGG GCC GCG GGC GGG",
	"AAA AAT ATA ATT TAA TAT TTT"
 );

for(var p in pList) {
	sm.SeqParseInit(p, fuzyMatch);

	if ( sm.SeqParseDimension() + 2 > ds.Columns ) {
		vv.Message("Not enough columns!\n" +
			"Please add more columns and adjust the filter.");
		vv.Return(0);
	}
	
	for(var row=0; row<ds.Rows; row++) {
		var f = sm.SeqParse(row);
		for(var i=0; i<f.Length; i++) 
			ds.SetDataAt(row, 2+i, f[i]);
		for(var i=2+f.Length; i<ds.Columns; i++) 
			ds.SetDataAt(row, i, 0);
	}
	ds.CommitChanges();
	
	if ( normalizing ) NormalizeRows();
	
	RunMDS();
	Show3DView(p);
}

//=====================================================

function RunMDS() {
	var mds = New.TsneMap();
	//var mds = New.CcaMap();
	mds.Show();
	mds.Reset();
	mds.Start();
	mds.Close();
}

function NormalizeRows() {
	var ds = vv.Dataset;
	for(var row=0; row<ds.Rows; row++) {
		var weight = 1000.0/ds.GetDataAt(row, 1);
		for(var col=2; col<ds.Columns; col++)
			ds.SetDataAt(row, col, weight * ds.GetDataAt(row, col));
	}	
	ds.CommitChanges();
}

function Show3DView(seq) {
	var m3d = New.Map3DView();
	m3d.Show();
	m3d.Title = "Seq: " + seq;
	m3d.DoPcaCentralize();
	for(var i=0; i<70; i++) { 
		vv.Sleep(75);
	     m3d.RotateXYZ(0, 0.02, 0);
	}
}