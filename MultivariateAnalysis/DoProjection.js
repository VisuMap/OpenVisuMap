// File: DoProjection.js
//
// Description: This script projects selected data to a projection
// base created by "Do LDA" or "Do PCA" analysis. Normally, LDA or PCA
// are applied to a small sampling of data to determine (i.e. "learn")
// the best projection direction. Then, this script is invoked to 
// apply the projection to unknown data.
//
var base = vv.GetObject("ProjectionBase");
if ( base == null ) {
  vv.Message("Please first create projection base through \"Do LDA\" or \"Do PCA\" menu.");
  vv.Return();
}

var nt = pp.GetSelectedNumberTable();

var columns = New.StringArray();
for(var rs in base.RowSpecList) columns.Add( rs.Id );
nt = nt.SelectColumnsById( columns );

if ( base.Rows != nt.Columns ) {
  vv.Message("Invalid projection base!");
  vv.Return();
}

nt = nt.Multiply( base );
nt.ShowCartesianView();
nt.Project