# Copyright (C) 2020 VisuMap Technologies Inc.
#
# TsneMapVM.py: Sample script to test TsneDx from python.
# This script fetchs data from VisuMap; apply tSNE on it; then displays result on VisuMap.
#
# Installation: The module 'pythonnet' need to be installed; and this directory
# need to be added to the PYTHONPATH environment variable.
#
import sys
sys.path.append('C:\\work\\OpenVisuMap\\TsneDx\\bin\\Release')
import clr, os, TsneDx, time, ModelUtil
import numpy as np

#=================================
tmpFile = 'tmp0123.npy'
def DoTsneMap(X, perplexityRatio=0.05, maxEpochs=1000, outDim=2, metricType=0):
    tsne = TsneDx.TsneMap()
    tsne.PerplexityRatio = perplexityRatio
    tsne.MaxEpochs = maxEpochs
    tsne.OutDim = outDim
    tsne.MetricType = metricType
    tsne.CacheLimit = 23000
    tsne.MaxCpuCacheSize = 26000
    tsne.ExaggerationFactor = 4.0
    tsne.ExaggerationRatio = 0.2

    np.save(tmpFile, X)
    Y = tsne.FitNumpyFile(tmpFile)
    os.remove(tmpFile)
    #Y = tsne.FitNumpy(X)   # simple way, but slow for large file.
    return np.fromiter(Y, float).reshape(X.shape[0], -1)

def ReduceByPca(X, pcaNumber=50):
    pca = TsneDx.FastPca()
    np.save(tmpFile, X)
    X1 = pca.DoPcaNumpyFile(tmpFile, pcaNumber)
    os.remove(tmpFile)    
    return np.fromiter(X1, float).reshape(X.shape[0], -1)
#=================================

log = ModelUtil.Logger()

print('Loading data from VisuMap...')
X = log.LoadTable(dsName='+')
if len(X) == 0: X = log.LoadTable(dsName='@')
print('Loaded table ', X.shape)

pcaNr = -100
if pcaNr>0:
    print('Doing PCA-Reduction on table ', X.shape)
    X = ReduceByPca(X, pcaNumber=pcaNr)
    print('Data reduced to: ', X.shape)

print('Fitting table ', X.shape)
t0 = time.time()
Y = DoTsneMap(X, perplexityRatio=0.025, maxEpochs=100, metricType=0)
print('Fitting finished in %.2f seconds'%(time.time()-t0))

log.ShowMatrix(Y, view=4)
