//File: MergeSeqBlobs.js
var sm = vv.FindPluginObject("SeqAnalysis");
var bAll = sm.CreateSequenceBlob("BlobAll");
var offset = 0;
var tAll = New.NumberTable(0, 2);
tAll.ColumnSpecList[0].Id = "SeqIdx";
tAll.ColumnSpecList[0].Name = bAll.Name;
tAll.ColumnSpecList[1].Id = "SeqLen";

for(var dn in vv.Folder.DatasetNameList) {
	if ( ! dn.StartsWith("Chr") ) continue;
	var ds = vv.Folder.OpenDataset(dn);
	var bName = ds.ColumnSpecList[0].Name;
	var b = sm.OpenSequence(bName);
	vv.Echo(bName);

	for(var n=0; n<b.Length; n+=1000)
		bAll.Add(b.FetchSeq(n, Math.min(1000, b.Length-n)))

	var i0 = tAll.Rows;
	tAll.Append(ds.GetNumberTable().SelectColumns(New.IntArray(0,1)));	
	for(var i=0; i<ds.Rows; i++) tAll.Matrix[i0+i][0] += offset
	offset += b.Length;
	b.Close();
	
}

bAll.Close();
tAll.SaveAsDataset("AllChrCDS", null);
vv.Echo("Done!");

