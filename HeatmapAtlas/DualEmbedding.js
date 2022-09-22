//
// DualEmbedding.js
// Create t-SNE embedding for rows and columns of heatmap.
//
vv.Import("AtlasHelp.js");
ValidateHeatMap(pp);

function RunEmbedding(mds, epochs, mtr, initExa, ppRatio, is3D) {
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
	return mpView;
};

var nt = vv.GetNumberTable();

function DEmbeddingMain() {
	var nt = cfg.hm.GetNumberTable();
	var mds = New.MdsCluster(nt);
	mds.Show();

	if (cfg.cMtr == mtrs.cos)
		mds.SetTrainingData(cs.ShiftTable(nt.Clone(), cfg.cPrShift));
	
	cfg.cellMap = RunEmbedding(mds, cfg.cEpochs, cfg.cMtr, cfg.cExa, cfg.cPpr, cfg.cIs3D);

	var nt2 = nt.Transpose2();
	if (cfg.gMtr == mtrs.cos) 
		nt2 = cs.ShiftTable(nt2, cfg.gPrShift);
	
	mds.SetTrainingData(nt2);

	cfg.geneMap = RunEmbedding(mds, cfg.gEpochs, cfg.gMtr, cfg.gExa, cfg.gPpr, cfg.gIs3D);
	nt2.FreeRef();
	mds.Close();

	var sz = 450;
	var winWidth = sz;
	var winHeight = sz;
	cfg.hm.TheForm.SetBounds(1000, 700, winWidth, winHeight);
	cfg.cellMap.TheForm.SetBounds(cfg.hm.TheForm.Left - sz + 15, cfg.hm.TheForm.Top, sz, sz);
	cfg.geneMap.TheForm.SetBounds(cfg.hm.TheForm.Left, cfg.hm.TheForm.Top - sz + 8, sz, sz);
	cfg.cellMap.Title = "Cell Map";
	cfg.geneMap.Title = "Gene Map";
}

DEmbeddingMain();
