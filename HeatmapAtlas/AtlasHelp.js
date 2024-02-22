// AtlasHelp.js
//
// Help functions.
//

var cfg = {
	hm:null,
	refFreq:100,
	seq:0,
	cos:'Correlation.Cosine Distance', 
	euc:'EuclideanMetric', 
	cor:'Correlation.Standard Correlation'
};

function PP(cVal,gVal=cVal){ return {c:cVal, g:gVal}; }

function CheckMaps() {
	if ( (cfg.cellMap==null) || (cfg.geneMap==null) 
		|| !cfg.cellMap.TheForm.Visible 
		|| !cfg.geneMap.TheForm.Visible ) {
		vv.Message("Cell or gene map not created!\nPlease run DualClustering!");
		vv.Return();
	}
}

function ValidateHeatMap(parent) {
	if (parent.Name != "HeatMap"){
		vv.Message('Please call this script from the context menu of a heatmap view.');
		vv.Return(0);
	}
	cfg.hm = parent;
}

function OpenAtlas() {
	var atList = vv.FindFormList("Atlas");
	return (atList.Count>0) ? atList[0] : New.Atlas().Show();
}

function TrainDatasets(dsList, doEmbedding=false, capturing=false) {
	vv.GuiManager.StopFlag = false;
	for(var nm of dsList) {
		vv.Folder.OpenDataset(nm);
		vv.Folder.DataChanged = false;
		var hm = New.HeatMap().Show();
		hm.ClickMenu("Atlas/Dual Sorting");
		if ( vv.GuiManager.StopFlag ) 
			return;
		if ( doEmbedding ) {
			hm.ClickMenu("Atlas/Dual Embedding");
			hm.ClickMenu("Atlas/Save Data");
			if ( capturing )
				cfg.cellMap.ClickMenu("Utilities/Capture Map");
			cfg.cellMap.Close();
			cfg.geneMap.Close();
		} else {
			hm.ClickMenu("Atlas/Save HeatMap");
		}
		hm.Close(); 
	}
	OpenAtlas().Close();
}

function SqueezeFeatures(nt, columns) {
	var N = nt.Columns;
	if ( (columns <= 0) || (columns>=N) )
		return nt;
	var vs = nt.SqueezeRows(1, true);
	var ids = New.StringArray();
	for(var i=N-columns; i<N; i++) 
		ids.Add(vs[i].Id);
	return nt.SelectColumnsById(ids);
}

function ConcatDatasets0(dsList, maxRows=0, refGenes=null) {
   if (dsList.length==0) {
		vv.Message("No dataset selected!");
		vv.Return();
	}

	var nt = New.NumberTable(0,0);
	for(var n=0; n<dsList.length; n++) {
		var t = vv.Folder.ReadDataset(dsList[n]);
		vv.Echo("Dataset: " + dsList[n] + ": " + t.Rows + ", " + t.Columns);
		t = t.GetNumberTableView();
		if ( maxRows != 0 )
			t = t.SelectRowsView(New.Range(maxRows));
		if (n == 0) {
			if (refGenes == null) {
				var colIds = t.ColumnSpecList.ToIdList();
			} else {
				var colIds = refGenes;
				t = t.SelectColumnsById2(colIds, 0.0);		
			}
		} else
			t = t.SelectColumnsById2(colIds, 0.0);
		for(var rs of t.RowSpecList) 
			rs.Type = n;
		var prefix = String.fromCharCode(65+n);
		for(var rs of t.RowSpecList)
			rs.Id = prefix + rs.Id;
		nt.Append(t);
    }

	return nt;
}

function ConcatDatasets(dsList, maxRows=0, refGenes=null) {
	var nt = ConcatDatasets0(dsList, maxRows, refGenes);
	var hm = nt.ShowHeatMap();
	hm.Title = "Datasets: " + dsList.join();
	hm.Description = dsList.join('|');
	return hm;
}

function SelectedDs() {
    var dsList = Array.from(OpenAtlas().GetSelectedItems(), x=>x.Name.trim());
	 dsList = dsList.filter(x=>x.length>0);
	 return dsList;
}

function NewExpressionMap(parent, winTitle) {
	vv.SelectedItems = null;
	var exMap;
	if ( parent.Name == "D3dRender" ) {
		exMap = parent.NewWindow();
	} else {
		exMap = parent.NewSnapshot();
		exMap.ShowMarker(false);
	}

	exMap.GlyphSet="Ordered 64";
	exMap.GlyphOpacity = 0.75;
	exMap.GlyphSize = 1.0;
	exMap.Width = parent.Width;
	exMap.Height = parent.Height;
	exMap.Title = winTitle;
	return exMap;
}

