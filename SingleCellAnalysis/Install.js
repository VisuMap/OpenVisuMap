var dir = vv.CurrentScriptPath.substr(0, vv.CurrentScriptPath.lastIndexOf("\\") + 1);
vv.SetProperty("SingleCell.Separation", "0.5", "A value between 0 and 1.0 to increas the sparation between cells and genes");
vv.SetProperty("SingleCell.CellScale", "1.0", "Scaling factor for cell clusters");
vv.SetProperty("SingleCell.GeneScale", "1.0", "Scaling factor for gene clusters");
vv.InstallPlugin("Single Cell Analysis", dir +"SingleCellAnalysis.dll", true);
vv.GuiManager.SetCustomMenu("SC-Utilities/*", true, dir + "Utilities.js", "<All>");
vv.GuiManager.SetCustomMenu("Morph Maps", true, dir + "MapMorph.js", "MainForm|MapSnapshot|MdsCluster");
vv.GuiManager.SetCustomMenu("Tracing Features", true, dir + "FeatureMap.js", "MapSnapshot|MdsCluster");

