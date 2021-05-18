// Client program to test two running models at the same time
// The twp models must be started before running this script.

function CallBack() {
  var bv = pp;
  var hv = vv.EventSource.Argument;
  var md1 = bv.Tag;
  var md2 = hv.Tag;

  var D = vv.Map.GetSelectedNumberTable();
  if (D.Rows==0) vv.Return(0);
  var R1 = md1.EvalVariable(D.Matrix, "OutputTensor", true)[0];
  var R2 = md2.EvalVariable(D.Matrix, "OutputTensor", true)[0];

  var L = R1.Length;
  var m1 = 0.0;
  var m2 = 0.0;
  for(var i=0; i<L; i++) {
	m1 += R1[i];
	m2 += R2[i];
  }
  m1 /= L;
  m2 /= L;
  var M = bv.GetNumberTable().Matrix;
  for(var i=0; i<L; i++) {
	M[0][i] = R1[i] - m1;
	M[1][i] = R2[i] - m2;
  }  
  bv.Redraw();
  bv.Title = "Count: " + D.Rows;
  hv.AddStep(m1, m2);
}

var md1 = vv.FindPluginObject("DMScript").NewLiveModel(null, true, 8889);
var md2 = vv.FindPluginObject("DMScript").NewLiveModel(null, true, 8879);

var bv = New.BarBand(New.NumberTable(2, md1.OutputDimension)).Show();
var hv = New.HistoryView(2).Show();
bv.Tag = md1;
hv.Tag = md2;
vv.EventManager.OnItemsSelected(CallBack.toString(), bv, hv);