function FlushMarkers(map1, map2, map3) {
	if (map1.Name == "MapSnapshot") {
		if (map2.Name == "MapSnapshot")
			map2.ShowMarker(false);
		if (map3.Name == "MapSnapshot")
			map3.ShowMarker(false);
		for(var i=0; i<4; i++) {
			map1.ShowMarker(false);
			vv.Sleep(250);
			map1.ShowMarker(true);
			vv.Sleep(250);
		}
	}
}

function LayoutMaps() {
	var sz = 600;
	var winWidth = sz;
	var winHeight = sz;
	cfg.hm.TheForm.SetBounds(1000, 700, winWidth, winHeight);
	cfg.cellMap.TheForm.SetBounds(cfg.hm.TheForm.Left - sz + 15, cfg.hm.TheForm.Top, sz, sz);
	cfg.geneMap.TheForm.SetBounds(cfg.hm.TheForm.Left, cfg.hm.TheForm.Top - sz + 8, sz, sz);
	cfg.cellMap.Title = "Cell Map";
	cfg.geneMap.Title = "Gene Map";
}

function SaveSortedTable() {
	var nt = pp.GetNumberTable();
	var info = [];	
	info.push(vv.Dataset.Name+'&'+nt.Rows.toString()+'&'+pp.Width+'&'+pp.Height);
	for(var rs of nt.RowSpecList) info.push(rs.Id);
	for(var cs of nt.ColumnSpecList) info.push(cs.Id);

	if ( vv.ModifierKeys.ControlPressed ) {
		if (pp.Tag != null)
			pp.Tag.Tag = info.join('|');
		return;
	}
	var at = OpenAtlas();

	var ii = at.NewRectItem();	
	ii.Name = vv.Dataset.Name.substr(0,8);
	ii.Tag = info.join('|');
	ii.Top = 50*(++cfg.seq % 15);
	ii.Left = 25;
	ii.FillColor = New.Color('Green');
	ii.Filled = true;
	ii.IsEllipse = false;
	ii.Opacity = 0.75;
	ii.LabelStyle = 3;
	ii.IconHeight = 30;
	ii.IconWidth = 50;
	ii.Script = "!vv.Import('AtlasHelp.js');LoadSortedHeatmap()";
	
	at.RedrawItem(ii);
	return ii.Id;
}

function OpenMapItem(isCellMap) {
		var mp = vv.EventSource.Item.Open();
		if ( isCellMap ) 
			cfg.cellMap = mp;
		else
			cfg.geneMap = mp;

		if ( (cfg.hm==null) || (cfg.hm.TheForm.IsDisposed) ) 
			return;

		mp.AddContextMenu('Atlas/Capture Coloring', '!csFct.CopyType(pp.BodyList, cfg.hm)', 
			isCellMap, null, 'Push the cluster coloring to the heatmap');
		if (isCellMap) {
			mp.Left = cfg.hm.Left - mp.Width + 15;
			mp.Top = cfg.hm.Top;		
			mp.ClickMenu('Atlas/Capture Coloring');
		} else {
			mp.Left = cfg.hm.Left;
			mp.Top = cfg.hm.Top - mp.Height + 8;
			mp.ClickMenu('Atlas/Capture Coloring');
		}
}

function LoadSortedHeatmap() {
   //var t0 = (new Date()).getTime();
	var tbItem = vv.EventSource.Item;
	var vs = New.StringSplit(tbItem.Tag);

	var hmId = (tbItem.Name == "") ? tbItem.Id : tbItem.Name;

	var fs = New.StringSplit(vs[0], '&');
	if ( fs.Count == 1 ) {
		var dsName = vv.Dataset.Name;
		var rows = fs[0] - 0;
	}
   if ( fs.Count >= 2 ){
		var dsName = fs[0];
		var rows = fs[1] - 0;
	}

   var columns = vs.Count-1-rows;
	var rowIds = vs.GetRange(1, rows);	
	var colIds = vs.GetRange(1 + rows, columns);
	if ( dsName != vv.Dataset.Name )
		if (! vv.Folder.OpenDataset(dsName) ) {
			vv.Message('Cannot open dataset "' + dsName + '".');
			vv.Return();
		}
	var nt = vv.GetNumberTableView(false);
	nt = nt.SelectRowsById2(rowIds);
	nt = nt.SelectColumnsById2(colIds, 0);
	cfg.hm = New.HeatMap(nt);
	cfg.hm.SelectionMode=2;
	if ( fs.Count >= 4 ) {		
		cfg.hm.Width = fs[2] - 0;
		cfg.hm.Height = fs[3] - 0;
	}
	cfg.hm.Show2();
	cfg.hm.Tag = tbItem;
	cfg.hm.Title = hmId + ": Sorted table; " + dsName + ": " + rows + "x" + columns;

   //var t1 = (new Date()).getTime();
   //cfg.hm.Title = "Time: " + (t1 - t0)/1000;
}


