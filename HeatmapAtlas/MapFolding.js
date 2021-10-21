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
`);

//cs.ShiftTable(vv.GetNumberTableView(false), 0.5)

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

	mtr:mtrList.euc,
	pp:0.1,
	loop0:5000,
	loop1:1900,
	Exa0:6.0,
	Exa1:2.0,

	RefFreq:50,
};

function SortColumns(mtr, epochs, ex, pr) {
	var T = vv.GetNumberTableView(true).Transpose2();
	cs.ShiftTable(T, 1.0);
	var tsne = New.TsneSorter(T, mtr);
	tsne.MaxLoops = epochs;
	tsne.InitExaggeration = ex;
	tsne.PerplexityRatio = pr;
	tsne.RefreshFreq = cfg.RefFreq;
	tsne.Show().Start();
	var ColumnSrtKeys = tsne.ItemList;
	tsne.Close();
	return cs.SortValueList(ColumnSrtKeys);
}

function NewTsne() {
	var tsne = New.TsneMap();
	tsne.MaxLoops = cfg.loop0;
	tsne.PerplexityRatio = cfg.pp;
	tsne.ExaggerationSmoothen = true;
	tsne.ExaggerationFactor = cfg.Exa0
	tsne.Repeats = 1;
	tsne.RefreshFreq = cfg.RefFreq
	tsne.AutoNormalizing = true;
	tsne.AutoScaling = false;
	tsne.Show();
	tsne.Reset().Start();
	tsne.InitializeWithMap();
	tsne.AutoNormalizing = false;
	return tsne;
}

function FoldingMap(geneList, tsne) {	
	var minValue = geneList[0].Value;
	var maxValue = geneList[geneList.Count-1].Value;
	var range = maxValue - minValue;

	var limitList = [];
	var delta = 0.005*range;
	var decay = 0.9;
	for(var rest=range; rest>delta; ) {
		limitList.push(minValue+range-rest);
		rest = (rest*(1-decay)>delta) ? (rest*decay) : (rest-delta)
	} 
	vv.Title = "Total Steps: " + limitList.length;

	var barView = New.BarView(geneList).Show();
	var mapRec = vv.FindPluginObject("ClipRecorder").NewRecorder();
	mapRec.Show().CreateSnapshot();

	var nt = vv.GetNumberTableView(true);
	tsne.ExaggerationFactor = cfg.Exa1;
	tsne.MaxLoops = cfg.loop1;

	for(var limit of limitList) {
		var selected = cs.GetAboveLimit(geneList, limit);
		vv.EventManager.RaiseItemsSelected(selected);
		var nt2 = nt.SelectColumnsById(selected);
		tsne.ChangeTrainingData(nt2);
		tsne.Restart();
		mapRec.CreateSnapshot();
		nt2.FreeRef();
		if ( tsne.CurrentLoops != cfg.loop1 )
			break;
	}
}

var geneList = SortColumns(cfg.mtrSrt, cfg.loopSrt, cfg.ExaSrt, cfg.ppSrt);
var tsne = NewTsne();
FoldingMap(geneList, tsne);
tsne.Close();

