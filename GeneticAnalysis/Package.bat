setlocal
del *.zip
copy bin\Release\GeneticAnalysis.dll .
set JsList=NcbiCdsPost.js EnsemblShowCDS.js SequenceOp.js ShowSeqMap.js Blast.js BlastDb.js
zip GeneticAnalysis.zip GeneticAnalysis.dll ICSharpCode.SharpZipLib.dll Install.js UnInstall.js %JsList%
rem uploadplugin GeneticAnalysis.zip
endlocal
