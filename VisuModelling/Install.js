// Install script for VisuModelling plugin
//
var mgr = vv.GuiManager;
var icon = vv.CurrentScriptDirectory + "\\Modelling.png";
mgr.SetCustomButton("VisuModelling/Train", icon, "TrainModel.pyn");
mgr.SetCustomButton("VisuModelling/*", null, "ModelOps.pyn");

