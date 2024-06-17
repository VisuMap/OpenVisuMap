set ZipFile=SeqModeling.zip
del %ZipFile%
zip %ZipFile%  *.pyn

copy %ZipFile% ..\..\VisuMapWeb\images
