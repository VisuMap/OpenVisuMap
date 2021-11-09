function StagedTraining(loops1=2000, loops2=1000, pp1=0.15, pp2=0.01, exa = 6) {
	var t = New.TsneMap();
	t.MaxLoops = loops1;
	t.Is3D = false;
	t.PerplexityRatio = pp1;
	t.Repeats = 1;
	t.RefreshFreq = 50;
	t.ExaggerationSmoothen = true;
	t.ExaggerationFactor = exa;
	t.AutoScaling = true;

	t.Show().Reset().Start();
	if (t.CurrentLoops != t.MaxLoops ) 
		vv.Return();
	t.AutoNormalizing = true;
	t.AutoScaling = true;
	//New.MapSnapshot().Show();

	t.MaxLoops = loops2;
	t.ExaggerationFactor = 2.5;
	t.PerplexityRatio = pp2;
	t.AutoNormalizing = false;
	t.Restart().Close();
	vv.Map.PcaNormalize();
	New.MapSnapshot().Show();
}

for(var n=0; n<5; n++)
StagedTraining(2000, 1000, 0.15, 0.01, 6.0);


