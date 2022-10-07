//
// DualClustering.js
// Cluster the rows and columns of a number table of the parent data view.
//
vv.Import("AtlasHelp.js");
ValidateHeatMap(pp);
CheckMaps();

function DoClustering(map, alg, minSize, minPoint, epsilon, dbMinPoint) {
	// Setup context menu to synchronize clusters with the heatmap.
	map.AddContextMenu("Atlas/Capture Coloring", 
		"!csFct.CopyType(pp, pp.BodyList, cfg.hm)", 
		(map == cfg.cellMap), null, "Push the cluster coloring to the heatmap");
	map.ClusterAlgorithm = alg;
	map.MinClusterSize = minSize;
	map.HdMinPoints = minPoint;
	map.ClusterNoise = true;
	map.EpsilonRatio = epsilon;
	map.MinClusterPoint = dbMinPoint;
	map.DoDataClustering();
	return map.Clusters;
}

function DCMain() {
	cfg.hm = pp;
	var nt = cfg.hm.GetNumberTable();

	var rowClusters = DoClustering(cfg.cellMap, cfg.Alg.c, cfg.MinSize.c, cfg.MinPoint.c, cfg.Epsilon.c, cfg.DbMinPoint.c);
	cfg.cellMap.ClickContextMenu("Atlas/Capture Coloring");

	var colClusters = DoClustering(cfg.geneMap, cfg.Alg.g, cfg.MinSize.g, cfg.MinPoint.g, cfg.Epsilon.g, cfg.DbMinPoint.g);
	cfg.geneMap.ClickContextMenu("Atlas/Capture Coloring");

	cfg.hm.Title = "Row/Column Clusters: " + rowClusters + "/" + colClusters;
}

DCMain();
