// File: ApplyModel.js
//============================
//MenuLabels CheckShapeModel CheckColorModel Check2Models ShapeWithNoise CheckWholeSpace CheckWholeSpace KnockOutFeaturs

var ms = vv.FindPluginObject("DMScript");
var nt = pp.GetSelectedNumberTable();
if ( nt.Rows * nt.Columns == 0 ) nt = pp.GetNumberTable();

// Call the function selected via the menu labels.
eval(vv.EventSource.Item+"()");

//===========================================================

function CheckShapeModel() {
	var bList = ms.ApplyModel2("ShapeModel", nt);
	New.MapSnapshot2(bList, vv.Map).Show().ResetSize();
}

function CheckColorModel() {
	var bList = ms.ApplyModel2("ColorModel", nt);
	for(var b in bList) {
		var b2 = vv.Dataset.BodyForId(b.Id);
		if ( b2 != null ) {
			b.X = b2.X;
			b.Y = b2.Y;
		}
	}
	New.MapSnapshot2(bList, vv.Map).Show().ResetSize();
}

function Check2Models() {
	var bList = ms.ApplyModel2("ShapeModel", nt);
	ms.ApplyModel2("ColorModel", nt, bList);
	New.MapSnapshot2(bList, vv.Map).Show().ResetSize();
}


function ShapeWithNoise() {
	for(var k=0; k<6; k++) nt.Append(nt);
	for(var row=0; row<nt.Rows; row++)
	for(var col=0; col<nt.Columns; col++)	
		    nt.Matrix[row][col] *= 1 + 0.2 *(2*Math.random() - 1);
	var bList = ms.ApplyModel2("ShapeModel", nt);
	New.MapSnapshot2(bList, vv.Map).Show().ResetSize();
}

function CheckWholeSpace() {
	nt.AddRows(500000);
	for(var row=0; row<nt.Rows; row++)
	for(var col=0; col<nt.Columns; col++) 
	    nt.Matrix[row][col] = 15*Math.random();
	var bList = ms.ApplyModel2("ShapeModel", nt);
	ms.ApplyModel2("ColorModel", nt, bList);
	New.MapSnapshot2(bList, vv.Map).Show().ResetSize();
}

function KnockOutFeaturs() {
	var V = 5;
	for(var k=0; k<8; k++) {
		for(var row=0; row<nt.Rows; row++)
		    nt.Matrix[row][V] = 2*k;
		
		var bList = ms.ApplyModel2("ShapeModel", nt);
		//ms.ApplyModel2("ColorModel", nt, bList);
		
		var map = New.MapSnapshot2(bList, vv.Map);
		map.Show().ResetSize();
		map.Title ="V: " + V + "   K: " + k;
	}
	vv.TileAllWindows();
	vv.GuiManager.ReuseLastWindow = false;
}


