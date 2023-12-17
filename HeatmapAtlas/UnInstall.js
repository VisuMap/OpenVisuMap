vv.GuiManager.RemoveCustomMenu("Atlas/");
vv.GuiManager.RemoveCustomButton("Atlas/");

if ( vv.ScriptDirectories.indexOf( vv.CurrentScriptDirectory ) >= 0 )
	vv.ScriptDirectories = vv.ScriptDirectories.replace(";"+vv.CurrentScriptDirectory, "");
