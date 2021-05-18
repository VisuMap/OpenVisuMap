del *.zip
copy bin\Release\SingleCellAnalysis.dll .
copy bin\Release\Python.Runtime.dll .
set JsFiles=Install.js UnInstall.js Utilities.js MapMorph.js FeatureMap.js
set PyFiles=SciKitTsne.py UMapRun.py HdbscanRun.py h5adread.py FastTsneRun.py AlgCompare.pyn
set SrcFiles=*.cs MakeShader.mk DualMetric.hlsl SingleCellAnalysis.csproj
zip SingleCellAnalysis.zip SingleCellAnalysis.dll Python.Runtime.dll %JsFiles% %PyFiles% %SrcFiles%

del SingleCellAnalysis.dll 
del Python.Runtime.dll

rem uploadplugin SingleCellAnalysis.zip
