//!import "AtlasHelp.js"
//
// ShowActiveCells.js
// Show active cells of selected genes.
//

ValidateHeatMap(pp);

function ShowActiveCells() {
	var expTable = pp.GetNumberTable();
	var [cellMap, geneMap] = FindCellGeneMap();
	
	var sp = NewExpressionMap(cellMap, "Active Cells");
	sp.Top = pp.Top - pp.Height + 8;
	sp.Left = pp.Left - pp.TheForm.ClientSize.Width;
	
	var bv = New.BarView(expTable.SelectColumns( New.IntArray(0) ));
	bv.Show();
	bv.Top = pp.Top + 14;
	bv.Left = pp.Left + pp.TheForm.ClientSize.Width + 1;
	bv.Width = pp.Width*2/3;
	bv.Height = sp.Height - 10;
	bv.AutoScaling = false;
	bv.Horizontal = true;
	bv.Title = "Gene Expression Profile";
	bv.BaseLineType = 4;

	cs.SetRange(expTable, bv);
	bv.Redraw();
	
	sp.Tag = bv;
	pp.SelectionMode = 1;
	vv.EventManager.OnItemsSelected(
		"!cs.ShowActiveCells(vv.EventSource.Item, cfg.hm.GetNumberTable(), vv.EventSource.Argument);",
		sp, sp);

	FlushMarkers(geneMap, cellMap, sp);
}

ShowActiveCells();

