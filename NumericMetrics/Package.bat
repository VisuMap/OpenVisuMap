del *.zip
copy bin\Release\NumericMetrics.dll .
zip NumericMetrics.zip NumericMetrics.dll Install.js UnInstall.js
rem call uploadplugin NumericMetrics.zip
