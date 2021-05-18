// NewRandomSeq.js
if ( pp.Name == "HeatMap" ) {
	var nt = pp.GetNumberTable();
	var rg = pp.GetSelection();	

	for(var row=rg.Top; row<rg.Bottom; row++) {
		var R = nt.Matrix[row];
		for(var col=rg.Left; col<rg.Right; col++) {
			var k = rg.Left + Math.random() * rg.Width;
			var tmp = R[col];
			R[col] = R[k];
			R[k] = tmp;
		}
	}

	pp.Redraw();
	vv.Return(0);
}
var ds = vv.Dataset;
var sm = vv.FindPluginObject("SeqAnalysis");
var blob = sm.CreateSequenceBlob("RandomSeq");
var N = 8697635;
for(var n=0; n<N; n++) {
	var v = Math.random();
	var k = (v<0.293) ? 0 :
		   (v<0.5 ) ? 1 :
		   (v<0.70) ? 2 : 3;
	blob.AddLetter(k);
}
blob.Dispose();

