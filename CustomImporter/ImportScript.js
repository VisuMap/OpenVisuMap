// File: ImportScript.js
//
// Description: This script provides a template to import one or more custom data files.
// 
// Usage: fill the parameters in the following block, then run this script.
// 
var fileList = New.StringArray(  // Copy&Paste or drag&drop a file list to the following block.

);
var mergingTables = true;    // merge all files to a single dataset.
var transpose = false;       // import the transposed the data table.
var columns = 5;                  // number of columns in the tables.
var textColumns = New.IntArray(); // List of textual collumns, e.g. New.IntArray(0,5,7);
var skipHeaderLine = true;  // skip the header line in each imput file.

///////////////////////////////////////////////////////////////////////////////////////////////

var fso = new ActiveXObject("Scripting.FileSystemObject");
var dataType = 0;  // row data type for each file, increment once after each file.
var tableList = mergingTables ? new Array() : null;

for(var f in fileList) {
  var table = LoadFile(f);

  if (transpose) table = table.Transpose(true);

  var rsList = table.RowSpecList;
  for(var row=0; row<table.Rows; row++) {
    rsList[row].Type = dataType;
  }
  dataType++;
  
  if ( mergingTables ) {
    tableList.push(table);
  } else {
    var dsName = table.SaveAsDataset(File2DatasetName(f), f);
    CheckResult(dsName, table);
  }
}

if (mergingTables && (tableList.length>0)) {
  MergeAndSave(tableList);
}

///////////////////////////////////////////////////////////////////////////////////////////////

// Merge a list of tables into a single table and save it as new dataset.
function MergeAndSave(tbList) {
  var table = New.FreeTable();
  var csList = tbList[0].ColumnSpecList;
  for(var col=0; col<csList.Count; col++) {
    table.AddColumn(csList[col].Id, (csList[col].DataType=='n'));
  } 

  var row=0;
  var dsName = "";
  var idPrefix = transpose ? "C" : "R";  
  for(var i in tbList) {
    var t = tbList[i];
    var rsList = t.RowSpecList;
    for(var r=0; r<t.Rows; r++) {
      table.AddRow(idPrefix+row, rsList[r].Type, t.Matrix[r]);
      row++;
    }
    
    if ( dsName != "" ) dsName += ", ";
    dsName += File2DatasetName(t.Tag);
  }

  dsName = table.SaveAsDataset(dsName, "Merged from : "+dsName);
  CheckResult(dsName, table);
  vv.Folder.OpenDataset(dsName);
}

function CheckResult(dsName, table) {
    if ( dsName != null ) {
      vv.Echo("New table imported: " + dsName + " with " + table.Rows + " rows, " + table.Columns + " columns.");
    } else {
      vv.Echo("Failed to import data: " + vv.LastError);
    }
}

// Load a ASCII file into a table.
function LoadFile(f) {
  var inFile = fso.OpenTextFile(f, 1);
  var table = New.FreeTable();

  for(var col=0; col<columns; col++) {
    table.AddColumn("C"+col, (textColumns.IndexOf(col)<0) )
  }

  var row = 0;
  var sepReg = new RegExp("[ ,\t]+");  // Using space, comma or tab as separator.
  var lineNr = 0;
  while( !inFile.AtEndOfStream ) {

    var line = inFile.ReadLine(); lineNr++;    

    if ( skipHeaderLine && (lineNr == 1) ) continue;

    var fields = line.split(sepReg);
    if ( fields.length != columns ) continue;
    var R = New.StringArray();
    for(var col=0; col<columns; col++) {
      R.Add(fields[col]);
    }
    table.AddRow("R"+row, R);
    
    /* short and fast version of above block.
    var R = line.Split(" ,\t".ToCharArray(), 20, 1);   
    if ( R.Length == columns ) table.AddRow("R"+row, R);    
    */
    
    row++;
  }
  inFile.Close();

  table.Tag = f;
  
  vv.Echo("Loaded table: " + f + ": " + table.Rows + " rwos, " + table.Columns + " columns");
  return table;
}

// Returns the base name part of a file path.
function File2DatasetName(filePath) {
  // return fso.GetFileName(filePath);
  return fso.GetBaseName(filePath);
}
