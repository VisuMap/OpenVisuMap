del *.zip
copy bin\Release\DataModeling.dll .
set JsFiles=Install.js UnInstall.js TrainModel.js ManagerTest.js BatchJobs.js 
set PyFiles=FFClassification.md.py FFRegression.md.py FFModel.md.py Test.md.py ^
  FullCnn.md.py ReTrain.md.py Autoencoder.md.py CustomTarget.md.py ^
  MultiLane.md.py Segregated.md.py PCA.md.py KerasClassification.md.py ^
  ModelUtil.py ModelLogger.py ShowGraph.py UMapRun.py ^
  Keras.ev.py NodeMap.ev.py Eval.ev.py Test.ev.py Autoencoder.ev.py ^
  ModelServer.py ServerUtil.py GraphMap.client.js Test.client.js Eval.client.js 
zip DataModeling.zip Readme.txt License.txt DataModeling.dll %JsFiles% %PyFiles%

del DataModeling.dll
rem uploadplugin DataModeling.zip
