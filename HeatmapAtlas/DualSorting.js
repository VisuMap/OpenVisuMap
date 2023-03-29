// DualSorting.js
// Context menu script to perform dual sorting on a heatmap.
//
vv.Import("AtlasHelp.js");

ValidateHeatMap(pp);

cfg = {...cfg, ...{
	hm:null,
	EpochsSrt: PP(2000),
	ExaSrt:    PP(10),
	ExaSrtF:   PP(1.5),
	PprSrt:    PP(0.05),
	MtrSrt:    PP(cfg.cos),
   PrShSrt:   PP(0, 0),
   SrtLimit:  PP(0, 0),
}};

/*
cfg.ExaSrt = PP(5.0);
cfg.ExaSrtF = PP(1.25);
cfg.PprSrt = PP(0.1);
cfg.MtrSrt = PP(cfg.cor);
cfg.SrtLimit = PP(400, 50000);
*/

function SortTable(T, mt, epochs, ex, exF, pr) {
	var tsne = New.TsneSorter(T, mt);
	tsne.MaxLoops = epochs;
	tsne.InitExaggeration = ex;
	tsne.FinalExaggeration = exF;
	tsne.PerplexityRatio = pr;
	tsne.RefreshFreq = cfg.refFreq;
	tsne.StagedTraining = false;
	tsne.Repeats = 1;
	tsne.Broadcasting = true;
	tsne.Show().Start();
	if (isNaN(tsne.ItemList[0].Value)) {
		vv.Message("Training degraded!\nPlease try with smaller initial exaggeration.");
		vv.Return(1);
	}
	if ( tsne.CurrentLoops != tsne.MaxLoops) {
		vv.GuiManager.StopFlag = true;
		vv.Return();
	}
	tsne.Close();
};


function DSortMain() {
	cfg.hm = pp;
	cfg.hm.DisableReorder = false;
	cfg.hm.Title = 'Sorting Rows...';
	cfg.hm.SelectionMode = 0;
	cfg.hm.RandomizeRows();
	var dsTable = cfg.hm.GetNumberTable();	
	var ds1 = dsTable;
	ds1 = SqueezeFeatures(ds1, cfg.SrtLimit.c);

	if ( (cfg.MtrSrt.c == cfg.cos ) && (cfg.PrShSrt.c != 0) ) {
		ds1 = dsTable.Clone();
		csFct.ShiftTable(ds1, cfg.PrShSrt.c);
	}


	SortTable(ds1, cfg.MtrSrt.c, cfg.EpochsSrt.c, cfg.ExaSrt.c, cfg.ExaSrtF.c,  cfg.PprSrt.c);

	cfg.hm.Title = 'Sorting Columns...';
	cfg.hm.SelectionMode = 1;
	cfg.hm.RandomizeColumns();
	var ds2 = cfg.hm.GetNumberTable().Transpose2();
   ds2 = SqueezeFeatures(ds2, cfg.SrtLimit.g);

	if ( (cfg.MtrSrt.g == cfg.cos)  && (cfg.PrShSrt.g != 0) )
		csFct.ShiftTable(ds2, cfg.PrShSrt.g);
	SortTable(ds2, cfg.MtrSrt.g, cfg.EpochsSrt.g, cfg.ExaSrt.g, cfg.ExaSrtF.g, cfg.PprSrt.g);

	ds2.FreeRef();
	cfg.hm.Title = 'Sorted';	
	cfg.hm.DisableReorder = true;
}

var doAll = vv.ModifierKeys.ControlPressed;
DSortMain();

if ( doAll ) {  // doAll = false;
  cfg.hm.ClickMenu("Atlas/Dual Embedding");
  //cfg.hm.ClickMenu("Atlas/Dual Clustering");
  //cfg.hm.ClickMenu("Atlas/Save Data");
  //cfg.hm.ClickMenu("Atlas/Active Cells");
}
