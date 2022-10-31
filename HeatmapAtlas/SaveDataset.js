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
	var mpSize = 40;
	cItem.IconHeight = cItem.IconWidth = gItem.IconHeight = gItem.IconWidth = mpSize;
	gItem.Left = hmItem.Left + hmItem.IconWidth/2;
	cItem.Left = gItem.Left - mpSize;
	cItem.Top = gItem.Top = hmItem.Top + 12;
	gItem.Opacity = cItem.Opacity = 1.0;

	cItem.Script = '!OpenMapItem(true)';
	gItem.Script = '!OpenMapItem(false)';
	
	atlas.GroupItems( hmItem, cItem, gItem );
	atlas.Redraw();
}

SaveMaps(SaveSortedTable());
