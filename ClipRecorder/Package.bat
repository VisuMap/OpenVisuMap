del *.zip
copy bin\Release\ClipRecorder.dll ClipRecorder.dll
zip ClipRecorder.zip ClipRecorder.dll *.js VectorField/*.*
copy ClipRecorder.zip ..\..\VisuMapWeb\images
