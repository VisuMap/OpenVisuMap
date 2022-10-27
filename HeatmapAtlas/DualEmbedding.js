//
// DualEmbedding.js
// Create t-SNE embedding for rows and columns of heatmap.
//
vv.Import("AtlasHelp.js");
ValidateHeatMap(pp);

cfg = {...cfg, ...{
	Epochs:	PP(5000,  5000),    // training epochs for cell/gene profiles.
	Exa:		PP(4,     4),      // initial exaggreation
	Ppr:		PP(0.1,   0.1),      // perplexity ratio    
	PrShift:	PP(1.0,   1.0),      // cell/gene profile shift towards arithmetric center.
	Mtr:		PP(cfg.cos, cfg.cos),
	Is3D:		PP(false, false),
	cellMap:null, 
	geneMap:null,
}};

function RunEmbedding(mds, nt, isCellMap, epochs, mtr, initExa, ppRatio, is3D) {
	mds.SetTrainingData(nt);
	mds.Is3D = is3D;
	mds.Metric = mtr;
	mds.AutoClustering = false;
	mds.AutoNormalizing = false;
	mds.StagedTraining = true;
	mds.RefreshFreq = cfg.refFreq;
	mds.Repeats = 1;
	mds.GlyphSet = "36 Clusters|36 Clusters|36 Clusters";

	mds.MaxLoops = epochs;
	mds.PerplexityRatio = ppRatio;
	mds.ExaggerationFactor = initExa;
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
	
	var nt1 = nt;
	if ( (cfg.Mtr.c == cfg.cos) && (cfg.PrShift.c!=0) )
		nt1 = csFct.ShiftTable(nt1.Clone(), cfg.PrShift.c);
	cfg.cellMap = RunEmbedding(mds, nt1, true, cfg.Epochs.c, cfg.Mtr.c, cfg.Exa.c, cfg.Ppr.c, cfg.Is3D.c);

	var nt2 = nt.Transpose2();
	if ( (cfg.Mtr.g == cfg.cos) && (cfg.PrShift.g!=0) )
		nt2 = csFct.ShiftTable(nt2, cfg.PrShift.g);	
	cfg.geneMap = RunEmbedding(mds, nt2, false, cfg.Epochs.g, cfg.Mtr.g, cfg.Exa.g, cfg.Ppr.g, cfg.Is3D.g);

	nt2.FreeRef();
	mds.Close();

	var sz = 600;
	var winWidth = sz;
	var winHeight = sz;
	cfg.hm.TheForm.SetBounds(1000, 700, winWidth, winHeight);
	cfg.cellMap.TheForm.SetBounds(cfg.hm.TheForm.Left - sz + 15, cfg.hm.TheForm.Top, sz, sz);
	cfg.geneMap.TheForm.SetBounds(cfg.hm.TheForm.Left, cfg.hm.TheForm.Top - sz + 8, sz, sz);
	cfg.cellMap.BackgroundColor = New.Color(0, 0, 64);
	cfg.geneMap.BackgroundColor = New.Color(0, 64, 64);
	cfg.geneMap.Refresh();
	cfg.cellMap.Refresh();
}

DEmbeddingMain();
