del *.zip
copy bin\Release\GraphMetrics.dll .
zip GraphMetrics.zip GraphMetrics.dll Install.js UnInstall.js
rem call uploadplugin GraphMetrics.zip
