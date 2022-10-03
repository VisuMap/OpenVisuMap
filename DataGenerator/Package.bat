del *.zip
copy bin\Release\DataGenerator.dll .
zip DataGenerator.zip DataGenerator.dll Install.js UnInstall.js ScriptSample.js
copy DataGenerator.zip ..\..\VisuMapWeb\images
