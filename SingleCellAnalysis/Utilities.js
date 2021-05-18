//MenuLabels SelectCells Snapshot Cell->Gene Gene->Cell CompCells CompGens CellExpr GeneExpr
//
//File: Utilities.js
//Various services to called from VisuMap context-menu.
//
var N = vv.Dataset.Rows - vv.Dataset.Columns;
var menu = vv.EventSource.Item;
var nt;

function GetData() {
    var tab = vv.GetNumberTableView();
    var bs = vv.Dataset.BodyList;
    var rowList = New.IntArray();
    var colList = New.IntArray();
    for (var i = 0; i < bs.Count; i++) {
        if (bs[i].Disabled)
            continue;
        if (i < N)
            rowList.Add(i);
        else
            colList.Add(i - N);
    }
    return tab.SelectRowsView(rowList).SelectColumns(colList);
}

function ToIdList(bodyList) {
    var idList = New.StringArray();
    for (var b in bodyList)
	idList.Add(b.Id);
    return idList;
}

function OpenGroupView(hm, title) {
    hm.ClickContextMenu("Utilities/Group Rows...");
    var bb = vv.LastView();
    bb.Left = hm.Left + 23;
    bb.Top = hm.Top + hm.Height - 8;
    bb.Width = hm.Width - 23;
    bb.Height = hm.Height;
    hm.Title = bb.Title = title;
}

switch (menu) {
    case "Cell->Gene":
    case "Gene->Cell":
        vv.Map.SelectedItems = null;
        var cs = vv.GetObject("SC.Utilities");
        cs.GeneToCell = (menu == "Gene->Cell");        
        var vw = New.MapSnapshot();
        vw.GlyphSet = vv.Map.GlyphType;
        vw.ShowMarker(true).MarkerColor = New.Color(cs.GeneToCell ? "Green" : "Red");
        vw.GlyphOpacity = Math.min(0.4, vw.GlyphOpacity);
        vw.Show();
        cs.SetExpression(GetData(), vw.BodyList, vv.Dataset.BodyList, N);        

        vw.AddEventHandler("ItemsSelected", function () {
            var cs = vv.GetObject("SC.Utilities");
            var thr = vv.ModifierKeys.ControlPressed ? 0 : cs.MeanExpression;
            var selectedItems = vv.EventSource.Argument;
            thr *= 1.0;  // chance to alter the sencitivity
            cs.Expression(selectedItems, thr);
            pp.Title = "Cell/Genes: " + cs.Cells + "/" + cs.Genes;
            pp.RedrawBodiesType();            
        });
        break;


    case "SelectCells":
	 var idList = New.StringArray();
	 var bsList = vv.Dataset.BodyList;
        for (var row = 0; row < N; row++)
            idList.Add(bsList[row].Id);
	 vv.Map.SelectedItems = idList;
        break;

    case "Snapshot":
	 if ( pp.Name != "MapSnapshot" ) {
	   vv.Message("The operation only works for map snapshot view!");
	   vv.Return(0);
        }
        var newView = New.MapSnapshot2(pp.BodyList, pp.MapLayout);
        var id2Body = New.DictId2Body(newView.BodyList);
        for (var id in pp.SelectedItems) id2Body[id].Highlighted = true;
        newView.ShowMarker(false);
        newView.SelectedItems = pp.SelectedItems;
        newView.Show();
        break;

    case "CellExpr": // the expression sume of selected cells.
        nt = GetData();
        nt = nt.ApplyFilter(vv.Map.Filter);
        var bv = New.BarView(nt.SelectRowsView(New.IntRange(5)));
        bv.AggregationOrientation = 0;
        bv.AggregationMethod = 0;
        bv.AutoScaling = true;
        bv.Horizontal = false;
        bv.Tag = nt;
        bv.Show();        
        bv.Detach();
        bv.Title = "Expression of Selected Cells";
        var cb = function () {            
            var newData = pp.Tag.SelectRowsByIdView(vv.EventSource.Item);
            if (newData.Rows > 0) {
                pp.SetDataByTable(newData);
                pp.Title = "Selected Cells: " + newData.Rows;
            }
        };
        vv.EventManager.OnItemsSelected(cb, bv, null);
        vv.Map.ShowMarker(true).MarkerColor = New.Color("Red");
        break;

    case "GeneExpr":  // the expression sume of selected genes.
        nt = GetData();
        var bv = New.BarView(nt.SelectColumns(New.IntRange(5)));
        bv.AggregationOrientation = 1;
        bv.AutoScaling = true;
        bv.AggregationMethod = 8;
        bv.Horizontal = true;
        bv.Tag = nt;
        bv.Show();
        bv.Detach();        
        bv.Title = "Expression of Selected Genes";
        var cb = function () {
            var newData = pp.Tag.SelectColumnsById(vv.EventSource.Item);
            if (newData.Columns > 0) {
                pp.SetDataByTable(newData);
                pp.Title = "Selected Genes: " + newData.Columns;
            }
        };
        vv.EventManager.OnItemsSelected(cb, bv, null);
        vv.Map.ShowMarker(true).MarkerColor = New.Color("Green");
        break;

    case "CompCells": // compare selected cells with highlighted cells.
        var bs1 = pp.Dataset.BodyListHighlighted();
        var bs2 = pp.GetSelectedBodies();
        var nt = GetData();
	 var nt1 = nt.SelectRowsByIdView(ToIdList(bs1));
	 var nt2 = nt.SelectRowsByIdView(ToIdList(bs2));
        for (var rs in nt1.RowSpecList) rs.Type = 0;
        for (var rs in nt2.RowSpecList) rs.Type = 1;
        var nt3 = nt1.Append(nt2);
        var hm = New.HeatMap(nt3).Show();
        OpenGroupView(hm, "Cell Group Expression");
        break;

    case "CompGens": // compare selected genes with highlighted genes.
        var bs1 = pp.Dataset.BodyListHighlighted();
        var bs2 = pp.GetSelectedBodies();
        var nt = GetData();
        var nt1 = nt.SelectColumnsById(ToIdList(bs1));
        var nt2 = nt.SelectColumnsById(ToIdList(bs2));
        for (var cs in nt1.ColumnSpecList) cs.Group = 0;
        for (var cs in nt2.ColumnSpecList) cs.Group = 1;
        var nt3 = nt1.AppendColumns(nt2);
        nt3.Transpose();
        var hm = New.HeatMap(nt3).Show();
        OpenGroupView(hm, "Gene Group Expression");
        break;
}
