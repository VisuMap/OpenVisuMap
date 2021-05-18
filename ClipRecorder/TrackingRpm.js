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
var initialVariation = 200;
var learningSpeed = 20;
var coolingSpeed = 0.2;

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
//cr.TrackingTrend = true;
//map.GlyphType = "VectorField";

var rpm = New.RpmMap().Show();
rpm.CoolingSpeed = coolingSpeed;
rpm.RefreshFreq = 100;

for(var i=0; i<=N; i++) {
  f.Factor[attrIdx] = (N-i)*1.0/N; 
  f.Save();
  map.SetMetricAndFilter(map.Metric, filterName);

  if ( i==0 ) {
    rpm.InitialVariation = initialVariation;
    rpm.LearningSpeed = initialVariation;
  } else {
    var damp = 0.2;
    rpm.InitialVariation = damp * initialVariation;
    rpm.LearningSpeed = damp * learningSpeed;
  }

  rpm.Start();
  map.CentralizeAt();
  cr.CreateSnapshot();  
}

rpm.InitialVariation = 200;
rpm.LearningSpeed = rpm.InitialVariation/20;
rpm.Close();

// Reset the filter.
f.Factor[attrIdx] = 1.0;
f.Save();
map.SetMetricAndFilter(map.Metric, filterName);

cr.ClipTitle = "RPM Tracking";
cr.TrackingTrend = false;
