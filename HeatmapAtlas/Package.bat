set ZipFile=HeatmapAtlas.zip
del %ZipFile%
zip %ZipFile%  *.js *.pyn MenuIcon.png

copy %ZipFile% ..\..\VisuMapWeb\images
