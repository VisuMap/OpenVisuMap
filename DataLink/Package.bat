del *.zip
copy bin\Release\DataLink.dll .
zip DataLink.zip DataLink.dll *.py *.js SP500.csv
del DataLink.dll
copy DataModeling.zip ..\..\VisuMapWeb\images
