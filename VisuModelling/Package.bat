set ZipFile=VisuModelling.zip
del %ZipFile%
zip %ZipFile%  *.js *.pyn *.png

copy %ZipFile% ..\..\VisuMapWeb\images
