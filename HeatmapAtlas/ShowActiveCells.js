//!import "AtlasHelp.js"
//
// ShowActiveCells.js
// Show active cells of selected genes.
//

ValidateHeatMap(pp);
CheckMaps();

function ShowActiveCells() {
	var expTable = cfg.hm.GetNumberTable();	
	var sp = NewExpressionMap(cfg.cellMap, "Active Cells");
	sp.Top = cfg.hm.Top - cfg.hm.Height + 8;
	sp.Left = cfg.hm.Left - cfg.hm.TheForm.ClientSize.Width;
	
	var bv = New.BarView(expTable.SelectColumns( New.IntArray(0) ));
	bv.Show();
	bv.Top = cfg.hm.Top + 14;
	bv.Left = cfg.hm.Left + cfg.hm.TheForm.ClientSize.Width + 1;
	bv.Width = host.toInt32(cfg.hm.Width*2/3);
	bv.Height = sp.Height - 10;
	bv.AutoScaling = false;
	bv.Horizontal = true;
	bv.Title = "Gene Expression Profile";
	bv.BaseLineType = 4;

	cs.SetRange(expTable, bv);
	bv.Redraw();
	
	sp.Tag = bv;
	cfg.hm.SelectionMode = 1;
	vv.EventManager.OnItemsSelected(
		"!cs.ShowActiveCells(vv.EventSource.Item, cfg.hm.GetNumberTable(), vv.EventSource.Argument);",
		sp, sp);

	FlushMarkers(cfg.geneMap, cfg.cellMap, sp);
}

ShowActiveCells();

