// Find the A G C T counts in selected number (table from heatmap).
//
var cs = New.CsObject("GenomeChr1",
"public void ColumnSum(double[][] M, double[][]table) {\
	for(int i=0; i<4; i++) { \
		for(int row=0; row<M.Length; row++) { \
			for(int col=0; col<M[row].Length; col++) \
				if( M[row][col] == (i+1) ) table[i][row] += 1.0; \
		}\
	}\
}");

var nt = pp.GetSelectedNumberTable();
vv.Message(nt.Rows + ":" + nt.Columns);

var sumTable = New.NumberTable(4, nt.Rows);
cs.ColumnSum(nt.Matrix, sumTable.Matrix);

for(var i=0; i<sumTable.Rows; i++)
	sumTable.RowSpecList[i].Id = "ACGT"[i];
var csList = sumTable.ColumnSpecList;
for(var col=0; col<csList.Count; col++) {
	csList[col].Id = nt.RowSpecList[col].Id;
}

sumTable.ShowAsBarBand();
