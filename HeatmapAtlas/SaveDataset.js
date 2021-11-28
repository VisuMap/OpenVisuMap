//!import "AtlasHelp.js"
//
// SaveDatset.js
// Save the sorted dataset with the trained MDS maps.
//

ValidateHeatMap(pp);
CheckMaps();

var dsName = "AtlasDs";
var desc = cfg.hm.Description;
if ( desc.startsWith("Data imported from:") ) {
  var i1 = desc.lastIndexOf("\\")+1;
  var i2 = desc.lastIndexOf(".");
  dsName = cfg.hm.Description.substring(i1, i2);
}

dsName = vv.PromptMessage("New Dataset Name", dsName);

if ( (dsName == null) || (dsName == "") )
  vv.Return();

dsName = cfg.hm.GetNumberTable().SaveAsDataset(dsName, desc);

if ( (dsName == null) || (dsName == "") ) {
  vv.Message("Failed to save dataset:" + vv.LastError);
  vv.Return();
}

vv.Folder.OpenDataset(dsName);
cfg.cellMap.ResetSize();
cfg.cellMap.CaptureMap();
vv.Map.Metric = cfg.cMtr;
cfg.cellMap.Close();

var atlas = New.Atlas("FeatureMaps");
atlas.Show();
cfg.geneMap.ResetSize();
var geneItem = atlas.NewSnapshotItem(cfg.geneMap);
geneItem.Id = dsName;
geneItem.Name = "HeatmapAtlas";
atlas.Close();
cfg.geneMap.Close();

cfg.hm.Close();
cfg.hm = cfg.cellMap = cfg.geneMap = null;