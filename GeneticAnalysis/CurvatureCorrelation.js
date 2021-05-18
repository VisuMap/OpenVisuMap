// File: CurvatureCorrelation.js
//
var filter = vv.Folder.OpenSequence2Filter(vv.Map.Filter);
var gTable = vv.Folder.ReadDataset("Chr1CDS.fa");
var sm = vv.FindPluginObject("SeqAnalysis").OpenSequence("Chr1CDS.fa");
var geneList = New.StringArray();
for(var b in gTable.BodyList) geneList.Add(b.Id);

var gId4K = "ENSG00000271723";  // 4005
var gId1K = "ENSG00000187513";  // 1002
//var refGeneId = "ENSG00000158481";  // 1002
var refGeneId = "Id2223"; // 1002
var gId300 = "ENSG00000187170";  // 300
var gId150 = "ENSG00000278883";  // 150
var gId26K = "ENSG00000154358";   // 26K
var refGene = FetchSeq(refGeneId);

//var idx0 = geneList.IndexOf("ENSG00000143641") + 1;
//for(var n=idx0; n<geneList.Count; n++) {
for(var id in geneList)	{
	//var id = geneList[n];
	DoTest(refGene, id);	
	if ( vv.GuiManager.StopFlag ) break;
}

//SaveHeatMap();
//DoTest(refGene, gId4K);

//====================================================

function DoTest(refGene, idEnv) {
	var seqEnv = FetchSeq(idEnv);
	filter.Sequence = (refGene == null) ? 
		seqEnv : (refGene + "\r\n\r\n" + seqEnv);
	filter.StepSize = 0;
	filter.Save();
	vv.Map.SetMetricAndFilter(vv.Map.Metric, vv.Map.Filter);

	if ( filter.StepSize >= 0 ) { return;}

	var op = New.AffinityEmbedding();
	//var op = New.TsneMap();
	op.TheForm.WindowState = 1;
	op.Is3D = true;
	op.CoolingSpeed = 0.2;
	//op.MaxLoops = 400;
     op.Show();

	for(var k=0; k<1; k++) {
		op.Reset();
		op.Start();
		CalculateCurvature(idEnv+"_" + k);
	}

	op.Close();
}

function FetchSeq(id) {
	var idx = gTable.IndexOfRow(id);
	var seqIdx = gTable.GetDataAt(idx, 0) - 0;
	var seqLen = gTable.GetDataAt(idx, 1) - 0;
	return sm.FetchSeq(seqIdx, seqLen);
}

function SaveHeatMap() {
	var fm = vv.FindFormList("HeatMap");  
	if ( fm.Count > 0 ) {
		var atlas = New.Atlas().Show();
		atlas.Items.Add(atlas.NewHeatMapItem(fm[0]));
		atlas.Close();
		fm[0].Close();
		fm[0].Title = "GeneId: " + refGeneId;
	}
}

function CalculateCurvature(idEnv) {
	var b = New.BodyList();
	for(var body in vv.Dataset.BodyList) 
		if( !body.Disabled && (body.Type == 0) ) b.Add(body);

	var cv = New.NumberArray();
	var idList = New.StringArray();
	var scanPerNode = -filter.StepSize;
	var N = scanPerNode; // the curvature resolution.
	var skips = scanPerNode;

	for(var i=0; i<(b.Count - 2*N - 1); i+=skips) {
		var iN = i+N;
		var iN2 = i+2*N;

		var x0 = b[iN].X - b[i].X;
		var y0 = b[iN].Y - b[i].Y;
		var z0 = b[iN].Z - b[i].Z;

		var x1 = b[iN2].X - b[iN].X;
		var y1 = b[iN2].Y - b[iN].Y;
		var z1 = b[iN2].Z - b[iN].Z;

	
		var x2 = x0+x1;
		var y2 = y0+y1;
		var z2 = z0+z1;

		// K = 2*sin(A)/s, 
		// where A is the angle between B[k+1]-B[k] and B[k+2]-B[k+1], 
		// s is the length of B[k+2]-B[k].

		var n0 = x0*x0+y0*y0+z0*z0;
		var n1 = x1*x1+y1*y1+z1*z1;
		var n2 = x2*x2+y2*y2+z2*z2;
		if ( (n0>0) && (n1>0) && (n2>0) ) {
			var dot = x0*x1+y0*y1+z0*z1;
			var sinA2 = 1 - (dot*dot)/(n0*n1);   // sinA2 = sin(A)^2.
			if (  sinA2>0 ) {
				cv.Add(2*Math.sqrt(sinA2/n2));
			}
		} else {
			cv.Add(0)
		}
		idList.Add(b[iN].Id);
	}

	// Add the data to a heatmap.
	var fm = vv.FindFormList("HeatMap");  
	if ( fm.Count == 0 ) {
		var tb = New.NumberTable(cv.ToArray());
		for(var col=0; col<tb.Columns; col++) tb.ColumnSpecList[col].Id = idList[col];
		tb.RowSpecList[0].Type = 0;
	     tb.RowSpecList[0].Id = idEnv;
		var hm = tb.ShowHeatMap();
		hm.ReadOnly = true;
		hm.SpectrumType = 2;
	} else {	
		var tb = fm[0].GetNumberTable();
		if ( tb.Columns < cv.Count ) tb.AddColumns( cv.Count - tb.Columns );
		tb.AddRows(1);
		for(var col=0; col<cv.Count; col++) tb.Matrix[tb.Rows-1][col] = cv[col];
		tb.RowSpecList[tb.Rows-1].Type = 0;
		tb.RowSpecList[tb.Rows-1].Id = idEnv;
		fm[0].Title = "N: " + tb.Rows;
		fm[0].Redraw();
	}
}


function Randomize(s) {
	var s = s.Trim().split("");
	for(var i=0; i<s.length; i++) {
	       if ( (s[i]=='\r') || ( s[i]=='\n' ) ) continue;	
		var j = Math.floor(Math.random() * s.length);	
	       if ( (s[j]=='\r') || ( s[j]=='\n' ) ) continue;	
		var temp = s[i];
		s[i] = s[j];
		s[j] = temp;
	}
     return s.join("");	
}
