del *.zip
copy bin\Release\DataCleansing.dll .
zip DataCleansing.zip DataCleansing.dll *.js
del DataCleansing.dll
rem uploadplugin DataCleansing.zip
copy DataCleansing.zip ..\..\VisuMapWeb\images\DataCleansing.zip
