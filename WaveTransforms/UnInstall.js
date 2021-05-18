// File: UnInstall.js
// 
// Purpose: This script uninstalls the WaveTransforms plugin. 
//
// Usage: Start VisuMap, then press F5 and select this file to execute. 
//
vv.RemovePlugin("Wave Transforms");
vv.GuiManager.RemoveCustomMenu("Band-passing by/");
vv.GuiManager.RemoveCustomMenu("Filtering by/");
