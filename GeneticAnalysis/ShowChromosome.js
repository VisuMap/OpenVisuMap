// ShowChromosome.js
var sa = vv.FindPluginObject("SeqAnalysis");

var bnList = vv.Folder.GetBlobList();
var gnName = vv.Dataset.Name.Replace("CDS", "");
if ( bnList.IndexOf(gnName) < 0 ) 
	gnName = bnList[0];

var sv = sa.OpenSequence(gnName);
var seqTable = New.ByteArray(sv.Length);
sv.FetchSeqIndex(0, sv.Length, seqTable, 0);            
var sm = New.SequenceMap(seqTable, 50, Math.ceil(sv.Length / 50));
seqTable = null;
sm.Show().Title = "Genome: " + gnName;
sm.SequenceName = gnName;	
