// EqualSlicing.js
var bList = vv.Folder.GetBlobListEx();
var N = bList[1].Split(' ')[1] - 0;
var columns = 50000;
var rows = parseInt(N/columns, 10);
var dt = New.FreeTable(rows, 2);
dt.ColumnSpecList[0].Id = "SeqIdx";
dt.ColumnSpecList[0].Name = bList[0];
dt.ColumnSpecList[1].Id = "SeqLen";
dt.ColumnSpecList[0].IsNumber = true;
dt.ColumnSpecList[1].IsNumber = true;


for(var row=0; row<rows; row++) {
	var R = dt.Matrix[row];
	R[0] = row*columns + "";
	R[1] = columns + "";
}
dt.SaveAsDataset("EqualSplicing", "Equaly slicing the genome");



