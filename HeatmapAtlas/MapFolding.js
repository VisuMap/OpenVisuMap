// MapFolding.js
//
// Gradually folds a t-SNE map by progressively disable features guided
// by a 1-D t-SNE map.
//
var csFct = New.CsObject(`
	public List<IValueItem> SortValueList(List<IValueItem> items) {
		return items.OrderBy(x=>x.Value).ToList();
	}

	public List<string> GetAboveLimit(List<IValueItem> items, double limit) {
		return items.Where(x=>x.Value >= limit).Select(x=>x.Id).ToList();
	}

	public List<string> GetBelowLimit(List<IValueItem> items, double limit) {
		return items.Where(x=>x.Value <= limit).Select(x=>x.Id).ToList();
	}

	public void ShiftTable(INumberTable nt, double shiftFactor) {
		double[] cm = nt.ColumnMean().Select(it=>it.Value * shiftFactor).ToArray();
		for(int row=0; row<nt.Rows; row++)
			for(int col=0; col<nt.Columns; col++)
				nt.Matrix[row][col] -= cm[col];
	}

	public double MaxItemValue(List<IValueItem> items){
		return items.Max(it=>it.Value);
	}
`);


var mtrList = { 
	cos:'Correlation.Cosine Distance', 
	euc:'EuclideanMetric', 
	cor:'Correlation.Standard Correlation',
};

var cfg = {
	mtrSrt:mtrList.cos,
	loopSrt:5000,
	ExaSrt:4.0,
	ppSrt:0.025,

	mtr:mtrList.cos,
	pp:0.15,
	loop0:5000,
	loop1:1000,
	Exa0:6.0,
	Exa1:1.25,

	reversOrder:false,
	accelerated:true,
};

function ShiftTable() {
	csFct.ShiftTable(vv.GetNumberTableView(false), 0.5)
}

function SortColumns(mtr, epochs, ex, pr, reverseOrder) {
	var T = vv.GetNumberTableView(true).Transpose2();
	csFct.ShiftTable(T, 1.0);
	var tsne = New.TsneSorter(T, mtr);
	tsne.MaxLoops = epochs;
	tsne.InitExaggeration = ex;
	tsne.PerplexityRatio = pr;
	tsne.RefreshFreq = 50;
	tsne.Show().Start();

	if (tsne.CurrentLoops != tsne.MaxLoops ) vv.Return();

	var ColumnSrtKeys = tsne.ItemList;
	tsne.Close();

	if ( reverseOrder ) {
		var maxV = csFct.MaxItemValue(ColumnSrtKeys);
		for(var it of ColumnSrtKeys)
			it.Value = maxV - it.Value;
	}
	
	return csFct.SortValueList(ColumnSrtKeys);
}

function NewTsne(mtr, loops, exa, pp) {
	var tsne = New.TsneMap();
	tsne.MaxLoops = loops;
	tsne.Is3D = false;
	tsne.PerplexityRatio = pp;
	vv.Map.Metric = mtr;
	tsne.Repeats = 1;
	tsne.RefreshFreq = 50;
	tsne.ExaggerationSmoothen = true;

	tsne.ExaggerationFactor = exa;
	tsne.AutoNormalizing = true;
	tsne.AutoScaling = true;

	tsne.Show();
	tsne.Reset().Start();
	tsne.InitializeWithMap();
	tsne.AutoNormalizing = false;
	tsne.AutoScaling = false;
	return tsne;
}

function FoldingMap(geneList, tsne, loops, exa, accelerated) {
	var minValue = geneList[0].Value;
	var maxValue = geneList[geneList.Count-1].Value;
	var range = maxValue - minValue;
	var L = [];
	var N = 200;
	for(var n=1; n<N; n++) 
		L.push(n/N);
	if (accelerated) 
		L = L.map(x=>Math.pow(x, 0.3333));
	L = L.map(x=>minValue+x*range);
	var K = N/2;
	L = L.filter((e,i)=>(i%8==1) && (i<K)) . concat(L.slice(K, -1));

	var barView = New.BarView(geneList).Show();
	var mapRec = vv.FindPluginObject("ClipRecorder").NewRecorder();
	mapRec.Show().CreateSnapshot(minValue);
	var nt = vv.GetNumberTableView(true);
	tsne.ExaggerationFactor = exa;
	tsne.MaxLoops = loops;

	var preFeatures = 0;
	for(var i in L) {
		var limit = L[i];
		var selected = csFct.GetAboveLimit(geneList, limit);
		if ( selected.Count < 2 ) break;
		if ( selected.Count == preFeatures ) continue;		
		preFeatures = selected.Count;

		vv.Title = `Step ${i-0+1} of ${L.length} with ${selected.Count} features`;
		vv.Echo(vv.Title);

		vv.EventManager.RaiseItemsSelected(selected);
		var nt2 = nt.SelectColumnsById(selected);
		tsne.ChangeTrainingData(nt2);
		tsne.Restart();
		mapRec.CreateSnapshot(limit);
		nt2.FreeRef();
		if ( tsne.CurrentLoops != loops )
			vv.Return(0);

	}
	return [mapRec, L];

}

function HighlightFeatures() {
	var HFeatureProc = `!
		var srcFrm = vv.EventSource.Form;
		if ( srcFrm == mapRec ) {
			var selected = csFct.GetAboveLimit(geneList, srcFrm.Timestamp);
			vv.EventManager.RaiseItemsSelected(selected);
			vv.Title = "Selected Features: " + selected.Count;
		}
	`;
    	vv.EventManager.OnBodyConfigured(HFeatureProc, mapRec);
}



var geneList = SortColumns(cfg.mtrSrt, cfg.loopSrt, cfg.ExaSrt, cfg.ppSrt, cfg.reversOrder);
var tsne = NewTsne(cfg.mtr, cfg.loop0, cfg.Exa0, cfg.pp);
var [mapRec, limitList] = FoldingMap(geneList, tsne, cfg.loop1, cfg.Exa1, cfg.accelerated);
tsne.Close();
HighlightFeatures();
