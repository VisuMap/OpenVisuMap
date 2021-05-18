// ParallelJob.js
// Testing parallel jobs
//!import "Common.js"

var ms = vv.FindPluginObject("DMScript");
vv.ReadOnly = true;
LoadDataset(testDataset);


var tr = ms.NewTrainer();
tr.ReadOnly = true;
tr.Show();
tr.LogLevel = 3;
tr.MaxEpochs = 8;
tr.ModelName = mdName;
tr.ModelScript = "FFRegression.md.py";

tr.RefreshFreq = 2;
tr.ParallelJobs = 2;
tr.JobArgument = "A";
tr.Repeats = 3;

tr.StartTraining();
tr.WaitForCompletion();
//tr.Close();

var info = vv.FindWindow("Dataset Information");
if (info != null ) {
	info.DataChanged = false;
	info.Close();
}

//
// Checked the number of created models and delete them.
//
var cnt = ms.AllModels().Count;

//ms.DeleteModel(mdName);
//ms.DeleteModel(mdName + "1");
//ms.DeleteModel(mdName + "2");

var N = cnt - ms.AllModels().Count;
vv.Echo("N: " + N);
Assert( N == 3 );
