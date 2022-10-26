//
// DualClustering.js
// Cluster the rows and columns of a number table of the parent data view.
//
vv.Import("AtlasHelp.js");
ValidateHeatMap(pp);
CheckMaps();

cfg = {...cfg, ...{
	DbMinPoint: PP(30, 30),   	// for DBSCAN
	Epsilon:    PP(2.0, 3.0),  // for DBSCAN
	Alg:        PP(0,	0),      // 0: for DBSCAN; 1: for HDBSCAN.
	MinPoint:   PP(15, 15),    // for HDBSCAN       
	MinSize: 	PP(40, 40),    // for HDBSCAN
}};


function DoClustering(map, alg, minSize, minPoint, epsilon, dbMinPoint) {
	// Setup context menu to synchronize clusters with the heatmap.
	map.AddContextMenu("Atlas/Capture Coloring", "!csFct.CopyType(pp.BodyList, cfg.hm)", 
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
	cfg.cellMap.ClickMenu("Atlas/Capture Coloring");

	var colClusters = DoClustering(cfg.geneMap, cfg.Alg.g, cfg.MinSize.g, cfg.MinPoint.g, cfg.Epsilon.g, cfg.DbMinPoint.g);
	cfg.geneMap.ClickMenu("Atlas/Capture Coloring");

	cfg.hm.Title = "Row/Column Clusters: " + rowClusters + "/" + colClusters;
}

DCMain();
