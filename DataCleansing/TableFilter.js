// File: TableFilter.js
//
// Description: provides services to filter selected data table.
//

var filter = vv.FindPluginObject("TableFilter");
if ( filter == null ) {
  vv.Message("DataCleansing plugin not installed!");
  vv.Return(-1);
}

var nt = pp.GetNumberTable();
if ( nt == null ) {
  vv.Message("Current data view does not support this operation!");
  vv.Return(-1);
}

var items = pp.SelectedItems;
if ( (items == null) || (items.Count == 0) ) {
  vv.Message("No data select selected.");
  vv.Return(0);
}


var isAttribute = false;

if ( pp.Name == "HeatMap" ) {
  isAttribute = (pp.SelectionMode == 1);
} else if ( pp.Name == "ValueDiagram") {
  isAttribute = (pp.DiagramMode == 2);
} else {
  isAttribute = pp.AttributeMode;
}


switch(vv.EventSource.Item) {
    case "Logicle": filter.Logicle(nt, isAttribute, items);
    break;

    case "Logarithmic": filter.Logarithmic(nt, isAttribute, items);
    break;
    
    case "Scale Up": filter.Scale(nt, isAttribute, items, 0);
    break;
        
    case "Normalize": filter.Normalize(nt, isAttribute, items);
    break;
    
    case "Delete": filter.Delete(nt, isAttribute, items);  
    break;
    
    case "Duplicate": filter.Duplicate(nt, isAttribute, items);
    break;
    
    case "InverseLogicle": filter.InverseLogicle(nt, isAttribute, items);
    break;

    case "Custom": CustomFilter(nt, isAttribute, items);
    break;
}

pp.Redraw();

// A JavaScript custom filter.
function CustomFilter(nTable, aMode, itemList) {
    var m = nTable.Matrix;
    for(var id in itemList) {
        if ( aMode ) {
            var col = nTable.IndexOfColumn(id);
            for(var row=0; row<nTable.Rows; row++) {
                m[row][col] += 1.0;
            }
        } else {
            var row = nTable.IndexOfRow(id);
            for(var col=0; col<nTable.Columns; col++) {
                m[row][col] += 1.0;
            }
        }
    }
}

