FX = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.17134.0\x64\fxc.exe"
BC = $(FX) /O3 /Tcs_5_0 /E $* /Fo $@ $?

TT = CreateDistanceCache.cso OneStep.cso OneStepNoCache.cso FastStep.cso FastStepS.cso \
 OneStepCpuCache.cso OneStepSumUp.cso CalculateP.cso CalculatePFromCache.cso \
 InitializeP.cso InitializeP3.cso CalculateSumQ.cso \
 CurrentCost.cso CurrentCostLarge.cso PartialDistance2.cso Dist2Affinity.cso

TT2 = PcaCreateCovMatrix.cso PcaInitIteration.cso PcaIterateOneStep.cso \
 PcaCalculateNormal.cso PcaAdjustCovMatrix.cso PcaTransposeEigenvectors.cso PcaReduceMatrix.cso

$(TT):	TsneMap.hlsl
	$(BC)

$(TT2): FastPca.hlsl
	$(BC)

all:	$(TT) $(TT2)

clear:
	del *.cso

list:
	dir/b *.cso
