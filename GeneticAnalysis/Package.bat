setlocal
del *.zip
copy bin\Release\GeneticAnalysis.dll .

set JsList=NcbiCdsPost.js EnsemblShowCDS.js SequenceOp.js GaHelp.js ShowSeqMap.js ^
 Blast.js BlastDb.js LocateGenes.js MarkExomes.js
set MiscFiles=GeneticAnalysis.zip GeneticAnalysis.dll ^
 ICSharpCode.SharpZipLib.dll Install.js UnInstall.js 

zip %MiscFiles% %JsList%
rem uploadplugin GeneticAnalysis.zip
copy GeneticAnalysis.zip ..\..\VisuMapWeb\images
endlocal
