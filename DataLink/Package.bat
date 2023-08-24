del *.zip
copy bin\Release\DataLink.dll .
zip DataLink.zip DataLink.dll *.py *.js SP500.csv BH_SNE.exe
copy DataLink.zip ..\..\VisuMapWeb\images
