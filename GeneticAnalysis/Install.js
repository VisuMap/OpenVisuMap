var dir = vv.CurrentScriptPath.substr(0, vv.CurrentScriptPath.lastIndexOf("\\") + 1);
vv.InstallPlugin("Genetic Analysis", dir +"GeneticAnalysis.dll", true);
vv.GuiManager.SetCustomMenu("SeqUtil/*", true, dir + "SequenceOp.js", "SequenceMap");
vv.GuiManager.SetCustomMenu("Blast/Blast", true, dir + "Blast.js", "SequenceMap");
vv.GuiManager.SetCustomMenu("Blast/BlastDb", true, dir + "BlastDb.js", "SequenceMap|MainForm");
vv.GuiManager.SetCustomMenu("Blast/NCBI-Post", true, dir + "NcbiCdsPost.js", "SequenceMap|MainForm");
vv.SetProperty("GeneticAnalysis.SmithWaterman", "1 0 2 -4", "Settings for smith-watern metric: gapCost, minMismatchScore, matscore and mismatchScore");
vv.SetProperty("GeneticAnalysis.NeedlemanWunsch", "2 0 1", "Settings for needleman-wunsch metric: cost for gap, match and mismatch");
