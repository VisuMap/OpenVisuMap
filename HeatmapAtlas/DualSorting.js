// DualSorting.js
// Context menu script to perform dual sorting on a heatmap.
//
vv.Import("AtlasHelp.js");

var doAllService = vv.ModifierKeys.ControlPressed;

ValidateHeatMap(pp);

function DSMain() {
	cfg.hm = pp;
	cfg.hm.DisableReorder = false;
	var dsTable = pp.GetNumberTable();
	
	cfg.hm.Title = 'Sorting Rows...';
	cfg.hm.SelectionMode = 0;
	SortTable(dsTable, cfg.cMtr, cfg.cEpochsSrt, cfg.cExaSrt, cfg.cPprSrt);
	
	cfg.hm.Title = 'Sorting Columns...';
	cfg.hm.SelectionMode = 1;
	var dsTable2 = dsTable.Transpose2();
	cs.ShiftTable(dsTable2, cfg.gPrShift);
	SortTable(dsTable2, cfg.gMtr, cfg.gEpochsSrt, cfg.gExaSrt, cfg.gPprSrt);
       dsTable2.FreeRef();
	cfg.hm.Title = 'Sorting Completed!';	
	cfg.hm.DisableReorder = true;
}

DSMain();

if ( doAllService ) {
  cfg.hm.ClickContextMenu("Atlas/Dual Embedding");
  cfg.hm.ClickContextMenu("Atlas/Dual Clustering");
  cfg.hm.ClickContextMenu("Atlas/Active Cells");
}
