// Install Single-Cell-Atlas related menus mark and run the following statements
//
function InstallAtlas() {
    var mgr = vv.GuiManager;
    var icon = vv.CurrentScriptDirectory + "\\MenuIcon.png";

    for (var [label, script, view, img] of [
        ["Dual Sorting", "DualSorting.js", "HeatMap", icon],
        ["Dual Embedding", "DualEmbedding.js", "HeatMap", null],
        ["Dual Clustering", "DualClustering.js", "HeatMap", null],
        ["Active Genes", "ShowActiveGenes.js", "HeatMap", null],
        ["Active Cells", "ShowActiveCells.js", "HeatMap", null],
        ["Save Data", "SaveDataset.js", "HeatMap", null],
        ["Save HeatMap", "SaveSortedHeatmap.js", "HeatMap", null],
        ["Save DsHm", "!vv.Import('AtlasHelp.js');SaveDsHm(pp);", "HeatMap", null],
        ["Show Mean", "ShowMeanVector.js", "HeatMap", null],		 
        ["Merge DS", "!vv.Import('AtlasHelp.js');ConcatDatasets(SelectedDs(), 5000, null)", "Atlas", null],
        ["Compare Maps", "MapMorph.js", "MainForm|MapSnapshot|D3dRender", icon],
        ["Variation Tracing", "VariationTracing.js", "MainForm", null],
		  ["Merge Maps", "CascadingTsne.pyn", "Atlas", null],
    ]) mgr.SetCustomMenu("Atlas/" + label, true, script, view, img);

    for (var [label, img, script] of [
        ["Import Loom", icon, "LoomRead.pyn"],
        ["Import H5AD", null, "H5adRead.pyn"],
        ["Import H5", null, "H5Read.pyn"],
        ["Import VCF", null, "VcfRead.pyn"],			
        ["Imp Cnt Mtx (*.mtx.gz)", null, "MatrixRead.pyn"],
        ["Show Maps", null, "ShowMaps.js"],
    ]) mgr.SetCustomButton("Atlas/" + label, img, script);


	var sCfg = New.ClassType('System.IO.File').ReadAllText(vv.CurrentScriptDirectory + '/MenuCfg.pyn')
	var formList = sCfg.substring(sCfg.indexOf(':')+1, sCfg.indexOf('\r\n')).split(',')
	for(var fm of formList) {
		var i0 = sCfg.indexOf('#MenuLabels', sCfg.indexOf('#'+fm)+1);
		var i1 = sCfg.indexOf('##', i0);
		var sScript = '@' + sCfg.substring(i0, i1);
		mgr.SetCustomMenu('Atlas/*', true, sScript, fm, null);
	}
}

InstallAtlas();

if ( vv.ScriptDirectories.indexOf( vv.CurrentScriptDirectory ) < 0 )
	vv.ScriptDirectories += ";"+vv.CurrentScriptDirectory;
