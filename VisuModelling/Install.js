// Install script for VisuModelling plugin
//
var mgr = vv.GuiManager;
var icon = vv.CurrentScriptDirectory + "\\Modelling.png";
mgr.SetCustomButton("VisuModelling/Map and Cluster", icon, "VsMap.pyn");
mgr.SetCustomButton("VisuModelling/Just Map", null, "VsTrain.pyn");
mgr.SetCustomButton("VisuModelling/Just Cluster", null, "VsCluster.pyn");
mgr.SetCustomButton("VisuModelling/*", null, "VsOperations.pyn");

/*
For using Pytorch library:

var mgr = vv.GuiManager;
var icon = vv.CurrentScriptDirectory + "\\Modelling.png";
mgr.SetCustomButton("VisuModelling/Train", icon, "PtTrain.pyn");
mgr.SetCustomButton("VisuModelling/*", null, "PtOperations.pyn");

For installing pytorch:

py -3.7 -m pip install torch==1.10.0+cu102 torchvision==0.11.1+cu102 torchaudio===0.10.0+cu102 -f https://download.pytorch.org/whl/cu102/torch_stable.html

*/

