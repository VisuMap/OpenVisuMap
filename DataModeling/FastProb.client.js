// Creates a node map by the fan-out/in vectors. 
// Biases are considered as input from their previous layer.
// 
var md = vv.GetObject("EvalMd"); // vv.SetObject("EvalMd", null);
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
  vv.SetObject("EvalMd", md);
}

vv.GuiManager.RememberCurrentMap();
var columnIdx = -1; //-1:49
var factor = 0.5; 
var shift = 0;
var X2 = md.Z.Prob(md.X, columnIdx, factor, shift); 
md.Eval(X2, true, vv.Dataset.BodyList);
vv.Map.Redraw();
