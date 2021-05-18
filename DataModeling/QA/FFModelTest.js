// GuiTest.js
// Simple test of the GUI interface.
//!import "Common.js"

vv.ReadOnly = true;
LoadDataset(testDataset);

ClickMenu("Model Training");
var tr = vv.FindWindow("Model Training");
Assert(tr.Title == "Model Training");

tr = tr.TheForm;
tr.LogLevel = 3;
tr.JobArgument = "A";
tr.MaxEpochs = 40;
tr.ModelScript = "FFModel.md.py";
tr.ModelName = mdName;
tr.RefreshFreq = 5;
tr.ParallelJobs = 1;
tr.StartTraining();
tr.WaitForCompletion();
tr.Close();

var info = vv.FindWindow("Dataset Information");
if (info != null) {
	info.DataChanged = false;
	info.Close();
}

ClickMenu("Model Evaluation");
var eval = vv.FindWindow("Model Evaluation");
eval = eval.TheForm;
vv.Echo(eval.ModelName + " : " + mdName)
Assert(eval.ModelName == mdName);
eval.EvalScript = "Eval.ev.py";
eval.DoPrediction();
vv.Sleep(500);
eval.Close();

ClickMenu("Model Manager");
var mgr = vv.FindWindow("Working Directory");
mgr = mgr.TheForm;
var mdList = mgr.ModelList();
var cnt = mdList.Count;
mgr.DeleteModel(mdName);
mdList = mgr.ModelList();
Assert( (cnt - mdList.Count) == 1 );
mgr.Close();


