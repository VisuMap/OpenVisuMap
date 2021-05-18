// File: DoLDA.js
//
// Description: Perform LDA analysis on selected data. 
// 
// Usage: This script offers services to do multi-class LDA
//   and 2-class LDA. In order to do multi-class LDA open
//   data view; marker data points of different classes by different
//   (glyph) types; then select a set of data and choose the context menu
//   that executes this script.
//
//   In order to do 2-class LDA, first select a subset of data points and
//   mark them as "High lighted" through the context menu "Change Attribute>Highlight Bodies";
//   then select another subset of data points and choose the context menu
//   that executes this script.
//
//   The LDA projection matrix learned by this operation will be saved into a named
//   objected (with name "ProjectionBase"). You can apply this projection to any selected
//   number table through the DoProjection menu.
//
// Remarks: this script should only be installed through the script SetupMva.js
// and called through the context menu.
// 
// If the Control key is pressed when executing this script, more details of the
// LDA analysis will be displayed. 
//
var moreDetails = vv.ModifierKeys.ControlPressed;
var tb = pp.GetSelectedNumberTable();
if ( tb.Rows < 2 ) {
  vv.Message("Not enough data selected!");
  vv.Return(0);
}

var binaryLDA = false;

for(var rs in tb.RowSpecList) {
  if ( rs.Highlighted ) {
    binaryLDA = true;
    break;
  }
}
if ( binaryLDA ) {
    // If there are highlited bodies, we do binary class LDA
    for(var rs in tb.RowSpecList) {
      rs.Type = rs.Highlighted ? 15 : 11; 
    }
}

var mva = vv.FindPluginObject("MultivariateAnalysis");
if ( mva == null ) {
  vv.Message("MultivariateAnlysis plugin not installed!");
  vv.Return(1);
}

var ret = mva.DoLDA(tb)

vv.SetObject("ProjectionBase", ret.EigenVectors.Transpose2());

ShowProjection(ret.Projection);

if ( moreDetails ) {
  var bb = New.BarBand(ret.EigenVectors.SelectRows(New.IntArray(0, 1, 2, 3, 4)));
  bb.BaseLineType = 0;
  bb.Title = "First 5 New Bases (eigenvectors)"
  bb.Show();
}


function ShowProjection(proj) {
  var vw = proj.ShowCartesianView();
  if ( binaryLDA ) { 
    vw.DecoupleView();
    for(var rs in proj.RowSpecList) {
      rs.Highlighted = false;
    }
  }

  vw.ResetView();
  vw.Title += " - LDA Projection";
}
