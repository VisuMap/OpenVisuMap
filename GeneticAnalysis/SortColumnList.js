// SortColumnList.js

for(var row=0; row<vv.Dataset.Rows; row++) {
	var s = vv.Dataset.GetDataAt(row, 6).Split(';');
	var vList = New.IntArray();
	for(var k=0; k<s.Length; k++) {
		vList.Add(s[k] - 0);
	}
	vList.Sort();	
	s = "";
	for(var v in vList) {
		if ( s.Length > 0) s += ";";
		s += v.ToString();
	}
	vv.Dataset.SetDataAt(row, 6, s);
	if ( row % 100 == 0 ) { vv.Title = "N:" + row; vv.Sleep(0); }
}
vv.Dataset.CommitChanges();