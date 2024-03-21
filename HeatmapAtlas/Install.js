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


var scriptStr = `@#MenuLabels CaptureMaps EmbedSelected
vv.Import('GeneMonitor.pyn')
match vv.EventSource.Item:
	case 'CaptureMaps':
		vv.AtlasManager.OpenAtlas().CaptureAllOpenViews()
	case 'EmbedSelected':
		EmbedGenes(vv.SelectedItems, epochs=2000, EX=4.0, PP=0.05)`;
	 mgr.SetCustomMenu('Atlas/*', true, scriptStr, "MainForm", null)

scriptStr = `@#MenuLabels SetMapLabel AdjustAtlasMaps
vv.Import('GeneMonitor.pyn')
match vv.EventSource.Item:
	case 'SetMapLabel':
		SetAtlasItemName()
	case 'AdjustAtlasMaps':		
		AdjustAtlasMaps(1000, 700, 0.5, 0.5)`;
	 mgr.SetCustomMenu('Atlas/*', true, scriptStr, "Atlas", null)

scriptStr = `@#MenuLabels Monitor ShowData ReEmbedding 3D-Expression ActiveCells
vv.Import('GeneMonitor.pyn')
match vv.EventSource.Item:
	case 'Monitor':
		MonitorMap(pp)
	case 'ShowData':
		ShowData(pp)
	case 'ReEmbedding':
		ReEmbedding(pp)
	case '3D-Expression':
		ShowExpress3D(pp)
	case 'ActiveCells':
		ShowActiveCells(pp)`;
	 mgr.SetCustomMenu('Atlas/*', true, scriptStr, "MapSnapshot", null)


}

InstallAtlas();


if ( vv.ScriptDirectories.indexOf( vv.CurrentScriptDirectory ) < 0 )
	vv.ScriptDirectories += ";"+vv.CurrentScriptDirectory;
