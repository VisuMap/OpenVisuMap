//!import "AtlasHelp.js"
//
// DualClustering.js
// Cluster the rows and columns of a number table of the parent data view.
//

ValidateHeatMap(pp);

function DoClustering(map, minSize, minPoint) {
	// Setup context menu to synchronize clusters with the heatmap.
	map.AddContextMenu("Atlas/Capture Coloring", 
		"!cs.CopyType(pp, pp.BodyList, cfg.hm)", 
		null, null, "Push the cluster coloring to the heatmap");
	map.ClusterAlgorithm = 1;
	map.MinClusterSize = minSize;
	map.MinClusterPoint = minPoint;
	map.DoDataClustering();
	return map.Clusters;
}

function DCMain() {
	cfg.hm = pp;
	var nt = cfg.hm.GetNumberTable();
	var [cellMap, geneMap] = FindCellGeneMap();

	var rowClusters = DoClustering(cellMap, cfg.cMinSize, cfg.cMinPoint);
	cs.NormalizeColoring(cellMap.BodyList, cfg.RowSrtKeys, rowClusters);
	cellMap.ClickContextMenu("Atlas/Capture Coloring");

	var colClusters = DoClustering(geneMap, cfg.gMinSize, cfg.gMinSize);
	cs.NormalizeColoring(geneMap.BodyList, cfg.ColumnSrtKeys, colClusters);
	geneMap.ClickContextMenu("Atlas/Capture Coloring");

	cfg.hm.Title = "Row/Column Clusters: " + rowClusters + "/" + colClusters;
	
	//cfg.hm.ClickContextMenu("Utilities/Sort Columns on Type");
	//cfg.hm.ClickContextMenu("Utilities/Sort Rows on Type");
}

DCMain();
