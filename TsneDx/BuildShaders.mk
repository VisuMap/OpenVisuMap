FX = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.17134.0\x64\fxc.exe"
BC = $(FX) /O3 /Tcs_5_0 /E $* /Fo $@ $?

TT = CreateDistanceCache.cso IterateOneStep.cso IterateOneStepNoCache.cso \
 EuclideanNoCache.cso EuclideanNoCacheS.cso \
 IterateOneStepSumUp.cso CalculateP.cso CalculatePEuclidean.cso \
 InitializeP.cso InitializeP3.cso CalculateSumQ.cso \
 CurrentCost.cso CurrentCostLarge.cso

$(TT):	TsneMap.hlsl
	$(BC)

all:	$(TT)

clear:
	del *.cso

list:
	dir/b *.cso
