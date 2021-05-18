// File: UnInstall.js
//
// Purpose: Uninstall the Multivariate Analysis plugin.
//
// Usage: Start VisuMap, then press F5 and select this file to execute. 
//
vv.RemovePlugin("MVA Plugin");
vv.GuiManager.RemoveCustomMenu("Do LDA");
vv.GuiManager.RemoveCustomMenu("Do PLS");
vv.GuiManager.RemoveCustomMenu("Do CCA");
vv.GuiManager.RemoveCustomMenu("Do PCA");
vv.GuiManager.RemoveCustomMenu("Do Projection");
