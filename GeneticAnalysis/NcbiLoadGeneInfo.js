// NcbiLoadGeneInfo.js
// 
// Load a complete genome by accession number from NCBI
//
var accession = "NC_026133";
var seqName = "MyrmicaMT";

var wc = vv.New.Instance("System.Net.WebClient");
var file = vv.New.ClassType("System.IO.File");
var outFile = "c:/temp/" + seqName + ".fasta";
var out = file.CreateText(outFile);
var loadUrl = 'https://eutils.ncbi.nlm.nih.gov/entrez/eutils/efetch.fcgi?db=nucleotide&rettype=fasta&retmode=text&id=' + accession;
out.WriteLine(wc.DownloadString( loadUrl ));
out.Close();
vv.ImportFile(outFile);

var info = New.InfoPad();
info.AppendText("Data source for dataset " + seqName + ": " + loadUrl);
info.Save();
info.Close();

