//File: MapMorph.js
//
//Morphing maps within a group with shared name prefix.
//
//Usage: Create multiple maps with similar settings and shared name prefix.
//Then, activate one map in the main window and run this script.
//

var msg = "Moved bodies: ";
var [loopPause, framePause, frames] = [500, 20, 50];
var repeats = 2;

function Animation(mp, bodyList) {
    var moved = mp.MoveBodiesTo(bodyList, frames, framePause, 0);
    vv.Sleep(loopPause);
    return moved;
}

var mapList = vv.FindFormList("MapSnapshot");
var enabledBodies = vv.Dataset.BodyListEnabled();
var mapList = Array.from(mapList).filter(m=>m.BodyList.Count==enabledBodies.Count);

if ( (pp==vv) && (mapList.length==1) ) {
    var moved = vv.Map.MoveBodiesTo(mapList[0].BodyList, frames, framePause, repeats, loopPause);
    msg = msg + moved;
} else if ( (pp.Name == "MapSnapshot") || (pp.Name == "MdsCluster") || (pp.Name == "D3dRender") ) {
    // Morphing between calling view and other open map snapshots.
    var initBody = New.BodyListClone(pp.BodyList);
    var vwList = New.ObjectArray();
    var f = pp.TheForm;
    var bsCount = pp.BodyList.Count;

    for (var vw of vv.FindFormList(pp.Name))
        if ((vw.TheForm !== f) && (vw.BodyList.Count == bsCount))
            vwList.Add(vw);

    for (rep = 0; rep<repeats; rep++) {
	    for (var vw of vwList) {
	        vw.TheForm.BringToFront();
			  var [left, top] = (f.Width < f.Height) ? [f.Left+f.Width-10, f.Top] : [f.Left, f.Top+f.Height-6]
			  vw.TheForm.SetDesktopLocation(left, top)
	        msg += Animation(pp, vw.BodyList) + ", ";
	    }
	    msg += Animation(pp, initBody) + ", ";
    }
} else {
    // Morphing between maps with the same name prefix.
    var initBody = New.BodyListClone(enabledBodies);
    var initName = vv.Map.Name;
    var mpList = New.StringArray();
    var prefix = initName.substring(0, 1);
    for (var nm of vv.Dataset.MapNameList)
        if (nm.startsWith(prefix) && (nm != initName))
            mpList.Add(nm);

    for (rep = 0; rep<repeats; rep++) {
    	    var fromName = initName;
	    for (var nm of mpList) {
		 var mpBodies = vv.Dataset.ReadMapBodyList(nm, true);
		 if ( mpBodies.Count == enabledBodies.Count) {
		        vv.Title = fromName + "<->" + nm;
		        msg += Animation(vv.Map, mpBodies) + ", ";
		        fromName = nm;
		}
	    }
	    vv.Title = fromName + "<->" + initName;
	    msg += Animation(vv.Map, initBody) + ", ";
    }
}

pp.Title = msg;
