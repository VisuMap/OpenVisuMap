// File: TransformClip.js
//
// Description: This script, when called by the ClipRecorder window, will transform
//   all the 3D frames in the current clip with a 4x4 transformation Matrix.
//
if ( pp.FrameList.Count < 1 ) {
  vv.Message("No clip recorded");
  vv.Return();
}

var f0 = pp.FrameList[0];

//
// We use the SlimDX library to construct the transformation matrix.
//
var mx = New.ClassType("SlimDX.Matrix");  
var t = mx.Translation(f0.MapWidth/2.0, f0.MapHeight/2.0, f0.MapDepth/2.0);  
var ax = 0;
var ay = 0;
var az = 0;
var s = 0;
var rotation = mx.RotationX(ax*0.01) * mx.RotationY(ay*0.01) *  mx.RotationZ(az*0.01);

var scale = mx.Scaling(Math.pow(1.1, s), Math.pow(1.1, s), Math.pow(1.1, s));
var m = mx.Invert(t) * rotation * scale  * t;

// We only support transformation of 3D Cube maps.
for(var i=0; i<pp.FrameList.Count; i++) pp.FrameList[i].MapType = 103;

pp.TrackingTrend = true; 
vv.Map.GlyphType = "VectorField";

pp.Transform(m.ToArray());
pp.ShowFrame(pp.CurrentFrame);


