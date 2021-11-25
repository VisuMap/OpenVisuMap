// Install Single-Cell-Atlas related menus mark and run the following statements
//
for(var [label, script, view] of [ 
	["Dual Sorting", "DualSorting.js", "HeatMap"],
	["Dual Embedding", "DualEmbedding.js", "HeatMap"],
	["Dual Clustering", "DualClustering.js", "HeatMap"],
	["Active Genes", "ShowActiveGenes.js", "HeatMap"],
	["Active Cells", "ShowActiveCells.js", "HeatMap"],	
	["Save Data", "SaveDataset.js", "HeatMap"],
	["Compare Maps", "MapMorph.js", "MapSnapshot"],
])
	vv.GuiManager.SetCustomMenu("Atlas/" + label, 
		true, vv.CurrentScriptDirectory + "/" + script, view);

for(var [label, img, script] of [
	["Import Loom", vv.CurrentScriptDirectory + "\\MenuIcon.png", "LoomRead.pyn"],
	["Import H5AD", null, "H5adRead.pyn"],
	["Import H5", null, "H5Read.pyn"],
	["Import Matrix", null, "MatrixRead.pyn"] 
]) 
	vv.GuiManager.SetCustomButton("Atlas/" + label, img, script);
