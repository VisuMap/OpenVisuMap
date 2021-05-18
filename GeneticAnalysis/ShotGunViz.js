// ShotGunVis.js
//
// Counts the frequncy of words of fixed length 
// and save them into the current table.
// The current table must have enough numerical columns to
// store the frequency data for all words of length of wordSize.
//
//==================================================

var ds = vv.Dataset;
var sm = vv.FindPluginObject("SeqAnalysis").OpenSequence(ds.ColumnSpecList[0].Name);

var wordSize = 2;
var columns = (1<<(2*wordSize));
var offset = ds.Columns;
ds.AddColumns(true, ds.Columns-1, columns)
sm.FetchFreqTable(wordSize, offset);

var normalizing = true;
if ( normalizing ) {
  for(var row=0; row<ds.Rows; row++) {
    var sum = 0;
    for(var col=offset; col<ds.Columns; col++)
       sum += ds.GetDataAt(row, col) - 0;
    for(var col=offset; col<ds.Columns; col++)
	  ds.SetValueAt(row, col, ds.GetDataAt(row, col)/sum);
  }
}

var L="ACGT";
var idx = offset;
var csList = vv.Dataset.ColumnSpecList;

if ( wordSize == 1 ) {
	for(var i=0; i<4; i++)
		csList[idx++].Id = L[i];
} else if ( wordSize == 2 ) {
	for(var i=0; i<4; i++)
	for(var j=0; j<4; j++)
		csList[idx++].Id = L[i] + L[j];
} else if ( wordSize == 3 ) {
	for(var i=0; i<4; i++)
	for(var j=0; j<4; j++)
	for(var k=0; k<4; k++) 
		csList[idx++].Id = L[i] + L[j] + L[k];
} else if ( wordSize == 4 ) {
	for(var i=0; i<4; i++)
	for(var j=0; j<4; j++)
	for(var k=0; k<4; k++)
	for(var p=0; p<4; p++)
		csList[idx++].Id = L[i] + L[j] + L[k] + L[p];
}

vv.Dataset.CommitChanges();
