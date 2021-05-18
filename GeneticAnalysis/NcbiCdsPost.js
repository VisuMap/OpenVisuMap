// File: NcbiCdsPost.js
// Perform post processing after imported a fasta file with CDS features
// downloaded from NCBI web site.
//
// Example: 
//   1. Goto https://www.ncbi.nlm.nih.gov/nuccore/NC_031699.1
// 
//   2. Choose "Send to"=>Gene Features>FASTA Nucleotide=>Create File. 
//
//   3. Rename to downloaded file to extension *.fasta, e.g. Chr3CDS.fasta;
//      The file name should end with "CDS.fasta"
//      Then drag and drop the file to VisuMap to import it.
//
//   4. Simply Run this script to unpack features info.
//
//   5. It is recommanded to download the whole chromosome as a whole
//      through the choose "Send to=>Complete Record=>File=>Format:FASTA=>Create File.
//      Then drag-and-drop the downloaded file into VisuMap to import it. The newly 
//
var ds = vv.Dataset;

ds.AddColumn("Begin", 0, "0", 3);
ds.AddColumn("End", 0, "0", 4);
ds.AddColumn("Strand", 1, "1", 5);

function GeneName(str) {
	str = str.split("[gene=")[1];
	return str.substring(0, str.indexOf(']'));
}

for(var row=0; row<ds.Rows; row++) {
	var s = ds.GetValueAt(row,2);
	ds.BodyList[row].Name = GeneName(s);
	var idx0 = s.IndexOf("location=");
       if ( idx0 < 0 ) continue;
	idx0 += 9;
	var idx1 = s.IndexOf(']', idx0);
	var f = s.Substring(idx0, idx1 - idx0);

	var strand = 1;
	if ( f.StartsWith("complement") ) {
		f = f.replace("complement", "");
		strand = -1;
		ds.BodyList[row].Type = 1;
	}
	f = f.replace(/\)|\(|\>|\</g, "");

	var fs = f.Replace("join", "").replace(/\.\./g, ",").Split(',');
	var sBegin = "";
	var sEnd = "";
	for(var i=0; (i+1)<fs.Length; i+=2) {
		if (sBegin.Length>0) { 
			sBegin += ","; 
			sEnd +=","; 
		}
		sBegin += fs[i];
		sEnd += fs[i+1];
	}

	ds.SetStringAt(row, 3, sBegin);
	ds.SetStringAt(row, 4, sEnd);
	ds.SetValueAt(row, 5, strand);	
}

vv.Map.GlyphType = "36 Clusters";
vv.Map.Redraw();