// AtlasHelp.js
//
// Help functions.
//

var cfg = {
	hm:null,
	refFreq:100,
	cos:'Correlation.Cosine Distance', 
	euc:'EuclideanMetric', 
	cor:'Correlation.Standard Correlation'
};

function PP(v1, v2){ return {c:v1, g:v2}; }

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

function SortTable(T, mt, epochs, ex, pr) {
	var tsne = New.TsneSorter(T, mt);
	tsne.MaxLoops = epochs;
	tsne.InitExaggeration = ex;
	tsne.PerplexityRatio = pr;
	tsne.RefreshFreq = cfg.refFreq;
	tsne.StagedTraining = true;
	tsne.Repeats = 1;
	tsne.Show().Start();
	if (isNaN(tsne.ItemList[0].Value)) {
		vv.Message("Training degraded!\nPlease try with smaller initial exaggeration.");
		vv.Return(1);
	}
	if ( tsne.CurrentLoops != tsne.MaxLoops)
		vv.Return();
	tsne.Close();
};

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
	var sz = 450;
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

	var atList = vv.FindFormList("Atlas");
	var at = (atList.Count>0) ? atList[0] : New.Atlas().Show();

	var ii = at.NewRectItem();	
	ii.Tag = info.join('|');
	if ( ii.Id.length > 1 ) {
		var idx = (ii.Id.substr(1) - 0)%15;
		ii.Top += 15*idx;
		ii.Left+= 10*idx;
	}
	ii.FillColor = New.Color('Green');
	ii.Filled = true;
	ii.IsEllipse = false;
	ii.Opacity = 0.5;
	ii.IconHeight = ii.IconWidth = 40;
	ii.Script = vv.CurrentScriptDirectory + "\\LoadSortedHeatmap.js";
	

	at.SetSelectedItems( ii );
	at.Redraw();
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
			mp.ClickContextMenu('Atlas/Capture Coloring');
		} else {
			mp.Left = cfg.hm.Left;
			mp.Top = cfg.hm.Top - mp.Height + 8;
			mp.ClickContextMenu('Atlas/Capture Coloring');
		}
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

		double minExpr = colMean.Min();
		double maxExpr = colMean.Max();
		double stepSize = (maxExpr - minExpr)/64;
		if ( stepSize <= 0 )
			return;
		for(int i=0; i<bList.Count; i++) {
			bList[i].Type = (short) ( (colMean[i] - minExpr)/stepSize );
			bv.ItemList[i].Value = colMean[i];
		}
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
