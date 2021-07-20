// File: Install.js
// 
// Purpose: This script installs the WaveTransforms plugin. 
//
// Usage: Start VisuMap, then press F5 and select this file to execute. 
//
//

var dir = vv.CurrentScriptPath.substr(0, vv.CurrentScriptPath.lastIndexOf("\\") + 1);

vv.InstallPlugin("Wave Transforms", dir +"WaveTransforms.dll");

for(var t in New.StringArray("Fourier", "Haar", "Walsh", "WaveletD4", "PCA") ) {
  vv.GuiManager.SetCustomMenu("Band-passing by/" + t, true, dir + "BandPass.js", "ValueDiagram");
  vv.GuiManager.SetCustomMenu("Filtering by/" + t, true, dir + "WaveFilter.js", "<All>");
}	  
