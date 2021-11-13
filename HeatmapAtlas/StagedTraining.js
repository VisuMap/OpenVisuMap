function StagedTraining(loops=2500, pp=0.10, exa=12.0) {
	var t = New.TsneMap();
	t.MaxLoops = loops;
	t.Is3D = false;
	t.PerplexityRatio = pp;
	t.Repeats = 1;
	t.RefreshFreq = 50;
	t.ExaggerationSmoothen = true;
	t.ExaggerationFactor = exa;
	t.AutoScaling = true;
	t.AutoNormalizing = false;
	vv.Title = "PP: " + pp;

	t.Show().Reset().Start();
	if (t.CurrentLoops != t.MaxLoops ) 
		vv.Return();

	t.ExaggerationFactor = 1.0;

	for(var n=0; n<3; n++) {
		t.MaxLoops = parseInt(0.5*t.MaxLoops);
		t.PerplexityRatio = 0.5*t.PerplexityRatio;
		vv.Title = "PP: " + t.PerplexityRatio;
		t.Restart();
		if (t.CurrentLoops != t.MaxLoops ) 
			vv.Return();
	}
	vv.Map.PcaNormalize();
	t.Close();
}

for(var n=0; n<3; n++) {
	StagedTraining();
	New.MapSnapshot().Show();
}

