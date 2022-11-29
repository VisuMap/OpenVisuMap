//
// SaveDatset.js
// Save the sorted dataset with the trained MDS maps.
//
vv.Import("AtlasHelp.js");
ValidateHeatMap(pp);

function SaveMaps(hmItemId) {
	var atlas = OpenAtlas();	
	var mpSize = 40;
	var cItem = null;
	var gItem = null;
	var hmItem = atlas.FindItemById(hmItemId);

	//vv.Message("AA:" + cfg.cellMap.TheForm.IsDisposed + " | " + cfg.geneMap.TheForm.IsDisposed);

	if ( (cfg.cellMap != null) && !cfg.cellMap.TheForm.IsDisposed) {
		cItem = atlas.CaptureItem(cfg.cellMap);
		cItem.IconHeight = cItem.IconWidth = mpSize;
		cItem.Left = hmItem.Left + hmItem.IconWidth/2 - mpSize;
		cItem.Top = hmItem.Top + 12;
		cItem.Opacity = 1.0;
		cItem.Script = '!OpenMapItem(true)';
	}

	if ( (cfg.geneMap != null) && !cfg.geneMap.TheForm.IsDisposed) {
		gItem = atlas.CaptureItem(cfg.geneMap);
		gItem.IconHeight = gItem.IconWidth = mpSize;
		gItem.Left = hmItem.Left + hmItem.IconWidth/2;
		gItem.Top = hmItem.Top + 12;
		gItem.Opacity = 1.0;	
		gItem.Script = '!OpenMapItem(false)';
	}
	
	atlas.GroupItems( hmItem, cItem, gItem );
	atlas.Redraw();
}

SaveMaps(SaveSortedTable());
