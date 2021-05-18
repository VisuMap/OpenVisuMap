// File: MakeGLyphSets.js
//
// Description: This script provides the service to create and install 
//   glyph sets with different sizes and colors.
//
var gm = vv.FindPluginObject("CustomGlyphSets");
if ( gm == null ) {
  vv.Message("CustomGlyphSets plugin not installed.");
  vv.Return(0);
}

var glyphSetName = "My Glyph Set";   // The name of the new glyph set.
var opacity = 0.15;  // The opacity of the glyphs, must be a value between 0 and 1.0;
var level = 4;       // The number of different glyph sizes. Must be an integer larger than 1.
var classes = 12;    // The number of different glyph colors. Should be an integer between 1 and 12.
var minSize = 2;     // The diameter in pixels of smallest glyph.

//
// Creates and installs the glyphset. The glyphset will contain levels*classes glyphs.
//
var errMsg;

errMsg = gm.CreateOrderedGlyphs(glyphSetName, opacity, minSize, level, classes);

/*
Replace above stmt by the following block to create phase glyphset:
var phases = 15; 
var size = 16;
errMsg = gm.CreatePhaseGlyphs(glphSetName, phases, size, New.Color("Red"));
*/

/*
Replace above stmt by the following block to create color-wheel glyphset:
var colors = 128;
var size = 5;
var isSquare = true;
gm.CreateColorWheelGlyphs(glyphSetName, colors, size, isSquare);
*/

if ( errMsg != null ) {
  vv.Message("Error: " + errMsg);
} else {
  vv.Message("New glyph set \"" + glyphSetName + "\" created!");
}