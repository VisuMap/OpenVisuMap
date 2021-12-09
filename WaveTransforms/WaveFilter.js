//MenuLabels Fourier Haar Walsh WaveletD4 PCA
// File: WaveFilter.js
// 
// Purpose: filter the number table of a data view by one of wave alike transformation.
//
// Usage: Select some data point in data view, then choose one of the "Filtering by"
//   context menu. The filtered result will be displayed in Value Diagram window.
//  
// Remarks: this script should be installed through the script SetWaveTransforms.js.
//
var lowFreq = 0;
var highFreq = 0.35;
var t = vv.FindPluginObject("WaveTransforms"); 
var table = pp.GetNumberTable();
var transform = null;

switch(vv.EventSource.Item) {
    case "Fourier": transform = t.NewFourier(table.Columns); break;
    case "Haar": transform = t.NewHaar(table.Columns); break;
    case "Walsh": transform = t.NewWalsh(table.Columns); break;
    case "WaveletD4": transform = t.NewWaveletD4(table.Columns); break;
    case "PCA": transform = t.NewPca(table); break;
}

if ( transform == null ) {
  vv.Message("Wave Transform plugin not properly installed.");
  vv.Return(-1);
}
  
var newTable = transform.Filter(table, lowFreq, highFreq);

if ( pp.Name == "HeatMap" ) 
  newTable.ShowHeatMap();
else
  newTable.ShowValueDiagram();
