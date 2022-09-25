//
// SaveDatset.js
// Save the sorted dataset with the trained MDS maps.
//
vv.Import("AtlasHelp.js");
ValidateHeatMap(pp);
CheckMaps();

function SaveMaps(hmItemId) {
	var atList = vv.FindFormList("Atlas");
	var atlas = (atList.Count>0) ? atList[0] : New.Atlas().Show();
	
	var cItem = atlas.CaptureItem(cfg.cellMap);
	var gItem = atlas.CaptureItem(cfg.geneMap);
	var hmItem = atlas.FindItemById(hmItemId);
	cItem.IconHeight = hmItem.IconHeight;
	cItem.IconWidth = hmItem.IconWidth;
	gItem.IconHeight = hmItem.IconHeight;
	gItem.IconWidth = hmItem.IconWidth;
	var w2 = hmItem.IconWidth/2 + 1;
	cItem.Left = hmItem.Left - w2;
	gItem.Left = hmItem.Left + w2;
	cItem.Top = gItem.Top = hmItem.Top + 10;
	gItem.Opacity = cItem.Opacity = 0.75;

	cItem.Script = `!vv.Import("AtlasHelp.js");
		var mp = vv.EventSource.Item.Open();
		mp.AddContextMenu('Atlas/Capture Coloring', 
			'!cs.CopyType(pp, pp.BodyList, cfg.hm)', 
			true, null, 'Push the cluster coloring to the heatmap');
		mp.Left = cfg.hm.Left - mp.Width + 15;
		mp.Top = cfg.hm.Top;		
		mp.ClickContextMenu('Atlas/Capture Coloring');
		cfg.cellMap = mp;		
		`;
	gItem.Script = `!vv.Import("AtlasHelp.js");
		var mp = vv.EventSource.Item.Open();
		mp.AddContextMenu('Atlas/Capture Coloring', 
			'!cs.CopyType(pp, pp.BodyList, cfg.hm)', 
			false, null, 'Push the cluster coloring to the heatmap');
		mp.Left = cfg.hm.Left;
		mp.Top = cfg.hm.Top - mp.Height + 8;
		mp.ClickContextMenu('Atlas/Capture Coloring');
		cfg.geneMap = mp;
		`;
	
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
	itemRow.Script = `!vv.Import("AtlasHelp.js");`;
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
	itemCol.Script = `!vv.Import("AtlasHelp.js");`;
	sp.Close();
	
	atlas.GroupItems( hmItem, cItem, gItem, itemRow, itemCol );
	*/

	atlas.GroupItems( hmItem, cItem, gItem );
	atlas.Redraw();
}

SaveMaps(SaveSortedTable());
