// File: BandPass.js
//
// Purpose: Filter the data of a value diagram view through a band-pass filter.
//   The low and high frequency limit will be controled by a interval-filter widget.
//
// Remarks: this script should be installed through the script SetWaveTransforms.js.
//
//
if ( pp.Name == "ValueDiagram" ) {
	var t = vv.FindPluginObject("WaveTransforms"); 
	var info = new Array();
	info.diagram = pp;
	info.table = pp.GetNumberTable().Duplicate();
    var dimension = info.table.Columns;

    switch(vv.EventSource.Item) {
        case "Fourier": info.transform = t.NewFourier(dimension); break;
        case "Haar": info.transform = t.NewHaar(dimension); break;
        case "Walsh": info.transform = t.NewWalsh(dimension); break;
        case "WaveletD4": info.transform = t.NewWaveletD4(dimension); break;
        case "PCA": info.transform = t.NewPca(info.table); break;
    }

	var filter = New.IntervalFilter();
    filter.Script = null;  // Avoid persistent script get called.
    filter.ValueMax = 1.0;
    filter.ValueMin = 0;
    filter.ValueHigh = 0.35;
    filter.ValueLow = 0.0;
    filter.Tag = info;
    filter.Orientation = 0;
    filter.NumberFormat = "f3";
    filter.Script = vv.CurrentScriptPath;
    filter.Show();
    
	RefreshDiagram(filter);	
} else if ( pp.Name == "IntervalFilter" ) {  // Assuming the caller is the track panel.
	RefreshDiagram(pp);
} else {
	vv.Message("This script can only be called from value diagram window.");
}

function RefreshDiagram(filter) {
	var info = filter.Tag;
	var newTable = info.transform.Filter(info.table, filter.ValueLow, filter.ValueHigh);
	info.diagram.SetNumberTable(newTable);
	info.diagram.Redraw();
}
