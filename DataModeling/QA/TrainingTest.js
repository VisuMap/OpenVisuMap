// TrainingTest.js
// Test various training scripts.
//!import "Common.js"

var ms = vv.FindPluginObject("DMScript");
vv.ReadOnly = true;

var tr = ms.NewTrainer();
tr.ReadOnly = true;
tr.Show();
tr.LogLevel = 2;
tr.MaxEpochs = 8;
tr.ModelName = "<NotSave>";
tr.RefreshFreq = 2;
tr.ParallelJobs = 1;
tr.JobArgument = "A";
tr.ValidationData = null;

var sList = New.StringArray(
	"FFModel.md.py", 
	"FFClassification.md.py", 
	"FFRegression.md.py",
	"Autoencoder.md.py",
	"CustomTarget.md.py",
	"KerasClassification.md.py",
	"MultiLane.md.py",
	"Segregated.md.py",
	"Test.md.py",
	"PCA.md.py",
	"gan.md.py",
);

LoadDataset(testDataset);
vv.Folder.OpenDataset("Training Data");

for(var s in sList){
	LoadDataset(testDataset);
	if( (s == "PCA.md.py") || (s == "gan.md.py") )
		vv.Folder.OpenDataset("GanData");
	tr.ModelScript = s;
	vv.Echo("    Running script: " + s);
	tr.StartTraining();
	tr.WaitForCompletion();
}


var mdName = "TestModel";
LoadDataset(testDataset);
vv.Folder.OpenDataset("Photo2");
tr.ModelScript = "FullCnn.md.py";
tr.ModelName = mdName;
vv.Echo("    Running script: " + tr.ModelScript);
tr.StartTraining();
tr.WaitForCompletion();


tr.ModelScript = "ReTrain.md.py";
vv.Echo("    Running script: " + tr.ModelScript);
tr.StartTraining();
tr.WaitForCompletion();

tr.Close();

ms.DeleteModel(mdName);
vv.Folder.OpenDataset("Training Data");

var info = vv.FindWindow("Dataset Information");
if (info != null ) {
	info.DataChanged = false;
	info.Close();
}
for (var f in vv.OpenWindowList()) {
	if ( (f.Name=="ValueDiagram") || (f.Name=="HeatMap") )
		f.Close();
}
