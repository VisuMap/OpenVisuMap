function Reverse(st) {
        return st.split('').reverse().join('');
}

function Invert(st) {
	var s = st.split("");
	for(var i=0; i<s.length; i++) {
		var z = s[i];
		s[i] =  (z=='G') ? 'C' : (z=='C') ? 'G' :       (z=='A') ? 'T' : (z=='T') ? 'A' :
			(z=='g') ? 'c' : (z=='c') ? 'g' :       (z=='a') ? 't' : (z=='t') ? 'a' : z;
	}
	return s.join("");
}

function InvertReverse(s) {
	return Invert(Reverse(s));
}

function ShowSeq(seq) {
	var seqLength = seq.Length;
	var nt = New.NumberTable(1, seqLength);
	for(var i=0; i<nt.Columns; i++) nt.Matrix[0][i] = 1 + "ACGT".IndexOf(seq[i]);
	var hm = nt.ShowHeatMap();
	var seqTitle = ( seqLength < 200 ) ? seq :	
		( seq.Substring(0, 100) + " ...... " + seq.Substring(seqLength-100, 100) );
	hm.Title = seqLength + ": " + seqTitle;
	hm.Show();
	nt = null;
}