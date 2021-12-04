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
	cfg.cellMap = null;
}

function SaveMaps() {
       var dsName = vv.Dataset.Name;
	var atlas = New.Atlas("FeatureMaps").Show();

	cfg.geneMap.ResetSize();
	var geneItem = atlas.NewSnapshotItem(cfg.geneMap);
	geneItem.Id = dsName;
	geneItem.Name = "HeatmapAtlas";
	cfg.geneMap.Close();
	cfg.geneMap = null;
		
	cs.SyncKeyColoring(cfg.hm.GetNumberTable(), cfg.RowSrtKeys, cfg.ColumnSrtKeys);
	cfg.hm.Close();
	cfg.hm = null;

	var sp = New.SpectrumView(cfg.RowSrtKeys).Show();
	sp.NormalizeView();
	sp.Horizontal = false;
	sp.Width =70; sp.Height=450;
	var itemRow = atlas.NewSpectrumItem(sp);
	itemRow.Id = dsName + "_Row";
	itemRow.IconWidth = 20;
	itemRow.IconHeight = 200;
	sp.Close();

	sp = New.SpectrumView(cfg.ColumnSrtKeys).Show();
	sp.NormalizeView();
	sp.Horizontal = true;
	sp.Width =450; sp.Height=70;
	var itemCol = atlas.NewSpectrumItem(sp);
	itemCol.Id = dsName + "_Col";
	itemCol.IconWidth = 200;
	itemCol.IconHeight = 20;
	sp.Close();

	atlas.Close();
}

SaveDataset();
SaveMaps();


