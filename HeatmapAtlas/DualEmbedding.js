//
// DualEmbedding.js
// Create t-SNE embedding for rows and columns of heatmap.
//
vv.Import("AtlasHelp.js");
ValidateHeatMap(pp);

function RunEmbedding(mds, isCellMap, epochs, mtr, initExa, ppRatio, is3D) {
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
	if ( mds.LoopsTsne != mds.MaxLoops )
		vv.Return();

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

	/*
	var nt1 = nt1;
	if ( (cfg.Mtr.c == mtrs.cos) && (cfg.PrShift.c!=0) )
		nt1 = mds.SetTrainingData(csFct.ShiftTable(nt.Clone(), cfg.PrShift.c));
	mds.SetTrainingData(nt1);
	cfg.cellMap = RunEmbedding(mds, true, cfg.Epochs.c, cfg.Mtr.c, cfg.Exa.c, cfg.Ppr.c, cfg.Is3D.c);
	*/

	var nt2 = nt.Transpose2();
	if ( (cfg.Mtr.g == mtrs.cos) && (cfg.PrShift.g !=0) )
		nt2 = csFct.ShiftTable(nt2, cfg.PrShift.g);	
	mds.SetTrainingData(nt2);
	cfg.geneMap = RunEmbedding(mds, false, cfg.Epochs.g, cfg.Mtr.g, cfg.Exa.g, cfg.Ppr.g, cfg.Is3D.g);

	nt2.FreeRef();
	mds.Close();

	/*
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
	*/
}

DEmbeddingMain();
