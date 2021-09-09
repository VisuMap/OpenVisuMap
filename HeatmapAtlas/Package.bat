set ZipFile=HeatmapAtlas.zip
del %ZipFile%
zip %ZipFile%  *.js *.pyn

copy %ZipFile% ..\..\VisuMapWeb\images
