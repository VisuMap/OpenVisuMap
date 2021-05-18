// Creates a node map by the fan-out/in vectors. 
// Biases are considered as input from their previous layer.
// 
var md = vv.GetObject("EvalMd"); // vv.SetObject("EvalMd", null);
var vName = "Layer_3/mx:0";
//var vName = "Layer_1/bias:0";
if (md == null) {
  md = vv.FindPluginObject("DMScript").NewLiveModel();
  md.X = vv.GetNumberTable().Matrix;
  md.Z = New.CsObject("\
	public double[][] Prob(double[][] A, int columnIdx, double factor, double shift){\
		if ( (columnIdx < 0) || (columnIdx>=A[0].Length) )\
			return A;\
		double[][] B = new double[A.Length][];\
		for(int i=0; i<A.Length; i++) {\
			B[i] = (double[]) A[i].Clone();\
			B[i][columnIdx] = factor * B[i][columnIdx] + shift;\
		}\
		return B;\
	}\
   ");
  md.Y = md.ReadVariable(vName);
  if ( md.Y[0].Length == 1 ) { // the variable is 1-dim tensor.
      md.Y = New.NumberTable(md.Y).Transpose2().Matrix;
  }
  vv.SetObject("EvalMd", md);
}

vv.GuiManager.RememberCurrentMap();
var columnIdx = 1; //-1:3
var factor = 0.95; 
var shift = 0;
var W = md.Z.Prob(md.Y, columnIdx, factor, shift); 
md.WriteVariable(vName, W);
md.Eval(md.X, true, vv.Dataset.BodyList);
vv.Map.Redraw();
