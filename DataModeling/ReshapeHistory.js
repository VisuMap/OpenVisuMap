// File: ReshapeHistory.js
//

var dm = vv.FindPluginObject("DMScript");
var tr = dm.CurrentTrainer
var sz = tr.MaxEpochs/tr.RefreshFreq;

var nt = pp.GetNumberTable().Reshape(0,sz);
for(var row=0; row<nt.Rows; row++) 
	nt.RowSpecList[row].Type = row;
nt.ShowValueDiagram();
