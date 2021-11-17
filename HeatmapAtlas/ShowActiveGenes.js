//!import "AtlasHelp.js"
//
// ShowActiveGenes.js
// Highlight active genes for selected cells.
//

ValidateHeatMap(pp);
CheckMaps();

function ShowActiveGenes() {
	var expTable = pp.GetNumberTable();	
	var sp = NewExpressionMap(cfg.geneMap, "Active Genes");
	sp.Top = cfg.geneMap.Top;
	sp.Left = cfg.cellMap.Left;
	
	var bv = New.BarView(expTable.SelectRows(New.IntArray(0)));
	bv.Show();
	bv.Top = pp.Top + pp.Height - 8;
	bv.Left = pp.Left + 24;
	bv.Width = pp.Width - 24;
	bv.Height = sp.Height/2;
	bv.AutoScaling = true;
	bv.Horizontal = false;
	bv.Title = "Cell Expression Profile";
	sp.Tag = bv;
	bv.Redraw();


	pp.SelectionMode = 0;
	vv.EventManager.OnItemsSelected(
		"!cs.ShowActiveGenes(vv.EventSource.Item, cfg.hm.GetNumberTable(), vv.EventSource.Argument);",
		sp, sp);

	FlushMarkers(cfg.cellMap, cfg.geneMap, sp);	
}

ShowActiveGenes();
