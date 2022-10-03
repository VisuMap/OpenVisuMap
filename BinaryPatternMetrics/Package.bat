del *.zip
copy bin\Release\BinaryPatternMetrics.dll .
zip BinaryMetrics.zip BinaryPatternMetrics.dll Install.js UnInstall.js SparseSetWordBag.xvmz WordFrequency.js
copy BinaryMetrics.zip ..\..\VisuMapWeb\images
