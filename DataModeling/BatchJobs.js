// BatchJobs.js
// Script to launch a sequence of parallel jobs
var dm = vv.FindPluginObject("DMScript");
var tr = dm.CurrentTrainer
if ( (tr == null) || tr.IsDisposed ) {
	tr = dm.NewTrainer();
	tr.Show();
	vv.Sleep(5000);
}

for( tr.JobArgument in New.StringArray("C", "A", "B") ) {
	vv.Echo('Batch: ' + tr.JobArgument);
	tr.StartTraining().WaitForCompletion();
}
vv.Echo('Completed!');

/*
tr.ParallelJobs = 1;
tr.LogLevel = 3;
tr.MaxEpochs = 500;
tr.RefreshFreq = 5;
tr.ModelScript = "TT.md.py";
*/

/*
for(var n=0; n<4; n++) {
	tr.JobArgument = n;
	tr.StartTraining().WaitForCompletion();
	vv.Echo("Job Arg: " + n);
	vv.Sleep(1000);
}

vv.Echo("\nCompleted")
*/
