// EnsemblShowCDS.js
//
// Display coding and no-coding genes in a chromosome view.
//
// This service can only be called from the context menu of a chromosome SequenceMap view.
//
// The current dataset must contain a list of the gene sequences. The dataset should be
// imported from a fasta file format as downloaded from the FTP site from ensembl.org. E.g. from 
// ftp://ftp.ensembl.org/pub/release-90/fasta/mus_musculus/ncrna/Mus_musculus.GRCm38.ncrna.fa.gz
// The header of each fasta sequence must contain 14 fields separated by ':'.
// The chromosome sequence for the sequence map should also be downloaded from ensembl site. E.g.
// from the location: ftp://ftp.ensembl.org/pub/release-90/fasta/mus_musculus/dna/.
//
// When calling this script first time with a gene table as current dataset, this script
// will unpack the header column and set the identifier of all rows to the transcript id.
//========================================================================================
//

var ds = vv.Dataset;
var fs;

if ( ds.Columns == 3 ) {  
  // the dataset is not yet unpacked, we do it now.
  fs = ds.GetDataAt(0, 2).Split(':');
  if ( fs.Length != 14 ) {  // It is not a ensembl fasta header.
    vv.Message("Invalid data format.");
    vv.Return(1);
  }

  ds.AddColumn("Chromosome", 0, '', 3);
  ds.AddColumn("Sense",      0, '1', 4);
  ds.AddColumn("ChrType",    0, '', 5);
  ds.AddColumn("LogBegin",   1, '0', 6);
  ds.AddColumn("LocEnd",     1, '0', 7);

  var bs = ds.BodyList;
  for(var row=0; row<ds.Rows; row++) {
  	fs = ds.GetDataAt(row, 2).Split(':');
	var chr = fs[2];
	var sense = fs[5].Split(' ')[0];
       var geneName = fs[6].Split(' ')[0];
	var chrType = fs[7];
	var locBegin = fs[3] - 0;
	var locEnd = fs[4] - 0;
	var tId = fs[0].Split(' ')[0];

	bs[row].Id = tId;
	bs[row].Name = geneName;
	ds.SetDataAt(row, 3, chr);
	ds.SetDataAt(row, 4, sense);
	ds.SetDataAt(row, 5, chrType);
	ds.SetDataAt(row, 6, locBegin);
	ds.SetDataAt(row, 7, locEnd);
  }

  ds.CommitChanges();
}

if ( pp.Name != "SequenceMap" ) vv.Return(0);

var fs = pp.Title.Split(':');
var myChr=fs[1].Split('.')[4];
var rg = pp.Regions[vv.ModifierKeys.ControlPressed ? 1 : 0];
rg.RegionStyle = vv.ModifierKeys.ControlPressed ? 6 : 5;
rg.Clear();

if (! vv.ModifierKeys.ControlPressed ) pp.ClearItems();

for(var row=0; row<ds.Rows; row++) {
  if ( ds.GetDataAt(row, 3) == myChr ) {
    var locBegin = ds.GetValueAt(row, 6);
    var locEnd =  ds.GetValueAt(row, 7);
    rg.Add(locBegin, locEnd);
    pp.AddItem(ds.BodyList[row].Id, locBegin, locEnd);
  }
}

pp.Redraw();
