//File: DualTest.js
//
var biases = New.NumberArray(0, 1, 2, 4, 8, 16);

for(var n=0; n<biases.Count; n++) {
  if ( n != 0 ) 
    vv.Dataset.AddMap();
  vv.SetProperty("SingleCell.LinkBias", biases[n]);  
  vv.Map.GetMetric().ReInitialize();
  vv.Title = n + ": " + biases[n];
  var sne = New.TsneMap();
  sne.Show();
  sne.PerplexityRatio = 0.01;
  sne.ExaggerationRatio = 0.2;
  sne.MaxLoops = 500;
  sne.Start().Close();
}