var dir = vv.CurrentScriptDirectory + "/";
vv.InstallPlugin("Genetic Analysis", dir +"GeneticAnalysis.dll");


function AddMenu(label, script, forms) {
	vv.GuiManager.SetCustomMenu(label, true, dir + script,  forms);
}
function AddMainMenu(label, script) {
	AddMenu("SeqUtil/" + label, script, "MainForm");
}

vv.GuiManager.RemoveCustomMenu("SeqUtil/");
vv.GuiManager.RemoveCustomMenu("Blast/");

AddMenu("SeqUtil/*", "SequenceOp.js", "SequenceMap");
AddMenu("Blast/Blast","Blast.js", "SequenceMap");
AddMenu("Blast/BlastDb", "BlastDb.js", "SequenceMap|MainForm");
AddMenu("Blast/NCBI-Post", "NcbiCdsPost.js", "SequenceMap|MainForm");

AddMainMenu("ShowMap", "LocateGenes.js");
AddMainMenu("ShowSeqs", "ShowSeqMap.js");
AddMainMenu("MarkExomes", "MarkExomes.js");

vv.SetProperty("GeneticAnalysis.SmithWaterman", "1 0 2 -4", 
	"Settings for smith-watern metric: gapCost, minMismatchScore, matscore and mismatchScore");
vv.SetProperty("GeneticAnalysis.NeedlemanWunsch", "2 0 1", 
	"Settings for needleman-wunsch metric: cost for gap, match and mismatch");
