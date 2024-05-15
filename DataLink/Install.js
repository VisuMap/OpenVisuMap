var mgr = vv.GuiManager;
var parentList = "MainForm|HeatMap|MdsCluster";
mgr.SetCustomMenu("PyUtils/UMAP", true, "UMapRun.py", parentList, null);
mgr.SetCustomMenu("PyUtils/OpenTsne", true, "OpenTsneRun.py", parentList, null);
mgr.SetCustomMenu("PyUtils/SciKitTsne", true, "SciKitTsne.py", parentList, null);
mgr.SetCustomMenu("PyUtils/BH-Sne", true, "BH_SneRun.py", parentList, null);
mgr.SetCustomMenu("PyUtils/hDBSCAN", true, "HdbscanRun.py", "MainForm|MapSnapshot", null);

vv.SetProperty("DataLink.CmdPort", "8877", "DataLink command IP port. Restart needed after change");
vv.SetProperty("DataLink.PythonEditor", "notepad", "Path to the python script editor.");
vv.SetProperty("DataLink.PythonProg", "python", "Path to the python engine. Use 'py -3.6' to choose a python version.");
vv.InstallPlugin("Data Link", vv.CurrentScriptDirectory +"\\DataLink.dll");
