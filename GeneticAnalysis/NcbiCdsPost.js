// File: NcbiCdsPost.js
// Perform post processing after imported a fasta file with CDS features
// downloaded from NCBI web site.
//
// Example: 
//   1. Goto NCBI site for nucleotides, i.e. https://www.ncbi.nlm.nih.gov/nuccore/NC_031699.1
// 
//   2. Choose "Send to"=>Gene Features>FASTA Nucleotide=>Create File. 
//
//   3. Rename to downloaded file to extension *.fasta, e.g. Chr3CDS.fasta;
//      The file name should end with "CDS.fasta"
//      Then drag and drop the file to VisuMap to import it.
//
//   4. Run this script to unpack features info.
//
//   5. It is recommanded to download the whole chromosome as a whole
//      through the choose "Send to=>Complete Record=>File=>Format:FASTA=>Create File.
//      Then drag-and-drop the downloaded file into VisuMap to import it. The newly 
//

vv.Import("GaHelp.js");

var ds = vv.Dataset;

ds.AddColumn("Begin", 0, "0", 3);
ds.AddColumn("End", 0, "0", 4);
ds.AddColumn("Strand", 1, "1", 5);

cs.ExtractGeneInfo(ds);

vv.Map.GlyphType = "36 Clusters";
vv.Map.Redraw();
