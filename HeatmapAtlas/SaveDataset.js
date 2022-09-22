//!import "AtlasHelp.js"
//
// SaveDatset.js
// Save the sorted dataset with the trained MDS maps.
//

ValidateHeatMap(pp);
CheckMaps();

var hmItemId = SaveSortedTable();

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
atlas.Redraw();

atlas.GroupItems( New.AtlasItemArray(cItem, gItem, hmItem) );
atlas.Close();