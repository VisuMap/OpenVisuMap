// File: Install.js
// 
// Purpose: This script installs the WaveTransforms plugin. 
//
// Usage: Start VisuMap, then press F5 and select this file to execute. 
//
//
vv.InstallPlugin("Wave Transforms", vv.CurrentScriptDirectory + "\\WaveTransforms.dll");
vv.GuiManager.SetCustomMenu("Band-passing by/*", true, "BandPass.js", "ValueDiagram|HeatMap");
vv.GuiManager.SetCustomMenu("Filtering by/*", true, "WaveFilter.js", "<All>");
