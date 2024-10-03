set ZipFile=SeqModeling.zip
del %ZipFile%
zip %ZipFile%  *.pyn TestData.xvm SeqModeling.cs SeqModeling.dll

copy %ZipFile% ..\..\VisuMapWeb\images
