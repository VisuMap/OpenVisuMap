// File: DimShrink.js
//
// Description: This script runs RPM algorithm with 
//  shrinking map depths to archieve dimensionality reduction.
//
var initSize = 700;
var coolingSpeed = 0.1;
var stopSize = 10;
var stopVariation = 0.2;

var m = vv.Map;
m.Width = m.Depth = m.Height = initSize;
m.RedrawMap();

var rpm = New.RpmMap().Show();
rpm.CoolingSpeed = coolingSpeed;
rpm.StopVariation = stopVariation;
rpm.Is3D = true;
rpm.RefreshFreq = 100;
rpm.Reset();

var plugin = vv.FindPluginObject("ClipRecorder");
var cr = plugin.NewRecorder().Show();
cr.TrackingTrend = false;
cr.Recording = false;

var i = 0;
while(m.Depth > stopSize) {
  if ( i++ == 0 ) {
    rpm.InitialVariation = 200;
    rpm.LearningSpeed = 10;
  } else {
    rpm.InitialVariation = 5;
    rpm.LearningSpeed = 2;
  }

  rpm.Start();
  m.CentralizeAt();
  cr.CreateSnapshot();
  rpm.Title = i+ ": " + m.Depth + ":" + rpm.Variation.ToString("f1");
  m.ScaleMap(1.0, 1.0, 0.95);
}

rpm.Close();
cr.ClipTitle = "RPM 3D-2D Dimension Reduction";
