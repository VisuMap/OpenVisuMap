del TsneDx.zip
rmdir/s /Q TsneDx
mkdir TsneDx
for %%f in (TsneDx.exe SharpDX.Direct3D11.dll SharpDX.dll SharpDX.DXGI.dll TsneDx.exe.config ) do copy bin\Release\%%f TsneDx
copy Readme.md TsneDx
copy SP500.csv TsneDx
copy TsneMap.py TsneDx
copy TsnePDB.py TsneDx
cd TsneDx
zip ..\TsneDx.zip *
cd ..
