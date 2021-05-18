var dir = vv.CurrentScriptPath.substr(0, vv.CurrentScriptPath.lastIndexOf("\\") + 1);
vv.InstallGlyphSet("VectorField", dir + "VectorField", false);
vv.InstallPlugin("Clip Recorder", dir +"ClipRecorder.dll", true);
