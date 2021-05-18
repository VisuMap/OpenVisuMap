// GeneCurve.js
var refSeq = pp.SelectedSequence();
var sa = vv.FindPluginObject("SeqAnalysis");
var filter = vv.Folder.OpenSequence2Filter(vv.Map.Filter);

filter.Sequence = refSeq;
filter.StepSize = 0;
filter.Clones = 3;
filter.CloneDecay = 0.15;
filter.ScanningSize = 50;
filter.Save();
vv.Map.SetMetricAndFilter(vv.Map.Metric, vv.Map.Filter);

var op = New.AffinityEmbedding();
op.Is3D = true;
op.CoolingSpeed = 0.2;
op.RefreshFreq = 1000;
op.AffinityRange = 1.0;
op.Show();
op.Reset().Start();
op.Close();

var bodies = New.StringArray();
for(var b in vv.Dataset.BodyList) if ( !b.Hidden && !b.Disabled ) bodies.Add(b.Id);
vv.SelectedItems = bodies;

var dv = New.Map3DView().Show();
dv.DoPcaCentralize();
dv.ClickContextMenu("ShowCurvature.js");
dv.Close();
vv.SelectedItems = null;
