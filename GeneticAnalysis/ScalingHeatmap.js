// File: ScalingHeatmap.js
// Scrolling Targets: 278
//
var t = pp.GetNumberTable();
if (pp.Tag == null) {
  pp.Tag = t.Rows * t.Columns;
}
var N = pp.Tag;
//var scaling = 1+vv.EventSource.Argument * 0.001;  // 298
var scaling = 0.5;
var newColumns = parseInt(t.Columns * scaling, 10);
if ( newColumns == t.Columns ) newColumns += vv.EventSource.Argument;
var newRows = parseInt( N / newColumns + 1, 10);
var tt = New.NumberTable(newRows, newColumns);

var cs = New.CsObject("CopyTable", "\
public void ToTable(INumberTable t1, INumberTable t2) {\
	int rIdx = 0;  int cIdx = 0; \
	for(int row=0; row<t1.Rows; row++) \
	for(int col=0; col<t1.Columns; col++) { \
		if ( rIdx>=t2.Rows ) break; \
		t2.Matrix[rIdx][cIdx] = t1.Matrix[row][col]; \
		cIdx++; \
		if ( cIdx >= t2.Columns) { \
			cIdx=0; \
			rIdx++; }}}");
cs.ToTable(t, tt);
t.Copy(tt);
pp.Redraw();