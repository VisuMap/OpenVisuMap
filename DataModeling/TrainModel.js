// File: TrainModel.js
//============================================
var ms = vv.FindPluginObject("DMScript");
var mdName = "TestModel";

// Train a model.
var trainer = ms.NewTrainer();
trainer.ReadOnly = true;
trainer.Show();

trainer.LogLevel = 2;
trainer.MaxEpochs = 80;
trainer.ModelScript = "FFModel.md.py";
trainer.ModelName = mdName;
trainer.RefreshFreq = 20;
trainer.StartTraining();
trainer.WaitForCompletion();
trainer.Close();

// Apply the trained model.
var tester = ms.NewTester();
tester.Show();
tester.SelectModel(mdName);
tester.DoPrediction();
tester.Close();
vv.Sleep(1000);
vv.GuiManager.RecoverLastMap();
