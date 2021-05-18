del *.zip
copy bin\Release\ClipRecorder.dll ClipRecorder.dll
zip ClipRecorder.zip ClipRecorder.dll *.js VectorField/*.*
rem call uploadplugin ClipRecorder.zip
rem del ClipRecorder.zip ClipRecorder.dll
