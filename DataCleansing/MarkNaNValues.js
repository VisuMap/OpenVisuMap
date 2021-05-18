// File: MarkNaNValues.js
//
// Description: Find all data rows with NaN numeric values and mark them
// with the glyph type (body type) 15.
//
// Usage: Load a dataset into VisuMap; then run this script.
//

var cs = New.CsObject("\
	public void MarkNaN(INumberTable nt, IList<IBody> bodies, short type) {\
		for(int row=0; row<nt.Rows; row++)\
		for(int col=0; col<nt.Columns; col++) {\
		  if ( double.IsNaN( nt.Matrix[row][col]) ) {\
		    bodies[row].Type = type;\
		    break;\
		  }\
		}\
	}\
");

cs.MarkNaN(vv.GetNumberTableView(), vv.Dataset.BodyList, 15);
vv.Map.Redraw();
