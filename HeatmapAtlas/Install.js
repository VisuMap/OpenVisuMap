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


var scriptStr = `@#MenuLabels - 'Capture Maps' 'Embed Selected', Monitor 'Show Data' 3D-Expression
vv.Import('GeneMonitor.pyn')
match vv.EventSource.Item:
	case 'Capture Maps':
		vv.AtlasManager.OpenAtlas().CaptureAllOpenViews()
	case 'Embed Selected':
		EmbedGenes(vv.SelectedItems, epochs=2000, EX=10, ex=1.0, PP=0.05, repeats=2)
	case 'Monitor':
		MonitorMap(vv.Map)
	case 'Show Data':
		ShowData(vv.Map)
	case '3D-Expression':
		ShowExpress3D(vv.Map)`;

mgr.SetCustomMenu('Atlas/*', true, scriptStr, "MainForm", null);

scriptStr = `@#MenuLabels - "Set Labels" "Configure Maps" 'Cluster Maps' 'Label Maps'
vv.Import('GeneMonitor.pyn')
selected = pp.GetSelectedItems()
match vv.EventSource.Item:
	case 'Set Labels':
		SetAtlasItemName(pp, selected)
	case 'Configure Maps':		
		AdjustAtlasMaps(pp, selected, 1000, 700, gSize=0.25, gOpacity=0.5, hiddenSize=8)
	case 'Cluster Maps':
		ClusterAtlasMaps(pp, selected, epsilon=1.0, minPoints=25)
	case 'Label Maps':
		LabelAtlasMaps(pp, selected)`;

mgr.SetCustomMenu('Atlas/*', true, scriptStr, "Atlas", null);

scriptStr = `@#MenuLabels - Monitor ShowData ReEmbedding 3D-Expression ActiveCells Clustering LabelGenes LabelAll ShowGeneTable MatchMap 'Show Super Cluster'
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
		ShowActiveCells(pp)
	case 'Clustering':
		ClusterMap(pp, epsilon=1.5, minPoints=25)
	case 'LabelGenes':
		LabelGenes(pp)
	case 'LabelAll':
		LabelAllClusters(pp)
		ShowLegend(pp)
	case 'ShowGeneTable':
		ShowLegend(pp)
	case 'MatchMap':
		Unify2Maps(pp)
	case 'Show Super Cluster':
		ShowSuperClusters(pp)`;

mgr.SetCustomMenu('Atlas/*', true, scriptStr, "MapSnapshot", null);

scriptStr = `@#MenuLabels 'Embed Groups' 'Show Data'
vv.Import('GeneMonitor.pyn')
match vv.EventSource.Item:
	case 'Embed Groups':
		gList = pp.GetSelectedGroups()
		if gList.Count==0:
			vv.Message('Please select some groups!')
			vv.Return()	
		LoopList(list(gList), epochs=2000, SS=0, EX=4.0, PP=0.05, repeats=1, saveTo=None)
	case 'Show Data':
		ShowData0( list(vv.SelectedItems) )`;

mgr.SetCustomMenu('Atlas/*', true, scriptStr, "GroupManager", null);

}

InstallAtlas();


if ( vv.ScriptDirectories.indexOf( vv.CurrentScriptDirectory ) < 0 )
	vv.ScriptDirectories += ";"+vv.CurrentScriptDirectory;
