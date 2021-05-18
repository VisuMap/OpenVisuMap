// Common.js
function LoadDataset(fn){
	vv.Folder.OpenFile(fn);
	vv.CurrentDirectory = vv.CurrentScriptDirectory + "\\..";
}

function ClickMenu(label) {
	for( var m in vv.TheForm.MainMenuStrip.Items ) {
		if ( m.Text == "Plugins" ) {
			for (var mm in m.DropDownItems) {
				if ( mm.Text == label ) {
					mm.PerformClick();
					return;
				}
			}
			return;
		}
	}
}

var assertIndex = 0;
function Assert(val){
    assertIndex++;
    if (!val) {
        vv.Echo("Assert failed: " + assertIndex); vv.Sleep(500);
    }
}

var mdName = "QaModel";
var testDataset = "QA\\QaTest.xvmz";
vv.CurrentDirectory = vv.CurrentScriptDirectory + "\\..";

