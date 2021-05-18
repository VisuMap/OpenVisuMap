// AllTest.js
vv.CurrentDirectory = vv.CurrentScriptDirectory + "\\..";
var testList = New.StringArray(
    "GuiTest.js", 
    "TrainingTest.js",
    "FFModelTest.js", 
    "ParallelJob.js",
);

for( var tst in testList) {
  vv.RunScript("QA\\" + tst);
  vv.Echo("Completed: " + tst);
}
