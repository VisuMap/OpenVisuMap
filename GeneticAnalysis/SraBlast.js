// SraBlast.js
// Blast a set of SRA sequences against a blast db that was created by SraMakeBlastDb.js.
// The current dataset must be contains the SRA sequences.
//
vv.CurrentDirectory = vv.CurrentDataDir2;
var fso = new ActiveXObject("Scripting.FileSystemObject");
var shell = new ActiveXObject("WScript.Shell");
var ds = vv.Dataset;

if ( !vv.ModifierKeys.ControlPressed ) {
	var bs = ds.BodyList;
	var sa = vv.FindPluginObject("SeqAnalysis");
	var itemList = (vv.SelectedItems.Count<=1) ? vv.AllItems : vv.SelectedItems;
	var qFile = fso.CreateTextFile("query.fa");
	var sBlob = sa.OpenSequence(ds.ColumnSpecList[0].Name);
	for(var g in itemList) {
		var row = vv.Dataset.IndexOfRow(g);
		var geneName = bs[row].Id;
		var geneSeq = sBlob.FetchSeq(ds.GetDataAt(row,0)-0, ds.GetDataAt(row, 1)-0);
		qFile.WriteLine(">" + geneName);
		qFile.WriteLine(geneSeq);
	}
	qFile.Close();
	sBlob.Close();
}

var cmd = "blastn -db seqdb.fa -query query.fa -out result.txt "
        + "-reward 1 -penalty -4 "
        + "-gapopen 7 -gapextend 2 "
        + "-task megablast "
        + "-word_size 15 "
        + "-evalue 1e-10 -perc_identity 99 "
        + "-dust no "
	 + "-num_threads 6 "
        + "-outfmt \"6 pident nident evalue sseqid\"";
var rtCode = shell.Run(cmd, 0, true);
if ( rtCode != 0 ) {
        vv.Message("blastn call failed: " + rtCode);
        var errFile = fso.CreateTextFile("error.log");
        errFile.WriteLine(cmd);
        errFile.Close();
        vv.Return();
}

var bv = New.BarView();
var items = bv.ItemList;
var ht = New.Hashtable();
var outFile = fso.OpenTextFile("result.txt", 1, false);
while(! outFile.AtEndOfStream ) {
       var line = outFile.ReadLine();
	var fs = line.Split('\t');
	var nm = fs[3];	
	if ( ht.ContainsKey(nm) ) {
		ht[nm]++;
	} else {
		ht[nm] = 1;
	}
}
outFile.Close();

for(var nm in ht.Keys) {
	bv.ItemList.Add(New.ValueItem(nm, null, ht[nm]));
}
bv.ReadOnly = true;
bv.AutoScaling = false;
bv.LowerLimit = 0;
bv.UpperLimit = 550;
bv.NumberFormat = "f0";
//bv.SortItems(true);
bv.Show();
