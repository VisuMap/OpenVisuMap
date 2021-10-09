// LocateGenes.js
//
// show genes embedded in a chromosome sequence map.
//
// Usage: Load a dataset with CDS informations (i.e. table with gene-begin-index, gene-length, Strand etc.);
// The dataset name must also be the name of seq blob that contains the complete chromesome sequences.
// Then call this script.
//

vv.Import("GaHelp.js");

var noExomes = vv.ModifierKeys.ControlPressed;

var sa = vv.FindPluginObject("SeqAnalysis");
var ds = vv.Dataset;
var hm = OpenSequenceMap(sa, ds);

//
// Configure the styles of the genes and exomes 
// 
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

exomeRegions.Opacity = antiExomes.Opacity = 0.25;
antiGenes.Opacity = geneRegions.Opacity = 1.0;

var rs = New.ClassType("VisuMap.Script.RegionStyle");

if (noExomes) {
	geneRegions.RegionStyle = rs.TopHalf;  
	antiGenes.RegionStyle = rs.BottomHalf;
} else {
	geneRegions.RegionStyle = rs.TopLine;  
	antiGenes.RegionStyle = rs.BottomLine;
	exomeRegions.RegionStyle = rs.TopHalf;
	antiExomes.RegionStyle = rs.BottomHalf;
}

hm.ClearItems();
hm.Redraw();

//
// Extract the genes and exomes specifications.
//
var senseColumnIdx = ds.IndexOfColumn("Strand");

if (noExomes) 
	exomeRegions = antiExomes = null;

for(var tId of vv.AllItems) {
	var antiSense = ( (senseColumnIdx>=0) && ( ds.GetDataAt(ds.IndexOfRow(tId), senseColumnIdx) == "-1" ) );
	var sec = cs.LocateOneGene(ds, hm, exomeRegions, antiExomes, tId, antiSense);
	hm.AddItem(tId, sec.Begin, sec.End);
	(antiSense ?	antiGenes : geneRegions).Add( sec.Shift(-hm.BaseLocation) );
}
hm.Redraw();

