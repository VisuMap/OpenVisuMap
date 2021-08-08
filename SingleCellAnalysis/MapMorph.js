//File: MapMorph.js
//
//Morphing maps within a group with shared name prefix.
//
//Usage: Create multiple maps with similar settings and shared name prefix.
//Then, activate one map in the main window and run this script.
//
function Animation(mp, bodyList) {
    var moved = mp.MoveBodiesTo(bodyList, 30, 75, 0);
    vv.Sleep(1000);
    return moved;
}

var msg = "Moved bodies: ";

if ((pp.Name == "MapSnapshot") || (pp.Name == "MdsCluster")) {
    // Morphing between calling view and other open map snapshots.
    var initBody = New.BodyListClone(pp.BodyList);
    var vwList = New.ObjectArray();
    var f = pp.TheForm;
    var bsCount = pp.BodyList.Count;
    for (var vw in vv.FindFormList("MapSnapshot"))
        if ((vw.TheForm !== f) && (vw.BodyList.Count == bsCount))
            vwList.Add(vw);
    for (var vw in vwList) {
        var g = vw.TheForm;
        g.BringToFront();
        var newTop = f.Top - Math.floor((g.Height - f.Height) / 2);
        g.SetBounds(f.Left + f.Width - 10, newTop, 0, 0, 3);
        msg += Animation(pp, vw.BodyList) + ", ";
    }
    Animation(pp, initBody);
    pp.Title = msg;
} else {
    var initBody = New.BodyListClone(vv.Map.BodyList);
    var mpName = vv.Map.Name;
    var mpList = New.StringArray();
    var prefix = mpName.substring(0, 1);
    for (var nm of vv.Dataset.MapNameList)
        if (nm.startsWith(prefix) && (nm != mpName))
            mpList.Add(nm);
    var fromName = mpName;
    for (var nm of mpList) {
        vv.Title = fromName + "<->" + nm;
        msg += Animation(vv.Map, vv.Dataset.ReadMapBodyList(nm)) + ", ";
        fromName = nm;
    }
    vv.Title = msg;
    Animation(vv.Map, initBody);
}
