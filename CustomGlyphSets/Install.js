var dir = vv.ApplicationData + "\\plugins\\Custom Glyphs\\";
vv.InstallGlyphSet("Red Green 3", dir +"RedGreenBinary",  0, 1.0);
vv.InstallGlyphSet("Phases 8", dir +"Phase8",  0, 1.0);
vv.InstallGlyphSet("Phases 16", dir +"Phase16",  0, 1.0);
vv.InstallGlyphSet("Alpha Particles", dir +"AlphaParticles",  0, 1.0);
vv.InstallGlyphSet("ColorWheel 128", dir +"ColorWheel128",  0, 1.0);

var sDir = vv.CurrentScriptPath.substr(0, vv.CurrentScriptPath.lastIndexOf("\\") + 1);
vv.InstallPlugin("Custom Glyphs", dir +"CustomGlyphSets.dll", true);

