del TsneDx.zip
mkdir TsneDx
for %%f in (TsneDx.exe SharpDX.Direct3D11.dll SharpDX.dll SharpDX.DXGI.dll TsneDx.exe.config ) do copy bin\Release\%%f TsneDx
copy Readme.md TsneDx
copy SP500.csv TsneDx
zip -r TsneDx.zip TsneDx
rmdir/s /Q TsneDx
