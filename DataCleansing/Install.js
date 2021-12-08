var dir = vv.CurrentScriptPath.substr(0, vv.CurrentScriptPath.lastIndexOf("\\") + 1);
vv.InstallPlugin("Data Cleansing", dir +"DataCleansing.dll");
for(var nm of ["Logicle", "Logarithmic", "Scale Up", "Normalize", "Delete", "Duplicate", "InverseLogicle", "Custom"]) {
    vv.GuiManager.SetCustomMenu("Filter/" + nm, true, dir + "TableFilter.js", "<All>");
}
vv.SetProperty("DataCleansing.Logicle.Settings", "262144; 1.0; 4.5", "The T, W, M parameters for the logicle transformation.");
