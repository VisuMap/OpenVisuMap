// File: BlastDB.js
// Create blast data base for selected chromosomes. 
//
// Notice: the application makeblastdb.exe must be installed in to Windows PATH environment variable. 
//
var dbDir = vv.CurrentDataDir2 + "/Chr1";
var genomes = New.StringArray(
"Chr1",
//"Chr1", "Chr2", "Chr3", "Chr4", "Chr5"
//    "Chr5"
//"ChrI","ChrII","ChrIII","ChrIV","ChrV","ChrX"
//"ChrX", "ChrY"
//"Chr2L", "Chr2R", "Chr3L", "Chr3R", "Chr4", "ChrX", "ChrY"
);

var fso = new ActiveXObject("Scripting.FileSystemObject");
if ( ! fso.FolderExists(dbDir) ) fso.CreateFolder(dbDir);
vv.CurrentDirectory = dbDir;

var ga = vv.FindPluginObject("SeqAnalysis");
var dbFile = fso.CreateTextFile("db.fa");


for(var gn in genomes) {
	var sBlob = ga.OpenSequence(gn);
	dbFile.WriteLine(">" + gn);
	for(var i=0; i<sBlob.Length; i+=70)
		dbFile.WriteLine(sBlob.ReadSeq(70));
	sBlob.Close();
	vv.Echo("Done: " + gn);
}
dbFile.Close();

var shell = new ActiveXObject("WScript.Shell");
shell.Run("makeblastdb -in db.fa -dbtype nucl -input_type fasta -parse_seqids", 0, true);




