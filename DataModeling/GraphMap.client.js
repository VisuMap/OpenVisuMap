// Creates a node map by the fan-out/in vectors. 
// Biases are considered as input from their previous layer.
// 
var modelName = pp.TheForm.ModelName;
var md = vv.FindPluginObject("DMScript").NewLiveModel(modelName);
var nt = New.NumberTable(0, 0);
var rType = 0;

function CreateMdsMap(nt, title) {
	var mp = New.MdsCluster(nt);
       mp.Show();
	mp.MdsAlgorithm=2;
	mp.PerplexityRatio = 0.05;
	mp.Is3D = false;
	mp.Reset().Start();	
	mp.Title = title;
}

var ndList = New.StringArray("Layer", "Layer_1", "Layer_2", "Layer_3");
for(var nd in ndList) {
  var t = New.NumberTable(md.ReadVariable(nd + "/mx:0"));
  for(var cs in t.RowSpecList) cs.Type = rType;  rType++;
  nt.Append(t);
  t = New.NumberTable(md.ReadVariable(nd + "/bias:0"));
  t.Transpose();
  t.RowSpecList[0].Type = rType++;
  nt.Append(t);
}
CreateMdsMap(nt, "Node fan-out map");

var ndList = New.StringArray("Layer_1", "Layer_2", "Layer_3", "Layer_4");
var nt = New.NumberTable(0, 0);
var rType = 0;
for(var nd in ndList) {
  var t = New.NumberTable(md.ReadVariable(nd + "/mx:0"));
  t.Transpose();
  var b = New.NumberTable(md.ReadVariable(nd + "/bias:0"));
  t.AppendColumns(b);
  for(var cs in t.RowSpecList) cs.Type = rType;  rType++;
  nt.Append(t);
}
CreateMdsMap(nt, "Node fan-in map");
