//!import "AtlasHelp.js"
//
// DualClustering.js
// Cluster the rows and columns of a number table of the parent data view.
//

ValidateHeatMap(pp);
CheckMaps();

function DoClustering(map, minSize, minPoint) {
	// Setup context menu to synchronize clusters with the heatmap.
	map.AddContextMenu("Atlas/Capture Coloring", 
		"!cs.CopyType(pp, pp.BodyList, cfg.hm)", 
		(map == cfg.cellMap), null, "Push the cluster coloring to the heatmap");
	map.ClusterAlgorithm = 1;
	map.MinClusterSize = minSize;
	map.HdMinPoints = minPoint;
	map.ClusterNoise = true;
	map.DoDataClustering();
	return map.Clusters;
}

function DCMain() {
	cfg.hm = pp;
	var nt = cfg.hm.GetNumberTable();

	var rowClusters = DoClustering(cfg.cellMap, cfg.cMinSize, cfg.cMinPoint);
	cs.NormalizeColoring(cfg.cellMap.BodyList, cfg.RowSrtKeys, rowClusters);
	cfg.cellMap.ClickContextMenu("Atlas/Capture Coloring");

	var colClusters = DoClustering(cfg.geneMap, cfg.gMinSize, cfg.gMinPoint);
	cs.NormalizeColoring(cfg.geneMap.BodyList, cfg.ColumnSrtKeys, colClusters);
	cfg.geneMap.ClickContextMenu("Atlas/Capture Coloring");

	cfg.hm.Title = "Row/Column Clusters: " + rowClusters + "/" + colClusters;
	
	//cfg.hm.ClickContextMenu("Utilities/Sort Columns on Type");
	//cfg.hm.ClickContextMenu("Utilities/Sort Rows on Type");
}

DCMain();
