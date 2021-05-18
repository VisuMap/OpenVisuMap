var ms = vv.FindPluginObject("DMScript");

var md = vv.GetObject("TestClient");
if (md == null) {
    md = ms.NewLiveModel('', true, 8889);
    vv.SetObject("TestClient", md); 
    md.RequestTimeout = 20*1000;
    var input = vv.Dataset.GetNumberTableEnabled();
    md.Y = New.Map3DView(vv.Dataset.BodyListEnabled(), null).Show();
    md.Z = md.TestEvalInit(input.Matrix);
}

var panel = pp.TheForm.OpenInputPanel(md);

/*

var md = vv.GetObject("TestClient");
if ( md != null ) {
	if ( md.Z != null ) {
		md.TestEvalClose(md.Z);
		md.Z = null;
	}
	vv.SetObject("TestClient", null);
	if ( md.Y != null ) 
		md.Y.Close();
}

*/

