var nt = pp.GetSelectedNumberTable();
var m = nt.Matrix;

if ( nt.Rows != 2 ) {
	vv.Message("Please select a table with two rows.");
	vv.Return();
}

var m0 = 0;
var m1 = 0;
for(var col=0; col<nt.Columns; col++) {
	m0 += m[0][col];
	m1 += m[1][col];
}
m0 /= nt.Columns;
m1 /= nt.Columns;

var v0 = 0;
var v1 = 0;
for(var col=0; col<nt.Columns; col++) {
	m[0][col] -= m0;
	m[1][col] -= m1;
	v0 += m[0][col] * m[0][col];
	v1 += m[1][col] * m[1][col];
}

var cr = 0;
for(var col=0; col<nt.Columns; col++)  cr += m[0][col] * m[1][col];


cr /= Math.sqrt(v0 * v1);

vv.Message(cr.ToString("f3"));