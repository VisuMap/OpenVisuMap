//File: SraMakeBlastDb.js
vv.CurrentDirectory = vv.CurrentDataDir;
var ds = vv.Dataset;
var sa = vv.FindPluginObject("SeqAnalysis");
var sBlob = sa.OpenSequence(ds.ColumnSpecList[0].Name);
var fso = new ActiveXObject("Scripting.FileSystemObject");
var dbFile = fso.CreateTextFile("seqdb.fa");

for(var i=0; i<ds.Rows; i++) {
	var idx = ds.GetDataAt(i, 0) - 1;
	var len = ds.GetDataAt(i, 1) - 0;
	var seq = sBlob.FetchSeq(idx, len);
	dbFile.WriteLine(">" + ds.BodyList[i].Id);
	dbFile.WriteLine(seq);
}
dbFile.Close();

var shell = new ActiveXObject("WScript.Shell");
shell.Run("makeblastdb -in seqdb.fa -dbtype nucl -input_type fasta -parse_seqids", 0, true);
