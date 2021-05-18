var dir = vv.CurrentScriptPath.substr(0, vv.CurrentScriptPath.lastIndexOf("\\") + 1);
vv.SetProperty("DataModeling.HomeDir", dir, "DataModeling home directory");
vv.SetProperty("DataModeling.WorkDir", dir, "DataModeling working directory");
vv.InstallPlugin("Data Modeling", dir +"DataModeling.dll", true);
