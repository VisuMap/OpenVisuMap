// File: TrackingRpm.js
//
// Description: This script produces a series of RPM maps with descreasing 
//   weight for a selected attribute. The maps will be recorded by a ClipRecorder window
//   that allows replay of the maps.
//
//  Usage: Load a dataset; set the variable attrIdx and N in this script; then run this 
//    this script.
//
var attrIdx = 0;  // The column index of the selected attribute.
var N = 20;       // Number of maps.

var pca = New.PcaView().Show();
var nt = pca.GetNumberTable();
var m = nt.Matrix;
var rows = nt.Rows;
var A = New.NumberArray();
for(var row=0; row<rows; row++)  A.Add(m[row][attrIdx]);
var plugin = vv.FindPluginObject("ClipRecorder");
var cr = plugin.NewRecorder().Show();
cr.TrackingTrend = true;
vv.Map.GlyphType = "VectorField";

for(var i=0; i<=N; i++) {
  var f = (N - i)*1.0/N;
  for(var row=0; row<rows; row++)  m[row][attrIdx] = f * A[row];
  pca.ResetView();
  pca.CaptureMap();
  cr.CreateSnapshot();
}

pca.Close();
cr.ClipTitle = "PCA tracking";
cr.TrackingTrend = false;
