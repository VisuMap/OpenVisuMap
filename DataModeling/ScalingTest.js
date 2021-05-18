var ds = vv.Dataset;
var ms = vv.FindPluginObject("DMScript");
var map = New.MapSnapshot().Show();

for(var k=0; k<10; k++) {
	for(var i=0; i<101; i++)
	for(var j=0; j<3; j++) 
		ds[i, j] *= 0.9;

	ms.ApplyModel("NewModelA");
	ms.ApplyModel("NewModelB");
	for(var i=0; i<101; i++)
		map.BodyList.Add(ds.BodyList[i].Clone());
	map.RedrawBodies();
	vv.Echo2(k + " ");
}
