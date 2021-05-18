// ShowDensity.js
//
// Show the density of A G C T in a chromosome.
//==================================================

var ds = vv.Dataset;
var sm = vv.FindPluginObject("SeqAnalysis").OpenSequence(ds.ColumnSpecList[0].Name);
sm.SeqParseInit("A T C G", false);

var bins = 500;  // number of bins.
var columns = parseInt(sm.Length/bins, 10);
var rows = parseInt(sm.Length / columns + 1, 10);
var nt = New.NumberTable(4, rows);

for(var row=0; row<rows; row++) {
	var f = sm.SeqParse2(row * columns, columns);
	for(var i=0; i<4; i++)	nt.Matrix[i][row] = f[i];
	if(row%500==0){vv.Title="N: "+row;vv.Sleep(0);}
}

for(var i=0; i<4; i++) nt.RowSpecList[i].Id = "ATCG"[i];
nt.ShowAsBarBand();
