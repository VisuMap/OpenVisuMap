//File: SequenceOp.js
//  Provide various sequence related services.
//MenuLabels Randomize Reverse Revert Flip CpGRatio atRatio Hide Show HideShow ToRegion1 ToRegion2 SeRegion1 SeRegion2 SeRegion3 SeRegion4 RevSelection Reset MergeSelections FlipSelections DynoFlip RightAlignment LeftAlignment SortRowsByLength BuildItems CpGDensity ExtSelection

vv.Import("GaHelp.js");

var seq = pp.SequenceTable;
var label = vv.EventSource.Item;
var ss = pp.SelectedSections();
var s = ( ss.Count >= 1 ) ? ss[0] : New.SequenceInterval(0,seq.Length-1);
var sa = vv.FindPluginObject("SeqAnalysis");

switch( label ) {
	case "Randomize":
		cs.Randomize(seq, s.Begin, s.End);
		break;

	case "Reverse":
		cs.Reverse(seq, s.Begin, s.End);
		break;

	case "Revert":
		cs.Revert(seq, s.Begin, s.End);
		break;

	case "Flip":
		cs.Flip(seq, s.Begin, s.End);
		break;

	case "FlipSelections":
		var sList = sa.UnionIntervals(ss);
		for(var s of sList)
			cs.Flip(seq, s.Begin, s.End);
		break;

	case "DynoFlip":
		var m0 = cs.GetMoment( sa.SequenceToBytes(pp.GetSequence(0, pp.MapColumns-1)) );
		for(var row=1; row<pp.MapRows; row++) {
			var rBegin = row*pp.MapColumns;
			var rEnd = (row+1)*pp.MapColumns - 1;
			var R = sa.SequenceToBytes(pp.GetSequence(rBegin, rEnd));
			var m = cs.GetMoment( R );
			cs.Flip(R, 0, R.Length-1);
			var mf = cs.GetMoment( R );

			var d = 0;
			var df = 0;
			for(var k=0; k<4; k++) {
				d += Math.abs(m[k] - m0[k]);
				df += Math.abs(mf[k] - m0[k]);
			}
			if ( df < d ) {
				sa.Flip(seq, rBegin, rEnd);
			}
		}
		break;

	case "CpGRatio":
		var cgCnt=cs.CpGCount(seq, s.Begin, s.End);
		var ratio = 100*2*cgCnt/(s.End-s.Begin+1);
		vv.Message("CpG Ratio: " + ratio.toPrecision(3) + "%");
		break;

	case "atRatio":
		var atCnt=cs.atCount(seq, s.Begin, s.End);
		var ratio = 100*2*atCnt/(s.End-s.Begin+1);
		vv.Message("ApT Ratio: " + ratio.toPrecision(3) + "%");
		break;

	case "Hide":
		if ( ss.Count == 0 )
			cs.Hide(seq, 0, seq.Length-1);
		else
			for(var s of ss)  
				cs.Hide(seq, s.Begin, s.End);	
		break;

	case "Show":
		cs.Hide(seq, 0, seq.Length-1);
		for(var s of ss) 
			cs.Show(seq, s.Begin, s.End);		
		break;

	case "HideShow":
		cs.HideShow(seq, s.Begin, s.End);
		break;

	case "ToRegion1":
		for(var s of ss) 
			cs.Show(seq, s.Begin, s.End);
		pp.SelectionToRegion(0);
		break;

	case "ToRegion2":
		for(var s of ss) 
			cs.Show(seq, s.Begin, s.End);
		pp.SelectionToRegion(1);
		break;

	case "SeRegion1":
	case "SeRegion2":
	case "SeRegion3":
	case "SeRegion4":
		if ( ! vv.ModifierKeys.ControlPressed )	
			pp.ClearSelection();
		var rIdx = parseInt(label.substring(label.length-1), 10) - 1;
		pp.AddSelections(pp.Regions[rIdx]);
		break;

	case "Reset":
		pp.ClearSelection();
		pp.ClearItems();
		for(var i=0; i<pp.Regions.Count; i++) pp.Regions[i].Clear();
		cs.Show(seq, 0, seq.Length-1);
		break;

	case "RevSelection":
		var currentSelection = pp.SelectedSections();
		var revSelection = sa.ComplementaryIntervals(currentSelection, 0, pp.SequenceTable.Length-1);
		pp.ClearSelection();
		pp.AddSelections(revSelection);

		break;

	case "MergeSelections":
		var sList = pp.SelectedSections();
		//sList = sa.UnionIntervals(sList);

		if ( sList.Count == 0 ) {
			vv.Message("Empty Intersections.");
			vv.Return();
		}

		var maxLength = 0;
		var totalLength = 0;
		for(var i=0; i<sList.Count; i++) {
			maxLength = Math.max(maxLength, sList[i].Length);
			totalLength += sList[i].Length;
		}

		if ( ! vv.ModifierKeys.ControlPressed ) {
			var sm = New.SequenceMap(null, sList.Count, maxLength);
			for(var i=0; i<sList.Count; i++) {
				sm.SetSequence(pp.GetSequence(sList[i].Begin, sList[i].End), i*maxLength)
			}
			sm.Show();
		} else {
			var gapSize = Math.floor(0.02*totalLength/sList.Count);
			gapSize = Math.max(50, gapSize);
			totalLength += gapSize * (sList.Count - 1);
			var rows = Math.min(100, sList.Count);
			var columns = Math.floor(totalLength / rows) + 1;
			var sm = New.SequenceMap(null, rows, columns);
			var loc = 0;

			for(var i=0; i<sList.Count; i++) {
				var seq = pp.GetSequence(sList[i].Begin, sList[i].End);
				sm.SetSequence(seq, loc)
				loc += seq.Length + gapSize;
			}
			sm.Show();
		}
		break;

	case "RightAlignment":
		for(var row=0; row<pp.MapRows; row++) {
			cs.RightAlignment(seq, row*pp.MapColumns, pp.MapColumns);
		}
		break;

	case "LeftAlignment":
		for(var row=0; row<pp.MapRows; row++) {
			cs.LeftAlignment(seq, row*pp.MapColumns, pp.MapColumns);
		}
		break;

	case "SortRowsByLength":
		cs.SortRows(seq, pp.MapRows, pp.MapColumns, pp);
		break;

	case "BuildItems": // build a region section for each consecutive sequence.
		var secList = cs.BuildSections(seq);
		var bv = New.BarView();
		pp.ClearItems();
		var b = pp.BaseLocation;
		var tb = New.FreeTable(secList.Count, 4);
		pp.Regions[5].Name = "Visible sequences";
		pp.Regions[5].Clear();
		for(var i=0; i<secList.Count; i++) {
			var s = secList[i];
			var id = "s" + i;
			pp.AddItem(id, b+s.Begin, b+s.End);
			pp.Regions[5].Add(s.Begin, s.End);

			bv.ItemList.Add(New.ValueItem(id, null, s.Length));
			tb.RowSpecList[i].Id = id;

			tb.Matrix[i][0] = sa.BytesToSequence(seq, s.Begin, Math.min(40, s.End - s.Begin+1));
			tb.Matrix[i][1] = "" + (b+s.Begin);
			tb.Matrix[i][2] = "" + (b+s.End);
			tb.Matrix[i][3] = "" + s.Length;
		}
		tb.ColumnSpecList[0].Id = "MotifSeq";
		tb.ColumnSpecList[1].Id = "SeqBegin";
		tb.ColumnSpecList[2].Id = "SeqEnd";
		tb.ColumnSpecList[3].Id = "SeqLength";
		for(var col=1; col<4; col++) tb.ColumnSpecList[col].DataType = 'n';
		bv.Show();
		tb.ShowAsTable();
		break;

	case "CpGDensity":
		var pList = 
			"ACG"
			//"AAA CCC GGG TTT"
			//"A C G T"
			//"AA CC GG TT"
			//"CG GC CC GG"
			//"AT TA CG GC"
			//"AC CA AG GA AT TA CG GC CT TC GT TG CC GG AA TT"
		;
		pList = pList.Split(' ');
		var vc = New.ViewContainer();
		for(var i=0; i<pList.Length; i++) {
			var p = pList[i];
			var values = cs.FindPattern(pp.SequenceTable, p);
			var bv = New.BigBarView(values);
			bv.ShortName = p;
			bv.TheForm.FormBorderStyle=0;
			bv.BaseLocation = pp.BaseLocation - p.Length + 1;
			vc.Add(bv);
			bv.Show();
			bv.TheForm.SetBounds(0,0, vc.Width-28, 16, 12); // 12: only set the size.
			bv.TheForm.Anchor = 13; // anchor to top,left and right.
			vc.Title = (i+1) + ": " + p ;
		}
		vc.TileWindows()
		break;

	case "ExtSelection": // Extends selections on both end with 500 nb.
		var L = 500;
		var nss = New.SequenceIntervalList();
		for(var s in ss) {
			nss.Add(New.SequenceInterval(s.Begin-L, s.End+L));
		}
		pp.ClearSelection();
		pp.AddSelections(nss);
		pp.Refresh();
		break;
}

pp.Redraw();

