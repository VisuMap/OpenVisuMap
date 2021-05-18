// AddGeneItems.js
//
var ds = vv.Dataset;
pp.ClearItems();

var hasExome = ( (ds.Columns >= 4) && ds.ColumnSpecList[3].IsEnumerate );

for(var body in vv.Dataset.BodyList) {
	var id = body.Id;
	var rowIdx = ds.IndexOfRow(id);

	if ( hasExome ) {
		var iBegin = New.IntArray();
		var iEnd = New.IntArray();
		iBegin.AddRange(New.IntArray(ds.GetDataAt(rowIdx, 3)));
		iEnd.AddRange(New.IntArray(ds.GetDataAt(rowIdx, 4)));
		for(var k=0; k<iBegin.Count; k++) { iBegin[k]--; iEnd[k]--; }
	
		var minIdx = 1000 * 1000000;
		var maxIdx = -1000;
		for(var n=0; n<iBegin.Count; n++) {
			minIdx = Math.min(minIdx, iBegin[n]);
			maxIdx = Math.max(maxIdx, iBegin[n]);
			minIdx = Math.min(minIdx, iEnd[n]);
			maxIdx = Math.max(maxIdx, iEnd[n]);
		}
	
		pp.AddItem( id, minIdx, maxIdx);
	} else {
		var idx = ds.GetDataAt(rowIdx, 0) - 0;
		var len = 16;
		pp.AddItem(id, idx, idx+len-1)
	}
}
