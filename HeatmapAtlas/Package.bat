set ZipFile=HeatmapAtlas.zip
del %ZipFile%
zip %ZipFile%  *.pyn *.png

copy %ZipFile% ..\..\VisuMapWeb\images
