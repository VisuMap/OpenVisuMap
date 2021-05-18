// File: NcbiLoadSeq.js
//
// Load a list of nucleotide sequences given by their accession number.
//=====================================================================

var wc = vv.New.Instance("System.Net.WebClient");
var file = vv.New.ClassType("System.IO.File");
var out = file.CreateText("c:/temp/y.fasta");

for(var accession in New.StringArray(
	"NC_024826",
	"NC_026133",
	"NC_013631",
)) {
  var seq = wc.DownloadString(
	  'https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=nucleotide&rettype=fasta&retmode=text&id=' 
	 + accession); 
  out.WriteLine(seq);
}

out.Close();
