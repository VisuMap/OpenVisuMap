// File: ShowSelected.js
var sList = pp.SelectedSections();
var exRight = 500;
var exLeft = 500;
var maxLen = 0;
for(var s in sList) maxLen = Math.max(maxLen, s.Length);
maxLen += exLeft + exRight;

var cs = New.CsObject("CopyBytes", "\
	public void Copy(byte[] src, int srcIdx, byte[] dst, int dstIdx, int len) { \
		Array.Copy(src, srcIdx, dst, dstIdx, len); \
	}");

var seq = New.ByteArray(sList.Count * maxLen, 4);
var Array = New.ClassType("System.Array");
var seq0 = pp.SequenceTable;
for(var row=0; row<sList.Count; row++) {
  var s = sList[row];
  var idx0 = Math.max(0, s.Begin-exLeft);
  var len = Math.min(seq0.Length-idx0, s.Length + exRight + exRight)
  cs.Copy(seq0, idx0, seq, row*maxLen, len);
}

var sm = New.SequenceMap(seq, sList.Count, maxLen);
sm.Show();