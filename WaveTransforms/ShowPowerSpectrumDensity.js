// File: ShowPowerSpectrumDensity.js
// 
// Purpose: Show the power spectrumn density of the selected data.
//
// Usage: Select a number table in a data view, then choose the context
//   menu to execute this script.
//

var nt = pp.GetSelectedNumberTable();
if ( (nt == null) || (nt.Rows ==0) || (nt.Columns==0) ) {
  vv.Message("No data selected.");
  vv.Return(0);
}

var wt = vv.FindPluginObject("WaveTransforms"); 
var dim = 256;   // number of the densities.
var f = wt.NewFourier(2*dim);  // Each density is calculated by 2 Fourier coefficient.
var fc = f.Transform(nt);

var psd = New.NumberTable(fc.Rows, dim);
var repeats = fc.Columns/dim/2;    // How many times the transformation has been repeated on each row.
var factor = 1.0/Math.PI/repeats;
var m = psd.Matrix;

for(var row=0; row<fc.Rows; row++) {
  var Mrow = fc.Matrix[row];
  for(var col=0; col<fc.Columns; col++) {
    var c = Mrow[col];
    var col2 = col % (2*dim);
    if ( col2 < dim ) {
        m[row][col2] += factor * c * c;
    } else {
        m[row][col2-dim] += factor * c * c;
    }
  }
}

// plot the log scale of the density.
for(var row=0; row<psd.Rows; row++) {
  for(var col=0; col<psd.Columns; col++) {
    m[row][col] = Math.log(m[row][col]);
  }
}

for(var row=0; row<psd.Rows; row++) {
  psd.RowSpecList[row].Copy(nt.RowSpecList[row]);
}
psd.ShowValueDiagram();
