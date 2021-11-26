// Install Single-Cell-Atlas related menus mark and run the following statements
//
var mgr = vv.GuiManager;
var imgPath = vv.CurrentScriptDirectory + "\\MenuIcon.png";
var scrPath = "!if (vv.AtlasManager.OpenMap('FeatureMaps', vv.Dataset.Name) == null)vv.Message('No Feature Map Found');";

for(var [label, script, view, img] of [ 
	["Dual Sorting", "DualSorting.js", "HeatMap", imgPath],
	["Dual Embedding", "DualEmbedding.js", "HeatMap", null],
	["Dual Clustering", "DualClustering.js", "HeatMap", null],
	["Active Genes", "ShowActiveGenes.js", "HeatMap", null],
	["Active Cells", "ShowActiveCells.js", "HeatMap", null],
	["Save Data", "SaveDataset.js", "HeatMap", null],
	["Compare Maps", "MapMorph.js", "MapSnapshot", imgPath],
])
	mgr.SetCustomMenu("Atlas/" + label, true, script, view, img);

for(var [label, img, script] of [
	["Import Loom", imgPath, "LoomRead.pyn"],
	["Import H5AD", null, "H5adRead.pyn"],
	["Import H5", null, "H5Read.pyn"],
	["Import Matrix", null, "MatrixRead.pyn"], 
       ["Show Feature Map", null, scrPath],
]) 
	mgr.SetCustomButton("Atlas/" + label, img, script);
