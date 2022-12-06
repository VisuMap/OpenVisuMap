// DualSorting.js
// Context menu script to perform dual sorting on a heatmap.
//
vv.Import("AtlasHelp.js");

ValidateHeatMap(pp);

cfg = {...cfg, ...{
	hm:null,
	EpochsSrt: PP(5000, 5000),
	ExaSrt:    PP(6,  4),
	PprSrt:    PP(0.025, 0.025),
	MtrSrt:    PP(cfg.cos, cfg.cos),
   PrShSrt:   PP(0, 0.5),      
}};

function SortTable(T, mt, epochs, ex, pr) {
	var tsne = New.TsneSorter(T, mt);
	tsne.MaxLoops = epochs;
	tsne.InitExaggeration = ex;
	tsne.PerplexityRatio = pr;
	tsne.RefreshFreq = cfg.refFreq;
	tsne.StagedTraining = false;
	tsne.Repeats = 1;
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
	cfg.hm.RandomizeRows().RandomizeColumns();
	var dsTable = pp.GetNumberTable();
	
	cfg.hm.Title = 'Sorting Rows...';
	cfg.hm.SelectionMode = 0;
	
	var dsTable1 = dsTable;
	if ( (cfg.MtrSrt.c == cfg.cos ) && (cfg.PrShSrt.c != 0) ) {
		dsTable1 = dsTable.Clone();
		csFct.ShiftTable(dsTable1, cfg.PrShSrt.c);
	}

	SortTable(dsTable1, cfg.MtrSrt.c, cfg.EpochsSrt.c, cfg.ExaSrt.c, cfg.PprSrt.c);

	cfg.hm.Title = 'Sorting Columns...';
	cfg.hm.SelectionMode = 1;
	var dsTable2 = dsTable.Transpose2();
	if ( (cfg.MtrSrt.g == cfg.cos)  && (cfg.PrShSrt.c != 0) )
		csFct.ShiftTable(dsTable2, cfg.PrShSrt.g);
	SortTable(dsTable2, cfg.MtrSrt.g, cfg.EpochsSrt.g, cfg.ExaSrt.g, cfg.PprSrt.g);

	dsTable2.FreeRef();
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

