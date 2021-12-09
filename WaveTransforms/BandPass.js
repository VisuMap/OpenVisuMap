//MenuLabels Fourier Haar Walsh WaveletD4 PCA
// File: BandPass.js
//
// Purpose: Filter the data of a value diagram view through a band-pass filter.
//   The low and high frequency limit will be controled by a interval-filter widget.
//
// Remarks: this script should be installed through the script SetWaveTransforms.js.
//
//
if ( (pp.Name == "ValueDiagram") || (pp.Name== "HeatMap") ) {
    var t = vv.FindPluginObject("WaveTransforms"); 	
    var dataTable = pp.GetNumberTable();
    var dimension = dataTable.Columns;
    var transform = null;

    switch(vv.EventSource.Item) {
        case "Fourier": transform = t.NewFourier(dimension); break;
        case "Haar": transform = t.NewHaar(dimension); break;
        case "Walsh": transform = t.NewWalsh(dimension); break;
        case "WaveletD4": transform = t.NewWaveletD4(dimension); break;
        case "PCA": transform = t.NewPca(dataTable); break;
    }
    var transTable = transform.Filter(dataTable, 0, 0.35); // Low-Band: [0, 0.35] out from [0, 1.0]
    pp.SetNumberTable(transTable);
    pp.Redraw();
} else {
    vv.Message("This script can only be called from value diagram window.");
}
