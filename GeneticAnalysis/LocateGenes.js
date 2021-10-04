// LocateGenes.js
//
// show genes embedded in a chromosome sequence map.
//
// Usage: Load a dataset with CDS informations (i.e. table with gene-begin-index, gene-length, Strand etc.);
// The dataset name must also be the name of seq blob that contains the complete chromesome sequences.
// Then call this script.
//
var sa = vv.FindPluginObject("SeqAnalysis");
var ds = vv.Dataset;
var hm = OpenSequenceMap(sa, ds);

for(var i=0; i<3; i++) 
	hm.Regions[i].Clear();

var geneRegions = hm.Regions[0];
var antiGenes = hm.Regions[2];
var exomeRegions = hm.Regions[1];
var antiExomes = hm.Regions[3];

geneRegions.Name = "Genes";
antiGenes.Name = "AntiGenes";
exomeRegions.Name = "Exomes";
antiExomes.Name = "AntiExomes";

geneRegions.Color = exomeRegions.Color =New.Color("Blue");
antiGenes.Color = antiExomes.Color = New.Color("Green");

exomeRegions.Opacity = antiExomes.Opacity =0.5;
antiGenes.Opacity = geneRegions.Opacity = 1.0;

var rs = New.ClassType("VisuMap.Script.RegionStyle");

geneRegions.RegionStyle = rs.Bar;  
antiGenes.RegionStyle = rs.BottomLine;
exomeRegions.RegionStyle = antiExomes.RegionStyle = rs.BottomHalf;

hm.ClearItems();
hm.Redraw();

var selected = vv.AllItems;
var senseColumnIdx = ds.IndexOfColumn("Strand");

for(var tId of selected) {
	var antiSense = ( (senseColumnIdx>=0) && ( ds.GetDataAt(ds.IndexOfRow(tId), senseColumnIdx) == "-1" ) );
	var sec = LocateOneGene(tId, antiSense);
	hm.AddItem(tId, sec.Begin, sec.End);
	(antiSense ?	antiGenes : geneRegions).Add( sec.Shift(-hm.BaseLocation) );
}
hm.Redraw();

function LocateOneGene( transId, antiSense ) {
	var iBegin = New.IntArray();
	var iEnd = New.IntArray();
	var rowIdx = ds.IndexOfRow(transId);  

	if ( rowIdx < 0 ) {
		vv.Message("Invalid Id: " + transId);
		return;
	}

	var exBegins = New.IntArray(ds.GetDataAt(rowIdx, 3));
	var exEnds = New.IntArray(ds.GetDataAt(rowIdx, 4));

	if ( exBegins.Count == 1 ) {
		// there is no splicing, we just mark the gene regions
		return New.SequenceInterval(exBegins[0]-1, exEnds[0]-1);
	}

	iBegin.AddRange(exBegins);
	iEnd.AddRange(exEnds);

	for(var exIdx=0; exIdx<exBegins.Count; exIdx++) {
		var exSec = New.SequenceInterval(exBegins[exIdx]-1, exEnds[exIdx]-1);
		(antiSense?antiExomes:exomeRegions).Add( exSec.Shift(-hm.BaseLocation) );
	}

	for(var k=0; k<iBegin.Count; k++) { iBegin[k]--; iEnd[k]--; }
	
	var minIdx = 1000 * 1000000;
	var maxIdx = -1000;
	for(var n=0; n<iBegin.Count; n++) {
		minIdx = Math.min(minIdx, iBegin[n]);
		maxIdx = Math.max(maxIdx, iBegin[n]);
		minIdx = Math.min(minIdx, iEnd[n]);
		maxIdx = Math.max(maxIdx, iEnd[n]);
	}
	return New.SequenceInterval(minIdx, maxIdx);
}

function OpenSequenceMap(sa, ds) {
	var blobs = vv.Folder.GetBlobList();
	var nm = ds.Name.replace(".CDS","").replace("CDS", "");
	var idx = blobs.IndexOf(nm);
	nm = ( idx < 0 ) ? blobs[0] : blobs[idx];
	var sv = sa.OpenSequence(nm);
	
	var rows = 40;
	var N = sv.Length;
	var columns = parseInt(N / rows, 10) + ( (N%rows > 0) ? 1 : 0 );
	var NN = rows * columns;
	var seqTable = New.ByteArray(NN);	
	
	sv.FetchSeqIndex(0, sv.Length, seqTable, 0);            
	var sm = New.SequenceMap(seqTable, rows, columns).Show();
	sm.Title = sm.SequenceName = nm;
       sm.ReadOnly = true;
	return sm;
}
