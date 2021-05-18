var ms = vv.FindPluginObject("DMScript");
var md = ms.NewLiveModel("A1");
md.StartModel();
md.Connect();

//md.ShowGraph();

var vName= vv.GuiManager.GetClipboard();
var d = md.ReadVariable("Layer_2/bias");
New.BarBand(New.NumberTable(d)).Show();


/*
var input = vv.GetNumberTable().Clone();
input.AddColumns(1);

var N = input.Rows;
var samples = 6;
var m = New.NumberTable(N*samples, 3).Matrix;

var delta = 0.5
for(var idx=0; idx<samples; idx++) {
    for(var row=0; row<input.Rows; row++)
	 input.Matrix[row][3] = idx + delta;
    var d = md.Eval(input.Matrix, false);

    if ( d == null ) {
       vv.Echo("Error: No data returned.");
	break;
    }

    for(var row=0; row<N; row++)
	m[idx*N+row] = d[row];
}

var nt = New.NumberTable(m);
for(var i=0; i<nt.Rows; i++){	
	nt.RowSpecList[i].Type = vv.Dataset.BodyList[i%N].Type;
	nt.RowSpecList[i].Id = "R" + i;
}
nt.ShowPcaView();
*/

// var d = md.ReadVariable("Layer_5/mx");

//vv.Echo(d.Length + " : " + d[0].Length);


//md.ShowGraph();

// md.Exec('a=1234')

//==============================================
/*
var nt = New.NumberTable(1,14);
var R = nt.Matrix[0];
R[0] = 238.3;
R[1] = 188.7;
R[2] = 255.4;
R[5] = 1.0;

R = md.Eval(nt.Matrix);
vv.Echo(R.Length + " " + R[0].Length);
*/

/*
var nt = vv.GetNumberTable().Clone();
nt.AddColumns(1);
var R = md.Eval(nt.Matrix, false);
vv.Echo(R.Length + ":" + R[0].Length);
*/

//==============================================
/*
var nt = vv.GetNumberTable();
//nt = nt.SelectRows(New.IntRange(230));
var R = md.Eval(nt.Matrix);

vv.Echo(R.Length + " " + R[0].Length);
vv.GuiManager.RememberCurrentMap();
var bs = vv.Dataset.BodyList;
var mismatches = 0;
for(var i=0; i<nt.Rows; i++) {
  var b = bs[i];
  b.X = R[i][0];
  b.Y = R[i][1];
  b.Z = R[i][2];
  var type = parseInt(R[i][3], 10);
  if ( b.Type != type ) {
     b.Type = type;
     mismatches++;
  }
}
vv.Map.Redraw();
vv.Folder.DataChanged = true;
vv.Echo("Mismatches: " + mismatches);
*/

/*
//==============================================
var ms = vv.FindPluginObject("DMScript");
var md = ms.NewLiveModel("A1");
md.Connect();
md.ShutdownModel();
*/


