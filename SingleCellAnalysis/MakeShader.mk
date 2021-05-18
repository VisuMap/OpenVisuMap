FX = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.17134.0\x64\fxc.exe"
TGT = DotProduct.cso
BC = $(FX) /O3 /Tcs_5_0 /E $* /Fo $@ $?

$(TGT): DualMetric.hlsl
	$(BC)

all: $(TGT)
