// Install script for VisuModelling plugin
//
var mgr = vv.GuiManager;
var icon = vv.CurrentScriptDirectory + "\\Modelling.png";

/*
mgr.SetCustomButton("VisuModelling/Train", icon, "PtTrain.pyn");
mgr.SetCustomButton("VisuModelling/*", null, "PtOperations.pyn");
*/

mgr.SetCustomButton("VisuModelling/Train", icon, "VsTrain.pyn");
mgr.SetCustomButton("VisuModelling/*", null, "VsOperations.pyn");
