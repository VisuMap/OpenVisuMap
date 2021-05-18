// Show gene-expression-map of selected cells.
//
// Usage: 1: create a gene expression map; 2: open a snapshot with the gene
// map; 3: load a cell map in the main map; 4: Run the menu "Tracing Features" of
// the snapshot view; 5: Selected some cells in the main map view, 
// the gene snapshot view visualize highly expression genes via different glyph size.
//
var sc = vv.GetObject("SC.Utilities");
if (vv.ModifierKeys.ControlPressed) {
    // Assume that the main window is the target feature map.
    var map = New.MapSnapshot();
    map.Show();
    map.TopMost = true;
    map.ReadOnly = true;
    sc.SetExpressionTable(pp.GetNumberTable(), map);
    pp.ShowMarker(true);
} else {
    vv.Map.ShowMarker(true);
    sc.SetExpressionTable(vv.GetNumberTableView(), pp);
    pp.TopMost = true;  
}
