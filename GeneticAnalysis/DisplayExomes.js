// DisplayExomes.js
// Display exomes of selected CDS transcripts.
//
var ds = vv.Dataset;
var sm = vv.FindFormList("SequenceMap")[0];
var allBegin = New.IntArray();
var allEnd = New.IntArray();

var cdsBegin = New.IntArray();
var cdsEnd = New.IntArray();

sm.HighlightColor = New.Color("White");

for(var id in vv.SelectedItems) {
  var rowIdx = ds.IndexOfRow(id);
  var begin = New.IntArray(ds.GetDataAt(rowIdx, 3));
  var end = New.IntArray(ds.GetDataAt(rowIdx, 4));
  begin.Sort();
  end.Sort();

  allBegin.AddRange(begin);
  allEnd.AddRange(end);
  cdsBegin.Add(begin[0]);
  cdsEnd.Add(end[end.Count-1]);

  sm.Title = "Exoms: " + begin.Count;
  sm.SetSelection(begin[0], end[end.Count-1]);
  vv.Sleep(100);
  sm.SetSelections(begin, end)
  vv.Sleep(100);
}


sm.SetSelection(allBegin[0], allEnd[allEnd.Count-1]);
vv.Sleep(500);
sm.SetSelections(allBegin, allEnd);
vv.Sleep(1500);
sm.ClearMap();
sm.HighlightColor = New.Color(32,255,255,255);
sm.SetSelections(cdsBegin, cdsEnd);
sm.Redraw();


