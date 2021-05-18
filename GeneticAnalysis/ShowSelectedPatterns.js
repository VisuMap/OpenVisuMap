// ShowSelectedPatterns.js
var ss = pp.SelectedSections();
var L = 0;
for(var s in ss) {
	L = Math.max(L, s.End - s.Begin + 1);
}
var sm = New.SequenceMap(null, ss.Count, L);
for(var i=0; i<ss.Count; i++) 
	sm.SetSequence(pp.GetSequence(ss[i].Begin, ss[i].End), i*L);	
sm.Show();