del *.zip
copy bin\Release\CustomImporter.dll .
zip CustomImporter.zip CustomImporter.dll Install.js UnInstall.js ImportScript.js
copy CustomImporter.zip ..\..\VisuMapWeb\images
