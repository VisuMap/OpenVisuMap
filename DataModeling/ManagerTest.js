// File: ManagerTest.js
//============================

var ms = vv.FindPluginObject("DMScript");

for (var md in ms.AllModels()) 
    vv.Echo(md);

ms.DeleteModel("ModelB")
