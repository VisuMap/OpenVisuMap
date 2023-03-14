//
// DualEmbedding.js
// Create t-SNE embedding for rows and columns of heatmap.
//
vv.Import("AtlasHelp.js");
ValidateHeatMap(pp);

cfg = {...cfg, ...{
	Epochs:PP(2000),    // training epochs for cell/gene profiles.
	Exa:   PP(10),     // initial exaggreation
	ExaF:  PP(1.5),    // final exaggeration.
	Ppr:   PP(0.05),   // perplexity ratio    
	Mtr:   PP(cfg.cos), // distance metric
 	PrShift:PP(0),      // cell/gene profile shift towards arithmetric center.
	Is3D:	PP(false, false),
	cellMap:null, 
	geneMap:null,
	hpSize: 400,
}};


/*
cfg.Exa = PP(3.0);
cfg.ExaF = PP(1.5);
cfg.Ppr = PP(0.05);
cfg.Mtr = PP(cfg.cor);
*/

function RunEmbedding(mds, nt, isCellMap, epochs, mtr, initExa, finalExa, ppRatio, is3D) {
	mds.SetTrainingData(nt);
	mds.Is3D = is3D;
	mds.Metric = mtr;
	mds.AutoClustering = false;
	mds.AutoNormalizing = false;
	mds.StagedTraining = false;
	mds.RefreshFreq = cfg.refFreq;
	mds.Repeats = 1;
	mds.GlyphSet = "36 Clusters|36 Clusters|36 Clusters";

	mds.MaxLoops = epochs;
	mds.PerplexityRatio = ppRatio;
	mds.ExaggerationFactor = initExa;
	mds.ExaggerationFinal = finalExa;
	mds.Reset().Start();
	if ( mds.LoopsTsne != mds.MaxLoops ) {
		vv.GuiManager.StopFlag = true;
		vv.Return();
	}

	var mpView = mds.Is3D ? mds.Show3DView() : mds.Show2DView();

	mpView.NormalizeView();
	mpView.AddContextMenu("Atlas/Capture Coloring", "!csFct.CopyType(pp.BodyList, cfg.hm)",
      isCellMap, null, "Push the cluster coloring to the heatmap");
	mpView.Title = isCellMap ? "Cell Map" : "Gene Map";
	return mpView;
};

function DEmbeddingMain() {
	var nt = cfg.hm.GetNumberTable();
	var mds = New.MdsCluster().Show();
	cfg.cellMap = cfg.geneMap = null;

	var sz = cfg.hpSize;
	cfg.hm.TheForm.SetBounds(1000, 700, sz, sz);
	
	if (cfg.Epochs.c > 0) {
		var nt1 = nt;
		if ( (cfg.Mtr.c == cfg.cos) && (cfg.PrShift.c!=0) )
			nt1 = csFct.ShiftTable(nt1.Clone(), cfg.PrShift.c);
		cfg.cellMap = RunEmbedding(mds, nt1, true, cfg.Epochs.c, cfg.Mtr.c, cfg.Exa.c, cfg.ExaF.c, cfg.Ppr.c, cfg.Is3D.c);

		cfg.cellMap.TheForm.SetBounds(cfg.hm.TheForm.Left - sz + 15, cfg.hm.TheForm.Top, sz, sz);
		cfg.cellMap.BackgroundColor = New.Color(0, 0, 64);
		cfg.cellMap.Refresh();
	}

	if (cfg.Epochs.g > 0) {
		var nt2 = nt.Transpose2();
		if ( (cfg.Mtr.g == cfg.cos) && (cfg.PrShift.g!=0) )
			nt2 = csFct.ShiftTable(nt2, cfg.PrShift.g);	
		cfg.geneMap = RunEmbedding(mds, nt2, false, cfg.Epochs.g, cfg.Mtr.g, cfg.Exa.g, cfg.ExaF.g, cfg.Ppr.g, cfg.Is3D.g);
		nt2.FreeRef();

		cfg.geneMap.TheForm.SetBounds(cfg.hm.TheForm.Left, cfg.hm.TheForm.Top - sz + 8, sz, sz);
		cfg.geneMap.BackgroundColor = New.Color(0, 64, 64);
		cfg.geneMap.Refresh();
	}

	mds.Close();
}

DEmbeddingMain();
