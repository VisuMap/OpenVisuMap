// Test classification capabilities.
//
var K = 5;
var repeats = 100;
var idxList = New.IntRange(K);
var rIdx = 1;
function UpdateIndex(idx0) {
	for(var i=0; i<idxList.Count; i++) {
		idxList[i] = idx0 + ( (rIdx * 54367) % 512 );
		rIdx++;
	}
}

var md = vv.FindPluginObject("DMScript").NewLiveModel();
var input = vv.GetNumberTable();
var rsList = input.RowSpecList;
var csString = New.ClassType("System.String");

var mdList = [ "B.md", "C.md", "D.md" ] ;
//var mdList = ["A.md", "A1.md", "A2.md", "A3.md"] ;

for ( var idx in mdList ) {
	var mdName = mdList[idx];
	pp.TheForm.SelectModel(mdName);
	pp.TheForm.StartServer();
	vv.Sleep(5000);
	
	var misses = 0;	
	vv.Echo2(mdName + ': misses: ')
	for(var n=0; n<12; n++) {
		var mapType = input.RowSpecList[n*512].Type;
		var err = 0;
		for(var r=0; r<repeats; r++) {
			UpdateIndex(n*512);
			var inData = input.SelectRows(idxList);
			var ret = md.Eval(inData.Matrix, true);	
			if ( mapType != ret[0] )
				err += 1;
		}
		vv.Echo2(csString.Format("{0,4:###0}", err) + ", ");
		misses += err;
	}
	vv.Echo("  Total: " + misses + ", ");
}