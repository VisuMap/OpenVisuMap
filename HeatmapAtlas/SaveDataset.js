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
	var w2 = hmItem.IconWidth/2.0 ;
	cItem.Left = hmItem.Left - w2;
	gItem.Left = hmItem.Left + w2;
	cItem.Top = gItem.Top = hmItem.Top + 10;
	gItem.Opacity = cItem.Opacity = 1.0;

	cItem.Script = '!OpenMapItem(true)';
	gItem.Script = '!OpenMapItem(false)';
	
	atlas.GroupItems( hmItem, cItem, gItem );
	atlas.Redraw();
}

SaveMaps(SaveSortedTable());
