//!import "AtlasHelp.js"
//
// ShowActiveGenes.js
// Highlight active genes for selected cells.
//

ValidateHeatMap(pp);
CheckMaps();

function ShowActiveGenes() {
	var expTable = cfg.hm.GetNumberTable();	
	var sp = NewExpressionMap(cfg.geneMap, "Active Genes");
	sp.Top = cfg.geneMap.Top;
	sp.Left = cfg.cellMap.Left;
	
	var bv = New.BarView(expTable.SelectRows(New.IntArray(0)));
	bv.Show();
	bv.Top = cfg.hm.Top + cfg.hm.Height - 8;
	bv.Left = cfg.hm.Left + 24;
	bv.Width = cfg.hm.Width - 24;
	bv.Height = host.toInt32(sp.Height/2);
	bv.AutoScaling = true;
	bv.Horizontal = false;
	bv.Title = "Cell Expression Profile";
	sp.Tag = bv;
	bv.Redraw();


	cfg.hm.SelectionMode = 0;
	vv.EventManager.OnItemsSelected(
		"!cs.ShowActiveGenes(vv.EventSource.Item, cfg.hm.GetNumberTable(), vv.EventSource.Argument);",
		sp, sp);

	FlushMarkers(cfg.cellMap, cfg.geneMap, sp);	
}

ShowActiveGenes();
