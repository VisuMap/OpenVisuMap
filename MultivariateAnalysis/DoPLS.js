// File: DoPLS.js
//
// Description: Perform PLS or CCA analysis on selected data and high lighted data. 
//
// Usage: First marker a set of rows or columns as highlighted, then select
// another set of rows or columns; then execute this script through the context menu.
//
// Remarks: this script should only be installed through the script SetupMva.js
// and called through the context menu.
//
// If the Control key is pressed when executing this script, more details of the
// LDA analysis will be displayed. 
//

var moreDetails = vv.ModifierKeys.ControlPressed;
var nt = pp.GetNumberTable();

var hColumns = New.StringArray();  // highlighted columns
var hRows = New.StringArray();     // highlighted rows
for(var cs in nt.ColumnSpecList) if ( cs.Highlighted ) hColumns.Add[cs.Id];
for(var rs in nt.RowSpecList) if ( rs.Highlighted ) hRows.Add[rs.Id];

var groupA;
var groupB = pp.GetSelectedNumberTable();

if ( groupB.Rows < 1 ) {
  vv.Message("Not enough data selected!");
  vv.Return(-1);
}

if ( hColumns.Count > 0) {
  if ( groupB.Rows == nt.Rows ) {
    groupA = nt.SelectColumnsById( hColumns );  
  } else {
    vv.Message("Inconsistent data selected.");
    vv.Return(-2);
  }
} else if ( hRows.Count > 0 ) {
  if ( groupB.Columns == nt.Columns ) {
    groupA = nt.SelectRowsById( hRows );
    // When some rows are highlighted we will consider each
    // row as a random variable. So, we transpose the two data tables here.
    // If we want use columns as random variables, we just
    // need to comment out the following two lines. In this case,
    // both tables must have the same number of rows, this means you must
    // highlight and select the same number of data points.
    groupA.Transpose();
    groupB.Transpose();    
  } else {
    vv.Message("Inconsistent data selected.");
    vv.Return(-3);
  }
} else {
    vv.Message("Some columns or rows must be highlighted.");
    vv.Return(-4);
}    

// =============================================================================

var mva = vv.FindPluginObject("MultivariateAnalysis");
if ( mva == null ) {
  vv.Message("MultivariateAnlysis plugin not installed!");
  vv.Return(-3);
}

var ret = (vv.EventSource.Item == "Do PLS") ? mva.DoPLS(groupA, groupB) : mva.DoCCA(groupA, groupB);

vv.GuiManager.ReuseLastWindow = false;
var pX = ret.ProjectionX.ShowCartesianView();
var pY = ret.ProjectionY.ShowCartesianView();
pX.Title = "High Lighted Data Rotated";
pY.Title = "Selected Data Rotated";
pX.ResetView();
pY.ResetView();

if ( moreDetails ) {
    var eXnt = New.NumberTable(ret.EigenVectorsX);
    var eYnt = New.NumberTable(ret.EigenVectorsY);
    for(var row=0; row<eXnt.Rows; row++) eXnt.RowSpecList[row].Id = "xEV" + row;
    for(var row=0; row<eYnt.Rows; row++) eYnt.RowSpecList[row].Id = "yEV" + row;
    var eX = New.ValueDiagram(eXnt);
    var eY = New.ValueDiagram(eYnt);
    eX.VScalingMode = 0;
    eY.VScalingMode = 0;
    eX.Title = "X EigenVectors";
    eY.Title = "Y EigenVectors";
    eX.Show();
    eY.Show();

    var evX = New.BarView(ret.EigenValuesX);
    var evY = New.BarView(ret.EigenValuesY);
    evX.Title = "X eigen values";
    evY.Title = "Y eigen values";
    evX.BaseLineType = 0;
    evY.BaseLineType = 0;
    for(var row=0; row<evX.ItemList.Count; row++) evX.ItemList[row].Id = "xEV" + row;
    for(var row=0; row<evY.ItemList.Count; row++) evY.ItemList[row].Id = "yEV" + row;
    evX.Show();
    evY.Show();
}
