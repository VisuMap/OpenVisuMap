var ms = vv.FindPluginObject("DMScript");

var md = vv.GetObject("TestClient");
if (md == null) {
    md = ms.NewLiveModel();
    vv.SetObject("TestClient", md); // vv.SetObject("TestClient", null);

    md.RequestTimeout = 20*1000;
    md.X = vv.Dataset.GetNumberTableEnabled();
    md.X.AddColumns(2);
    md.Y = New.Map3DView(vv.Dataset.BodyListEnabled(), null).Show();
    md.Z = New.CsObject("\
	public void Matrix2BodyList(IList<IBody> bs, double[][] matrix, double f){\
		for(int row=0; row<matrix.Length; row++) {\
		    bs[row].X = f*matrix[row][0];\
		    bs[row].Y = f*matrix[row][1];\
		    bs[row].Z = f*matrix[row][2];\
		}\
	}\
	public void SetArgument1(double[][] matrix, double delta){\
	    foreach(double[] R in matrix) {\
		    R[3] = delta;\
	    }\
	}\
	public void SetArgument2(double[][] matrix, int idx, double velocity, double delta){\
	    double a = velocity * delta;\
	    double sinA = Math.Sin(a);\
	    double cosA = Math.Cos(a);\
	    foreach(double[] R in matrix) {\
		    R[idx] = cosA;\
		    R[idx + 1] = sinA;\
	    }\
	}\
	public void SetArgument3(double[][] matrix, double idx, int jobIndex){\
	    double velocity = 0.1 * (1+jobIndex); \
	    double x = Math.Sin(velocity * idx);\
	    double y = Math.Sin(velocity * (idx+1));\
	    double z = Math.Sin(velocity * (idx+2));\
	    foreach(double[] R in matrix) {\
		    R[3] = x;\
		    R[4] = y;\
		    R[5] = z;\
	    }\
	}\
   ");
}

var input = md.X;
var map = md.Y;

//md.TestEval(input.Matrix, map);

var cs = md.Z;
//var rd =  vv.FindPluginObject("ClipRecorder").OpenRecorder(map);
var velocity = vv.EventSource.Item - 0.0;

function MapData(t) {
	cs.SetArgument2(input.Matrix, input.Columns - 2, velocity, t);
	md.Eval(input.Matrix, true, map.BodyList);
	map.Redraw();
	map.Title = "Delta: " + t.ToString("g4");
	//rd && rd.AddSnapshot(map.BodyList);
	vv.Sleep(0);
}

if ( vv.EventSource.Argument == null ) {
	for (var delta = -2.0; delta<=12+2; delta+=0.1) 
		MapData(delta);
} else {
	// called when scrolling mouse-wheel:
	MapData(10.188);
}