function ShowDsHm() {	
	var vs = New.StringSplit(vv.EventSource.Item.Tag);
	var fs = New.StringSplit(vs[0], '&');

	var L = fs.Count;
	var dsList = Array.from( fs.GetRange(0, L-2) );
	var rows = fs[L-2]-0;
	var columns = fs[L-1]-0;
	var rowIds = vs.GetRange(1,rows);
	var colIds = vs.GetRange(1+rows, columns)


	var ntList = [];
	for(var k in dsList) {
		var ds = dsList[k];
		var nt = vv.Folder.ReadDataset(ds).GetNumberTableView();
		nt = nt.SelectColumnsById2(colIds, 0);
		for(var rs of nt.RowSpecList) {	
			rs.Type = k-0;
			rs.Id = String.fromCharCode(65+rs.Type)+rs.Id;
		}		
		ntList.push( nt );
	}

	var nt = New.NumberTable(0,0);
	for(var t of ntList)
		nt = nt.Append(t);
	nt = nt.SelectRowsById2(rowIds);
	var hm = nt.ShowHeatMap();
	hm.Description = dsList.join('|');
	hm.Title = "Datasets: " + dsList.join(',');
	return hm;
}

function SaveDsHm(hmParent) {
	if ((hmParent==null)||(hmParent.Name!="HeatMap")) {
		vv.Message("The parent form is not a heatmap");
		vv.Return();
	}
	var dsList = hmParent.Description.split('|');
	if ( (dsList.length==0) ) {
		vv.Message("The parent heatmap does not have dataset list set.");
		vv.Return();
	}
	var allDs = New.HashSet(vv.Folder.DatasetNameList);
	for(var ds in dsList) {
		if ( !allDs.Contains(ds) ) {
			vv.Message("The parent heatmap does not have valid dataset names in its description!");
			vv.Return();			
		}
	}

	var nt = pp.GetNumberTable();
	var rsList = nt.RowSpecList;
	var info = [];
	info.push( dsList.join('&') + '&' + nt.Rows + '&' + nt.Columns );	
	info.push(...nt.RowSpecList.ToIdList());
	info.push(...nt.ColumnSpecList.ToIdList());

	var at = OpenAtlas();

	var ii = at.NewRectItem();	
	ii.Tag = info.join('|');
	ii.Top = 60*(++cfg.seq % 15);
	ii.Left = 25;
	ii.FillColor = New.Color('Yellow');
	ii.Filled = true;
	ii.IsEllipse = false;
	ii.Opacity = 0.75;
	ii.LabelStyle = 2;
	ii.IconHeight = 40; 
	ii.IconWidth = 60;
   ii.Script = "!vv.Import('AtlasHelp.js');ShowDsHm();";
	ii.Name = dsList.join('|');
	at.RedrawItem(ii);
}


