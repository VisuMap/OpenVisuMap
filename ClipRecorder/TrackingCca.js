// File: TrackingCca.js
//
// Description: This script produces a series of CCA maps with descreasing 
//   weight for a selected attribute. The maps will be recorded by a ClipRecorder window
//   that allows replay of the maps.
//
//  Usage: Load a dataset; set the variable attrIdx and N in this script; then run this 
//    this script.
//
var attrIdx = 0;  // The column index of the selected attribute.
var N = 20;       // Number of maps.

var filterName = "ProbFilter";
var f = vv.Folder.OpenTableFilter(filterName);
if ( f == null ) {
  f = vv.Folder.NewTableFilter(filterName, vv.Dataset.Columns);
  f.Save();
}

var map = vv.Map;
map.Filter = filterName;
var plugin = vv.FindPluginObject("ClipRecorder");
var cr = plugin.NewRecorder().Show();
map.GlyphType = "VectorField";
cr.TrackingTrend = true;

for(var i=0; i<=N; i++) {
  f.Factor[attrIdx] = (N-i)*1.0/N; 
  f.Save();
  map.SetMetricAndFilter(map.Metric, filterName);

  var cca = New.CcaMap().Show();
  if ( i == 0 ) {
    cca.MaxLoops = 40;
    cca.InitialRate = 0.99;
    cca.Reset();
  } else {
    cca.InitialRate = 0.3;
    cca.LambdaZero = 100;
    cca.MaxLoops = 20;
  }

  cca.Start();
  cr.CreateSnapshot();
  if ( i == N ){
    cca.InitialRate = 0.99;
    cca.MaxLoops = 10;
  }
  cca.Close();
}

// Reset the filter
f.Factor[attrIdx] = 1.0;
f.Save();
map.SetMetricAndFilter(map.Metric, filterName);

cr.ClipTitle = "CCA Tracking";
cr.TrackingTrend = false;

