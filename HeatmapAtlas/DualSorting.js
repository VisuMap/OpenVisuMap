// DualSorting.js
// Context menu script to perform dual sorting on a heatmap.
//
vv.Import("AtlasHelp.js");

var doAll = vv.ModifierKeys.ControlPressed;
ValidateHeatMap(pp);

function DSortMain() {
	cfg.hm = pp;
	cfg.hm.DisableReorder = false;
	var dsTable = pp.GetNumberTable();
	
	cfg.hm.Title = 'Sorting Rows...';
	cfg.hm.SelectionMode = 0;
	
	var dsTable1 = dsTable;
	if ( cfg.cMtr == mtrs.cos ) {
		dsTable1 = dsTable.Clone();
		cs.ShiftTable(dsTable1, cfg.cPrShift);
	}

	SortTable(dsTable1, cfg.cMtr, cfg.cEpochsSrt, cfg.cExaSrt, cfg.cPprSrt);

	cfg.hm.Title = 'Sorting Columns...';
	cfg.hm.SelectionMode = 1;
	var dsTable2 = dsTable.Transpose2();
	if ( cfg.gMtr == mtrs.cos )
		cs.ShiftTable(dsTable2, cfg.gPrShift);
	SortTable(dsTable2, cfg.gMtr, cfg.gEpochsSrt, cfg.gExaSrt, cfg.gPprSrt);

	dsTable2.FreeRef();
	cfg.hm.Title = 'Sorted';	
	cfg.hm.DisableReorder = true;
}

DSortMain();

if ( doAll ) {
  cfg.hm.ClickContextMenu("Atlas/Dual Embedding");
  cfg.hm.ClickContextMenu("Atlas/Dual Clustering");
  cfg.hm.ClickContextMenu("Atlas/Active Cells");
}

