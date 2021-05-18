// File: Install.js
//
// Purpose: Install the Multivariate Analysis plugin.
//
// Usage: Start VisuMap, then press F5 and select this file to execute. 
//
var dir = vv.CurrentScriptPath.substr(0, vv.CurrentScriptPath.lastIndexOf("\\") + 1);

vv.InstallPlugin("MVA Plugin", dir +"MultivariateAnalysis.dll", true);
vv.GuiManager.SetCustomMenu("Do LDA", true, dir + "DoLDA.js", "<All>");
vv.GuiManager.SetCustomMenu("Do PLS", true, dir + "DoPLS.js", "<All>");
vv.GuiManager.SetCustomMenu("Do CCA", true, dir + "DoPLS.js", "<All>");
vv.GuiManager.SetCustomMenu("Do PCA", true, dir + "DoPCA.js", "<All>");
vv.GuiManager.SetCustomMenu("Do Projection", true, dir + "DoProjection.js", "<All>");
