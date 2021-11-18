// DualSorting.js
// Context menu script to perform dual sorting on a heatmap.
//
vv.Import("AtlasHelp.js");

var doAllService = vv.ModifierKeys.ControlPressed;

ValidateHeatMap(pp);

function DSMain() {
	cfg.hm = pp;
	pp.DisableReorder = false;
	var dsTable = pp.GetNumberTable();
	
	pp.Title = 'Sorting Rows...';
	pp.SelectionMode = 0;
	SortTable(dsTable, cfg.cMtr, cfg.cEpochsSrt, cfg.cExaSrt, cfg.cPprSrt);
	
	pp.Title = 'Sorting Columns...';
	pp.SelectionMode = 1;
	var dsTable2 = dsTable.Transpose2();
	cs.ShiftTable(dsTable2, cfg.gPrShift);
	SortTable(dsTable2, cfg.gMtr, cfg.gEpochsSrt, cfg.gExaSrt, cfg.gPprSrt);
       dsTable2.FreeRef();
	pp.Title = 'Sorting Completed!';	
	pp.DisableReorder = true;
}

DSMain();

if ( doAllService ) {
  cfg.hm.ClickContextMenu("Atlas/Dual Embedding");
  cfg.hm.ClickContextMenu("Atlas/Dual Clustering");
  cfg.hm.ClickContextMenu("Atlas/Active Cells");
}
