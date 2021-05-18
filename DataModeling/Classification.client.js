// Test classification capabilities.
// 
var md = vv.FindPluginObject("DMScript").NewLiveModel();
var input = vv.Map.GetSelectedNumberTable();
if ( input.Rows == 0 ) vv.Return(0);
var ret = md.EvalVariable(input.Matrix, "OutputTensor", true);
New.BarView(New.NumberTable(ret)).Show();