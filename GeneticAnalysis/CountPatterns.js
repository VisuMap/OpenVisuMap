// PatternCount.js
//
// Counts frequency of different word patterns.
//==================================================
var normalizing = false;
var fuzyMatch = false;
var ds = vv.Dataset;
var sm = vv.FindPluginObject("SeqAnalysis").OpenSequence(ds.ColumnSpecList[0].Name);
//var p = "ATATAT AGGAGGAGG TGGTGGTGG GTTGTTGTT";
//var p = "GCCCCC CTTTTT CAAAAA TCCCCC";
var p = "ATT CGG GCC ATG";

sm.SeqParseInit(p, fuzyMatch);

ds.AddColumns(true, ds.Columns-1, sm.SeqParseDimension())
var column0 = ds.Columns - sm.SeqParseDimension();

var pList = p.split(" ");
var csList = ds.ColumnSpecList;
for(var col=0; col<sm.SeqParseDimension(); col++) csList[column0+col].Id = "Cnt:"+pList[col];

for(var row=0; row<ds.Rows; row++) {
	var f = sm.SeqParse(row);
	for(var i=0; i<f.Length; i++) 
		ds.SetDataAt(row, column0+i, f[i]);
}
ds.CommitChanges();

if ( normalizing ) NormalizeRows();

//========================================================
function NormalizeRows() {
	var ds = vv.Dataset;
	for(var row=0; row<ds.Rows; row++) {
		var weight = 1000.0/ds.GetDataAt(row, 1);
		for(var col=2; col<ds.Columns; col++)
			ds.SetDataAt(row, col, weight * ds.GetDataAt(row, col));
	}	
	ds.CommitChanges();
}

