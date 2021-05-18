// File: CalculateCurvature.js
// ========================================================================================

var filter = vv.Folder.OpenSequence2Filter(vv.Map.Filter);

var bodyList = New.BodyList();
for(var body in vv.Dataset.BodyList) 
	if( !body.Disabled && (body.Type == 0) ) bodyList.Add(body);

var sa = vv.FindPluginObject("SeqAnalysis");
sa.CalculateCurvature(null, "R0", 0, bodyList, -filter.StepSize, -filter.StepSize)


