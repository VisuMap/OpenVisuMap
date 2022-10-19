// DualSorting.js
// Context menu script to perform dual sorting on a heatmap.
//
vv.Import("AtlasHelp.js");

var doAll = vv.ModifierKeys.ControlPressed;
ValidateHeatMap(pp);

cfg = {...cfg, ...{
	EpochsSrt: PP(5000, 5000),
	ExaSrt:    PP(10,	  6),
	PprSrt:    PP(0.15,  0.1),
	MtrSrt:    PP(cfg.cos, cfg.cos),
   PrShSrt:   PP(1.0,  1.0),      
}};

function DSortMain() {
	cfg.hm = pp;
	cfg.hm.DisableReorder = false;
	var dsTable = pp.GetNumberTable();
	
	cfg.hm.Title = 'Sorting Rows...';
	cfg.hm.SelectionMode = 0;
	
	var dsTable1 = dsTable;
	if ( cfg.MtrSrt.c == cfg.cos ) {
		dsTable1 = dsTable.Clone();
		csFct.ShiftTable(dsTable1, cfg.PrShSrt.c);
	}

	SortTable(dsTable1, cfg.MtrSrt.c, cfg.EpochsSrt.c, cfg.ExaSrt.c, cfg.PprSrt.c);

	cfg.hm.Title = 'Sorting Columns...';
	cfg.hm.SelectionMode = 1;
	var dsTable2 = dsTable.Transpose2();
	if ( cfg.MtrSrt.g == cfg.cos )
		csFct.ShiftTable(dsTable2, cfg.PrShSrt.g);
	SortTable(dsTable2, cfg.MtrSrt.g, cfg.EpochsSrt.g, cfg.ExaSrt.g, cfg.PprSrt.g);

	dsTable2.FreeRef();
	cfg.hm.Title = 'Sorted';	
	cfg.hm.DisableReorder = true;
}

DSortMain();

if ( doAll ) {
  cfg.hm.ClickContextMenu("Atlas/Dual Embedding");
  cfg.hm.ClickContextMenu("Atlas/Dual Clustering");
  //cfg.hm.ClickContextMenu("Atlas/Save Data");
  //cfg.hm.ClickContextMenu("Atlas/Active Cells");
}

