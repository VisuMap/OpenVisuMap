//VariationTracing.js

var cs = New.CsObject(`
	public void UpdateValues(IBarView bv, INumberTable ntBase, 
			IList<string> selected, double[] refValues) {
		var htSelected = new HashSet<string>(selected);
		var rsList = ntBase.RowSpecList;
		var M = ntBase.Matrix;
		double[][] selectedRows = Enumerable.Range(0, ntBase.Rows)
			.Where(row=>htSelected.Contains(rsList[row].Id))
			.Select(row=>M[row] as double[]).ToArray();
		double[] colMean = VisuMap.MathUtil.ColumnMean(selectedRows);
		if ( colMean == null ) colMean = new double[ntBase.Columns];
		for(int col=0; col<ntBase.Columns; col++)
			bv.ItemList[col].Value = colMean[col] - refValues[col];
		bv.Redraw();	
	}

	public double[] ItemsToArray(IList<IValueItem> items) {
		return items.Select(item=>item.Value).ToArray();
	}

	public double MaxValue(double[] values) {
		return values.Max();
	}
`);

//var ntBase = vv.GetNumberTableView(true);
var refBase = ntBase.SelectRowsByIdView(vv.SelectedItems).ColumnMean();

var bv = New.BarView(refBase);
bv.Tag = cs.ItemsToArray(refBase);
bv.ReadOnly = true;
bv.AutoScaling = false;
bv.DisableReorder = true;
bv.Horizontal=false;
var maxV = cs.MaxValue(bv.Tag);
if ( maxV == 0 ) maxV = 40;
bv.UpperLimit = 1.2*maxV;
bv.LowerLimit = -1.2*bv.UpperLimit;
bv.BaseLineType = 0;
bv.Show();
vv.Tag = ntBase;

bv.AddEventHandler("ItemsSelected", `!
	if ( vv.EventSource.Item == pp.TheForm ) 
		vv.Return();
	pp.Title = "Data points selected: " + vv.SelectedItems.Count;
	cs.UpdateValues(pp, vv.Tag, vv.SelectedItems, pp.Tag);
`);
