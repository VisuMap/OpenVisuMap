function StagedTraining(loops=1000, pp=0.15, exa=12.0) {
	var t = New.TsneMap();
	t.MaxLoops = loops;
	t.PerplexityRatio = pp;
	t.ExaggerationFactor = exa;
	t.Is3D = false;
	t.Repeats = 1;
	t.RefreshFreq = 10;
	t.ExaggerationSmoothen = true;
	t.AutoScaling = true;
	t.AutoNormalizing = false;
	vv.Title = "PP: " + pp;
	t.Show().Reset().Start();
	if (t.CurrentLoops != t.MaxLoops ) 
		vv.Return();

	t.ExaggerationFactor = 1.0;
	for(var stage=0; stage<3; stage++) {
		t.MaxLoops = parseInt(t.MaxLoops/2);
		t.PerplexityRatio /= 2;
		vv.Title = "PP: " + t.PerplexityRatio;
		t.Restart();
		if (t.CurrentLoops != t.MaxLoops ) 
			vv.Return();
	}
	vv.Map.PcaNormalize();
	t.Close();
}

StagedTraining();
vv.Dataset.AddMap();
StagedTraining();
vv.Dataset.AddMap();
StagedTraining();

