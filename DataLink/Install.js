vv.SetProperty("DataLink.CmdPort", "8877", "DataLink command IP port. Restart needed after change");
vv.SetProperty("DataLink.PythonEditor", "notepad", "Path to the python script editor.");
vv.InstallPlugin("Data Link", vv.CurrentScriptDirectory +"\\DataLink.dll");
