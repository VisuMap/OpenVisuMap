// GuiTest.js
// Simple test of the GUI interface.
//!import "Common.js"

vv.ReadOnly = true;
var menuList = New.StringArray(
	"Model Training", 
	"Model Evaluation", 
	"Model Manager"
);

LoadDataset(testDataset);

for (var lb in menuList) {
	ClickMenu(lb);
	if (lb == "Model Manager")
		lb = "Working Directory"
	var fm = vv.FindWindow(lb);
	vv.Sleep(1000);
	Assert(fm.Title.StartsWith(lb));
	fm.Close();
}	
