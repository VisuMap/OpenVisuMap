//File: MapMorph.js
//
//Morphing maps within a group with shared name prefix.
//
//Usage: Create multiple maps with similar settings and shared name prefix.
//Then, activate one map in the main window and run this script.
//

var [loopPause, framePause, frames] = [500, 50, 50];
var repeats = 2;
var movedList = [];

function Animation(mp, bodyList) {
    var moved = mp.MoveBodiesTo(bodyList, frames, framePause, 0);
    vv.Sleep(loopPause);
    return moved;
}

if ( "MapSnapshot|D3dRender|MdsCluster".includes(pp.Name) ) {
    // Morphing between calling view and other open map snapshots.
    var initBody = New.BodyListClone(pp.BodyList);
    var vwList = New.ObjectArray();
    var f = pp.TheForm;
    var bsCount = pp.BodyList.Count;
	 f.BringToFront();
    for (var vw of vv.FindFormList(pp.Name)) {
		  var wStat = vw.TheForm.WindowState.ToString();
        if ((vw.TheForm !== f) && (wStat == "Normal") && (vw.BodyList.Count == bsCount))
            vwList.Add(vw);
	}

	if ( vwList.Count == 0 ){
		vv.Message("No similar maps have been found for comparison");
		vv.Return();
	}

   for (rep = 0; rep<repeats; rep++) {
	    for (var vw of vwList) {
	        vw.TheForm.BringToFront();
			  var [left, top] = (f.Width <= f.Height) ? [f.Left+f.Width-10, f.Top] : [f.Left, f.Top+f.Height-6]
			  vw.TheForm.SetDesktopLocation(left, top)
	        movedList.push ( Animation(pp, vw.BodyList) );
	    }
	    movedList.push( Animation(pp, initBody) );
   }

} else {
    // Morphing between maps with the same name prefix.
	 var enabledBodies = vv.Dataset.BodyListEnabled();
    var initBody = New.BodyListClone(enabledBodies);
    var initName = vv.Map.Name;
    var mpList = New.StringArray();
    var prefix = initName.substring(0, 1);
    for (var nm of vv.Dataset.MapNameList)
        if (nm.startsWith(prefix) && (nm != initName))
            mpList.Add(nm);
    var movedList = [];

    for (rep = 0; rep<repeats; rep++) {
    	 var fromName = initName;
	    for (var nm of mpList) {
		     var mpBodies = vv.Dataset.ReadMapBodyList(nm, true);
		     if ( mpBodies.Count == enabledBodies.Count) {
		         vv.Title = fromName + "<->" + nm;
		         movedList.push( Animation(vv.Map, mpBodies) );
		         fromName = nm;
		     }
	    }
	    vv.Title = fromName + "<->" + initName;
	    movedList.push( Animation(vv.Map, initBody) );
    }

}

var avg = movedList.reduce((a,b)=>a+b) / movedList.length;
pp.Title = "Moved bodies: " + movedList.toString() + ".  Average: " + avg.toFixed(2);
