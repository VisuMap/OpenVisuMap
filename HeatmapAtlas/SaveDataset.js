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
	
	atlas.GroupItems( hmItem, cItem, gItem );
	atlas.Redraw();
}

SaveMaps(SaveSortedTable());
