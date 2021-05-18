//File: SeqProfiling.js
//  Provide various sequence related services.
//MenuLabels ToDyad GcDensity Peaks Peaks2 Log1Trans

var cs = New.CsObject("\
	const byte cA = 0;\
	const byte cC = 1;\
	const byte cG = 2;\
	const byte cT = 3;\
	public void ToDyad(byte[] s, int idx0, int idx1) {\
		idx0 = Math.Min(s.Length-1, Math.Max(0, idx0));\
		idx1 = Math.Min(s.Length-1, Math.Max(0, idx1));\
		for(int i=idx0; i<=idx1; i++) {\
			if( s[i] == cC ) s[i] = cG;\
			else if (s[i]==cT) s[i] = cA;\
		}\
	}\
	public float[] GcDensity(byte[] s, int winSize) {\
		float[] values = new float[s.Length];\
		bool[] win = new bool[winSize];\
		for(int i=0; i<winSize; i++)\
			win[i] = ((s[i]==cC)||(s[i]==cG));\
		int gcCnt = win.Count(b=>b);\
		values[winSize-1] = (float)gcCnt;\
		int n = 0; /* index to the head and tail in th moving window. */\
		for(int i=winSize; i<s.Length; i++) {\
			if (win[n])\
				gcCnt--;\
			win[n] = ((s[i]==cC)||(s[i]==cG));\
			if (win[n])\
				gcCnt++;\
			values[i] = gcCnt;\
			n++;\
			if (n==winSize)\
				n = 0;\
		}\
		for(int i=0; i<values.Length; i++)\
			values[i] /= winSize;\
		return values;\
	}\
	public float[] ExtractPeaks(float[] values, int peakWidth) {\
		int I = peakWidth;\
		int minSep = I / 2;\
		int L = values.Length;\
		float mv = 0.05f * values.Max();\
		float[] ret = new float[L];\
		int pPeakIdx = -1;\
		float pPeak = 0;\
		for(int i=0; i<L; i+=I) {\
			int maxIdx = -1;\
			float maxValue = 0;\
			int J = Math.Min(L-1, i+I);\
			for(int j=i; j<J; j++) {\
				if ( values[j] > maxValue ) {\
					maxIdx = j;\
					maxValue = values[j];\
				}\
			}\
			if ( (maxIdx == (J-1))  || (maxValue<mv) )\
				continue;\
			else {\
				if ( (maxIdx-pPeakIdx)>minSep ) {\
					ret[maxIdx] = maxValue;\
					pPeakIdx = maxIdx;\
					pPeak = maxValue;\
				} else {\
					if (pPeak < maxValue) {\
						ret[pPeakIdx] = 0;\
						ret[maxIdx] = maxValue;\
						pPeakIdx = maxIdx;\
						pPeak = maxValue;\
					}\
				}\
			}\
		}\
		return ret;\
	}\
	public double[][] GetPeakSignatures(float[] peaks, float[] densities, int binNr, int binSize) {\
		int rows = peaks.Count(v=>(v!=0));\
		int columns = binNr;\
		double[][] m = new double[rows][];\
		for(int r=0; r<rows; r++) m[r] = new double[columns];\
		int left = binSize*(binNr-2)/2;\
		int row=0;\
		for(int i=0; i<peaks.Length; i++) {\
			if ( peaks[i] != 0 ) {\
				for(int col=0; col<columns; col++)\
					m[row][col] = densities[i-left+col*binSize];\
				row++;\
			}\
		}\
		return m;\
	}\
	public float[] SubSeries(float[] s, int beginIndex, int length) {\
		float[] ret = new float[length];\
		Array.Copy(s, beginIndex, ret, 0, length);\
		return ret;\
	}\
	public void Log1Trans(float[] s) {\
		for(int i=0; i<s.Length; i++) {\
			s[i] = (float)Math.Log(1.0 + s[i]);\
		}\
	}\
\
");
// =================================================================================

var sa = vv.FindPluginObject("SeqAnalysis");

/*
var winSize = 100;  // moving window size to estimate the GC-densities.
var binSize = 20;  // about a histone size.
var binNr = 15;     // number of bined GC density dimensions.  

var winSize = 50;  // moving window size to estimate the GC-densities.
var binSize = 25;  // about a histone size.
var binNr = 10;     // number of bined GC density dimensions.  
*/

// for Chr7C02M22.wig
var winSize = 250;  // moving window size to estimate the GC-densities.
var binSize = 250;  // about a histone size.
var binNr = 7;     // number of bined GC density dimensions.  

function ShowValues(densities, title) {
	var bbv = New.BigBarView(densities);
	bbv.Rows = ( pp.Name == "SequenceMap" ) ? pp.MapRows : pp.Rows;
	bbv.BaseLineType = 6;
	bbv.BaseLocation = pp.BaseLocation;
	bbv.BaseValueExplicit = 0.5;
	bbv.AutoScaling = false;
	bbv.LowerLimit = 0;
	bbv.UpperLimit = 1.0;
	bbv.Title = title;
	bbv.AutoScaling = true;
	bbv.Show();
	return bbv;
}

var label = vv.EventSource.Item ;
switch( label ) {
	case "ToDyad":
		var seq = pp.SequenceTable;
		cs.ToDyad(seq, 0, seq.Length-1);
		pp.Redraw();
		break;

	case "GcDensity":			
		var seq = ( pp.Name == "SequenceMap" ) ? pp.SequenceTable : sa.OpenSequence("Chr7").AllAsBytes();
		var densities = cs.GcDensity(seq, winSize);
		ShowValues(densities, "Density with window size " + winSize)
		break;

	case "Peaks":
	case "Peaks2":
		var seq = sa.OpenSequence("Chr7");
		var densities = cs.GcDensity(seq.AllAsBytes(), winSize);
		var peaks = cs.ExtractPeaks(pp.Values, binSize * binNr);
		if ( peaks.Length != seq.Length )
			densities = cs.SubSeries(densities, pp.BaseLocation, pp.Values.Length);		
		var m = cs.GetPeakSignatures(peaks, densities, binNr, binSize);

		if (label == "Peaks2") {
			ShowValues(peaks, "Peaks with window size " + winSize);
			ShowValues(densities, "Density with window size " + winSize);
		}
		New.MdsCluster(New.NumberTable(m)).Show();
		break;
	case "Log1Trans":
		cs.Log1Trans(pp.Values);
		pp.ValuesChanged = true;
		pp.Redraw();	
		break;
}



