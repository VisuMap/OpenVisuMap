del *.zip
copy bin\Release\GraphMetrics.dll .
zip GraphMetrics.zip GraphMetrics.dll Install.js UnInstall.js
copy GraphMetrics.zip ..\..\VisuMapWeb\images
