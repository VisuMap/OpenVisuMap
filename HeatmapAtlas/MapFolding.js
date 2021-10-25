// MapFolding.js
//
// Gradually folds a t-SNE map by progressively disable features guided
// by a 1-D t-SNE map.
//
var cs = New.CsObject(`
	public List<IValueItem> SortValueList(List<IValueItem> items) {
		return items.OrderBy(x=>x.Value).ToList();
	}

	public List<string> GetAboveLimit(List<IValueItem> items, double lowLimit) {
		return items.Where(x=>x.Value >= lowLimit).Select(x=>x.Id).ToList();
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
	ExaSrt:6.0,
	ppSrt:0.01,

	mtr:mtrList.cos,
	pp:0.1,
	loop0:5000,
	loop1:2000,
	Exa0:6.0,
	Exa1:1.5,

	reversOrder:false,
	evenSegmentation:false,
};

function ShiftTable() {
	cs.ShiftTable(vv.GetNumberTableView(false), 0.5)
}

function SortColumns(mtr, epochs, ex, pr, reverseOrder) {
	var T = vv.GetNumberTableView(true).Transpose2();
	cs.ShiftTable(T, 1.0);
	var tsne = New.TsneSorter(T, mtr);
	tsne.MaxLoops = epochs;
	tsne.InitExaggeration = ex;
	tsne.PerplexityRatio = pr;
	tsne.RefreshFreq = 50;
	tsne.Show().Start();
	var ColumnSrtKeys = tsne.ItemList;
	tsne.Close();

	if ( reverseOrder ) {
		var maxV = cs.MaxItemValue(ColumnSrtKeys);
		for(var it of ColumnSrtKeys)
			it.Value = maxV - it.Value;
	}
	
	return cs.SortValueList(ColumnSrtKeys);
}

function NewTsne(mtr, loops, exa, pp) {
	var tsne = New.TsneMap();
	tsne.MaxLoops = loops;
	tsne.PerplexityRatio = pp;
	vv.Map.Metric = mtr;
	tsne.ExaggerationSmoothen = true;
	tsne.ExaggerationFactor = exa
	tsne.Repeats = 1;
	tsne.RefreshFreq = 50;
	tsne.AutoNormalizing = true;
	tsne.AutoScaling = true;
	tsne.Show();
	tsne.Reset().Start();
	tsne.InitializeWithMap();
	tsne.AutoNormalizing = false;
	tsne.AutoScaling = false;
	return tsne;
}

function FoldingMap(geneList, tsne, loops, exa, evenSegmentation) {	
	var minValue = geneList[0].Value;
	var maxValue = geneList[geneList.Count-1].Value;
	var range = maxValue - minValue;
	var limitList = [];

	if ( evenSegmentation ) {
		var N = 100;
		var stepSize = range/N;
		for(var n=1; n<N; n++)
			limitList.push(minValue + n*stepSize);
	} else {
		var delta = 0.0025*range;
		var decay = 0.95;
		for(var rest=range; rest>delta; ) {
			limitList.push(minValue+range-rest);
			rest = (rest*(1-decay)>delta) ? (rest*decay) : (rest-delta)
		}
	}

	limitList = limitList.filter((e,i)=>i%2);

	vv.Title = "Total Steps: " + limitList.length;

	var barView = New.BarView(geneList).Show();
	var mapRec = vv.FindPluginObject("ClipRecorder").NewRecorder();
	mapRec.Show().CreateSnapshot();

	var nt = vv.GetNumberTableView(true);
	tsne.ExaggerationFactor = exa;
	tsne.MaxLoops = loops;

	for(var limit of limitList) {
		var selected = cs.GetAboveLimit(geneList, limit);
		vv.EventManager.RaiseItemsSelected(selected);
		var nt2 = nt.SelectColumnsById(selected);
		tsne.ChangeTrainingData(nt2);
		tsne.Restart();
		mapRec.CreateSnapshot();
		nt2.FreeRef();
		if ( tsne.CurrentLoops != loops )
			break;
	}

	return limitList;
}

function PlaySelections(geneList, limitList) {
	for(var limit of limitList) {
		var selected = cs.GetAboveLimit(geneList, limit);
		vv.EventManager.RaiseItemsSelected(selected);
	}
}

var geneList = SortColumns(cfg.mtrSrt, cfg.loopSrt, cfg.ExaSrt, cfg.ppSrt, cfg.reversOrder);
var tsne = NewTsne(cfg.mtr, cfg.loop0, cfg.Exa0, cfg.pp);
var limitList = FoldingMap(geneList, tsne, cfg.loop1, cfg.Exa1, cfg.evenSegmentation);
PlaySelections(geneList, limitList);
tsne.Close();
