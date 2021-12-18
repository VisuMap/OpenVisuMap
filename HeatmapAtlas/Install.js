// Install Single-Cell-Atlas related menus mark and run the following statements
//
var mgr = vv.GuiManager;
var icon = vv.CurrentScriptDirectory + "\\MenuIcon.png";

for(var [label, script, view, img] of [ 
  ["Dual Sorting", 		"DualSorting.js", 	"HeatMap", icon],
  ["Dual Embedding",		"DualEmbedding.js",	"HeatMap", null],
  ["Dual Clustering",	"DualClustering.js", "HeatMap", null],
  ["Active Genes",		"ShowActiveGenes.js","HeatMap", null],
  ["Active Cells",		"ShowActiveCells.js","HeatMap", null],
  ["Save Data",		"SaveDataset.js",	"HeatMap", null],
  ["Compare Maps",		"MapMorph.js",	"MainForm|MapSnapshot", icon],
  ["Variation Tracing",	"VariationTracing.js","MainForm", null],
]) mgr.SetCustomMenu("Atlas/"+label, true, script, view, img);

for(var [label, img, script] of [
	["Import Loom", 	icon, "LoomRead.pyn"],
	["Import H5AD", 	null, "H5adRead.pyn"],
	["Import H5", 	null, "H5Read.pyn"],
	["Import Matrix", 	null, "MatrixRead.pyn"], 
       ["Show Maps",		null, "ShowMaps.js"],
]) mgr.SetCustomButton("Atlas/"+label, img, script);

