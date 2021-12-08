// File: Install.js
// 
// Purpose: This script installs the WaveTransforms plugin. 
//
// Usage: Start VisuMap, then press F5 and select this file to execute. 
//
//
var dir = vv.CurrentScriptDirectory;
var mgr = vv.GuiManager;
vv.InstallPlugin("Wave Transforms", dir+"\\WaveTransforms.dll");

for(var t of ["Fourier", "Haar", "Walsh", "WaveletD4", "PCA"] ) {
  mgr.SetCustomMenu("Band-passing by/" + t, true, "BandPass.js", "ValueDiagram");
  mgr.SetCustomMenu("Filtering by/" + t, true, "WaveFilter.js", "<All>");
}	  