var csFct = New.CsObject(`
	public INumberTable ShiftTable(INumberTable nt, double shiftFactor) {
		double[] cm = nt.ColumnMean().Select(it=>it.Value * shiftFactor).ToArray();
		for(int row=0; row<nt.Rows; row++)
			for(int col=0; col<nt.Columns; col++)
				nt.Matrix[row][col] -= cm[col];
		return nt;
	}

	public void CopyType(IList<IBody> bList, IHeatMap hm) {
		if ( (hm==null) || (hm.TheForm.IsDisposed) )
			return;
		INumberTable nt = hm.GetNumberTable();
		bool isCellMap = (bool) vv.EventSource.Item;
		if ( isCellMap )
			for(int i=0; i<bList.Count; i++)
				nt.RowSpecList[i].Type = bList[i].Type;
		else
			for(int i=0; i<bList.Count; i++)
				nt.ColumnSpecList[i].Type = bList[i].Type;
		hm.Redraw();
	}

	public void ShowActiveGenes(IList<string> selectedItems, INumberTable expTable, IForm map) {
		if ( (selectedItems==null) || (selectedItems.Count==0) )
			return;
		INumberTable selected = expTable.SelectRowsById(selectedItems);
		if ( selected.Rows == 0 )
			return;
		var colMean = selected.ColumnMean().Select(it=>it.Value).ToArray();

		var bv = map.Tag as IBarView;
		if (bv.TheForm.IsDisposed || map.TheForm.IsDisposed) return;
		bool is2D = (map.Name == "MapSnapshot");
		var bList = is2D ? (map as IMapSnapshot).BodyList : (map as IMap3DView).BodyList;

		int N = bList.Count;
		double hiThreasHold = 0.75*colMean.Max();
		for(int i=0; i<N; i++) {
			double v = colMean[i];
			bv.ItemList[i].Value = v;
			bv.ItemList[i].Group = (short)((v>hiThreasHold)?0:4);
		}

		for(int i=0; i<N; i++)
			colMean[i] *= colMean[i];
		double minExpr = colMean.Min();
		double maxExpr = colMean.Max();
		double stepSize = (maxExpr - minExpr)/64;
		if ( stepSize <= 0 )
			return;
		for(int i=0; i<N; i++) 
			bList[i].Type = (short) ( (colMean[i] - minExpr)/stepSize );

		bv.Redraw();
		if ( is2D )
			(map as IMapSnapshot).RedrawBodiesType();
		else
			(map as IMap3DView).Redraw();
	}

	public void ShowActiveCells(IList<string> selectedItems, INumberTable expTable, IForm map) {
		if ( (selectedItems==null) || (selectedItems.Count==0) )
			return;
		INumberTable selected = expTable.SelectColumnsById(selectedItems);
		if ( selected.Columns == 0 )
			return;
		var bv = map.Tag as IBarView;
		if (bv.TheForm.IsDisposed || map.TheForm.IsDisposed) return;
		bool is2D = (map.Name == "MapSnapshot");
		var bList = is2D ? (map as IMapSnapshot).BodyList : (map as IMap3DView).BodyList;

		var items = bv.ItemList;
		double overflow = 0;
		int overCount = 0;
		for(int row=0; row<selected.Rows; row++) {
			double rowMean = 0;
			double[] R = (double[])selected.Matrix[row];
			for(int col=0; col<selected.Columns; col++)
				rowMean += R[col];
			rowMean /= selected.Columns;
			items[row].Value = rowMean;
			if ( rowMean > bv.UpperLimit ) {
				overflow += rowMean;
				overCount++;
			}
		}
		if ( overCount > (int)(0.02 * selected.Rows) )
			bv.UpperLimit = overflow/overCount;

		bv.AutoScaling = true;
		double hiMarker = 0.75 * bv.MaxValue();
		for(int i=0; i<items.Count; i++)
			items[i].Group = (short)( (items[i].Value<hiMarker) ? 4 : 0);

		double minExpr = bv.LowerLimit;
		double maxExpr = bv.UpperLimit;
		double stepSize = (maxExpr - minExpr)/64;
		if ( stepSize <= 0 )
			return;
		for(int row=0; row<bList.Count; row++)
			bList[row].Type = (short) ( (items[row].Value - minExpr)/stepSize);

		bv.Redraw();
		if ( is2D )
			(map as IMapSnapshot).RedrawBodiesType();
		else
			(map as IMap3DView).Redraw();
	}

	public void SetRange(INumberTable expTable, IBarView bv) {
		double[] colMean = expTable.ColumnMean().Select(it=>it.Value).ToArray();
		Array.Sort(colMean);
		Array.Reverse(colMean);
		int n = (int)(0.15 * colMean.Length);		
		double sum = 0;
		for(int i=0; i<n; i++)
			sum += colMean[i] * colMean[i];
		bv.UpperLimit = 3* Math.Sqrt(sum/n);
		bv.LowerLimit = 0;		
	}

	public bool SyncKeyColoring(INumberTable nt, List<IValueItem> rowKeys, List<IValueItem> colKeys) {
		if ( (nt==null) || (rowKeys==null) || (colKeys==null) )
			return false;
		var id2RowSpec = nt.RowSpecList.ToDictionary(rs=>rs.Id);
		var id2ColSpec = nt.ColumnSpecList.ToDictionary(cs=>cs.Id);
		var rowMean = rowKeys.Average(it=>it.Value);
		var colMean = colKeys.Average(it=>it.Value);

		foreach(var item in rowKeys) {
			item.Group = id2RowSpec[item.Id].Type;
			item.Value = 2*rowMean - item.Value;
		}
		foreach(var item in colKeys) {
			item.Group = id2ColSpec[item.Id].Group;
			//item.Value = 2*colMean - item.Value;
		}
		return true;
	}
`);

