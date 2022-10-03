del *.zip
copy bin\Release\NumericMetrics.dll .
zip NumericMetrics.zip NumericMetrics.dll Install.js UnInstall.js
copy NumericMetrics.zip ..\..\VisuMapWeb\images
