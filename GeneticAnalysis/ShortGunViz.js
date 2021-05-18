// ShortGunVis.js
//
// Counts the frequncy of words of fixed length 
// and save them into the current table.
//==================================================

var ds = vv.Dataset;
var sm = vv.FindPluginObject("SequenceManager");
sm.OpenSequence( ds.ColumnSpecList[0].Name );

for(var row=0; row<ds.Rows; row++) {
	var f = sm.FetchSeqFreq(row, 4);
	for(var i=0; i<f.Length; i++) ds.SetDataAt(row, 86+i, f[i]);

	if ( (row % 100) == 0 ) { vv.Title = "N: " + row;  vv.Sleep(0); }
}

/* Set the column head according to their AGCT words.
var n="AGCT";
var idx = 0;
for(var i=0; i<4; i++)
for(var j=0; j<4; j++)
for(var k=0; k<4; k++) 
for(var p=0; p<4; p++) 
{
	vv.Dataset.ColumnSpecList[idx].Id = n[i] + n[j] + n[k] + n[p];
	idx++;
}
vv.Dataset.CommitChanges();
*/

