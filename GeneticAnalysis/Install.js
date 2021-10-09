var dir = vv.CurrentScriptDirectory + "/";
vv.InstallPlugin("Genetic Analysis", dir +"GeneticAnalysis.dll");

function AddMenu(label, script, forms) {
	vv.GuiManager.SetCustomMenu("SeqUtil/" + label, true, dir + script,  forms);
}
function AddMainMenu(label, script) {
	AddMenu(label, script, "MainForm");
}

vv.GuiManager.RemoveCustomMenu("SeqUtil/");

AddMenu("*", "SequenceOp.js", "SequenceMap");
AddMenu("Blast","Blast.js", "SequenceMap");
AddMenu("BlastDb", "BlastDb.js", "SequenceMap|MainForm");
AddMenu("NCBI-Post", "NcbiCdsPost.js", "SequenceMap|MainForm");

AddMainMenu("ShowMap", "LocateGenes.js");
AddMainMenu("ShowSeqs", "ShowSeqMap.js");
AddMainMenu("MarkExomes", "MarkExomes.js");

vv.SetProperty("GeneticAnalysis.SmithWaterman", "1 0 2 -4", 
	"Settings for smith-watern metric: gapCost, minMismatchScore, matscore and mismatchScore");
vv.SetProperty("GeneticAnalysis.NeedlemanWunsch", "2 0 1", 
	"Settings for needleman-wunsch metric: cost for gap, match and mismatch");
