var mgr = vv.GuiManager;
mgr.SetCustomMenu("PyUtils/UMAP", true, "UMapRun.py", "MainForm|Heatmap", null);

vv.SetProperty("DataLink.CmdPort", "8877", "DataLink command IP port. Restart needed after change");
vv.SetProperty("DataLink.PythonEditor", "notepad", "Path to the python script editor.");
vv.SetProperty("DataLink.PythonProg", "python", "Path to the python engine. Use 'py -3.6' to choose a python version.");
vv.InstallPlugin("Data Link", vv.CurrentScriptDirectory +"\\DataLink.dll");
