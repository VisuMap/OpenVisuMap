//!import "AtlasHelp.js"
//
// SaveDatset.js
// Save the sorted dataset with the trained MDS maps.
//

ValidateHeatMap(pp);
CheckMaps();

function SaveDataset() {
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
	vv.Map.GlyphOpacity = 0.5;
	vv.Map.GlyphSize = 0.5;
	vv.Map.GlyphType= "36 Clusters|36 Clusters|36 Clusters";
	vv.Map.Redraw();
	cfg.cellMap.Close();
}
SaveDataset();

function SaveOrderKeys() {
	cs.SyncKeyColoring(cfg.hm.GetNumberTable(), cfg.RowSrtKeys, cfg.ColumnSrtKeys);
       var dsName = vv.Dataset.Name;
	var atlas = New.Atlas("OrderKey1D");
	atlas.Show();

	var spRow = New.SpectrumView(cfg.RowSrtKeys).Show();
	spRow.NormalizeView();
	spRow.Horizontal = false;
	spRow.Width =70; spRow.Height=450;
	var itemRow = atlas.NewSpectrumItem(spRow);
	itemRow.Id = dsName + "_Row";

	var spCol = New.SpectrumView(cfg.ColumnSrtKeys).Show();
	spCol.NormalizeView();
	spCol.Horizontal = true;
	spRow.Width =450; spRow.Height=70;
	var itemCol = atlas.NewSpectrumItem(spCol);
	itemCol.Id = dsName + "_Col";

	spRow.Close();
	spCol.Close();
	atlas.Close();
}
SaveOrderKeys();


function SaveFeatureMap() {
       var dsName = vv.Dataset.Name;
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
}
SaveFeatureMap();


