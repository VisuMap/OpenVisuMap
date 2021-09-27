//!import "AtlasHelp.js"
//
// ShowActiveGenes.js
// Highlight active genes for selected cells.
//

ValidateHeatMap(pp);

function ShowActiveGenes() {
	var expTable = pp.GetNumberTable();
	var [cellMap, geneMap] = FindCellGeneMap();
	
	var sp = NewExpressionMap(geneMap, "Active Genes");
	sp.Top = geneMap.Top;
	sp.Left = cellMap.Left;
	
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

	FlushMarkers(cellMap, geneMap, sp);	
}

ShowActiveGenes();
