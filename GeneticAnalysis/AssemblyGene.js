// AssemblyGene.js
var ds = vv.Dataset;
var sm = vv.FindPluginObject("SeqAnalysis").OpenSequence(ds.ColumnSpecList[0].Name);

var ht = New.Hashtable();

for(var row=0; row<ds.Rows; row++) {
   var gName = ds.BodyList[row].Name;
   var seqLen = int.Parse(ds.GetValueAt(row, 1), 10);
   if ( ht.ContainsKey(gName) && ( seqLen <= ht[gName] ) ) {
	;
   } else {
     ht[gName] = seqLen;
   }
}

var tb = New.NumberTable(ht.Count, 2)
var gIdx = 0;

var issued = New.HashSet();


for(var row=0; row<ds.Rows; row++) {
   var gName = ds.BodyList[row].Name;
   var seqLen = int.Parse(ds.GetValueAt(row, 1), 10);
   if ( (seqLen == ht[gName]) && ! issued.Contains(gName) ) {
	 var seqIdx = int.Parse(ds.GetValueAt(row, 0), 10);
	 tb.Matrix[gIdx][0] = seqIdx;
	 tb.Matrix[gIdx][1] = seqLen;
      tb.RowSpecList[gIdx].Id = gName;
	 issued.Add(gName);
	 gIdx++;
   }
}

tb.ColumnSpecList[0].CopyFrom(ds.ColumnSpecList[0]);
tb.ColumnSpecList[1].CopyFrom(ds.ColumnSpecList[1]);

var dsName = tb.SaveAsDataset(ds.Name, "Coding sequence of genes")
vv.Folder.OpenDataset(dsName);

