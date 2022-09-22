//!import "AtlasHelp.js"
//
// SaveDatset.js
// Save the sorted dataset with the trained MDS maps.
//

ValidateHeatMap(pp);
CheckMaps();

function SaveMaps(hmItemId) {
	var atlas = New.Atlas();
	atlas.Show();
	
	var cItem = atlas.CaptureItem(cfg.cellMap);
	var gItem = atlas.CaptureItem(cfg.geneMap);
	var hmItem = atlas.FindItemById(hmItemId);
	cItem.IconHeight = hmItem.IconHeight;
	cItem.IconWidth = hmItem.IconWidth;
	gItem.IconHeight = hmItem.IconHeight;
	gItem.IconWidth = hmItem.IconWidth;
	cItem.Left = hmItem.Left + 10;
	cItem.Top = hmItem.Top + 10;
	gItem.Left = hmItem.Left + 20;
	gItem.Top = hmItem.Top + 20;

	cItem.Script = `!//!import "AtlasHelp.js"
		var mp = vv.EventSource.Item.Open();
		mp.AddContextMenu('Atlas/Capture Coloring', 
			'!cs.CopyType(pp, pp.BodyList, cfg.hm)', 
			true, null, 'Push the cluster coloring to the heatmap');
		mp.Left = cfg.hm.Left - mp.Width + 15;
		mp.Top = cfg.hm.Top;		
		mp.ClickContextMenu('Atlas/Capture Coloring');
		cfg.cellMap = mp;		
		`;
	gItem.Script = `!//!import "AtlasHelp.js"
		var mp = vv.EventSource.Item.Open();
		mp.AddContextMenu('Atlas/Capture Coloring', 
			'!cs.CopyType(pp, pp.BodyList, cfg.hm)', 
			false, null, 'Push the cluster coloring to the heatmap');
		mp.Left = cfg.hm.Left;
		mp.Top = cfg.hm.Top - mp.Height + 8;
		mp.ClickContextMenu('Atlas/Capture Coloring');
		cfg.geneMap = mp;
		`;


	atlas.Redraw();
	
	/*
	var sp = New.SpectrumView(cfg.RowSrtKeys).Show();
	sp.NormalizeView();
	sp.Horizontal = false;
	sp.Width =70; sp.Height=450;
	var itemRow = atlas.NewSpectrumItem(sp);
	itemRow.IconWidth = 15;
	itemRow.IconHeight = 60;
	itemRow.Top = hmItem.Top;
	itemRow.Left = hmItem.Left - 18;
	itemRow.Script = `!//!import "AtlasHelp.js"`;
	sp.Close();
	
	sp = New.SpectrumView(cfg.ColumnSrtKeys).Show();
	sp.NormalizeView();
	sp.Horizontal = true;
	sp.Width =450; sp.Height=70;
	var itemCol = atlas.NewSpectrumItem(sp);
	itemCol.IconWidth = 60;
	itemCol.IconHeight = 15;
	itemCol.Top = hmItem.Top - 18;
	itemCol.Left = hmItem.Left;
	itemCol.Script = `!//!import "AtlasHelp.js"`;
	sp.Close();
	
	atlas.GroupItems( hmItem, cItem, gItem, itemRow, itemCol );
	*/

	atlas.GroupItems( hmItem, cItem, gItem );
	atlas.Close();
}

SaveMaps(SaveSortedTable());