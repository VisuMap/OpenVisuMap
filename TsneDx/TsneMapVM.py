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
import clr, os, TsneDx, time
import DataLinkCmd as vm
import numpy as np

#=================================
def DoTsneMap(X, perplexityRatio=0.05, maxEpochs=1000, outDim=2, metricType=0):
    tsne = TsneDx.TsneMap()
    tsne.PerplexityRatio = perplexityRatio
    tsne.MaxEpochs = maxEpochs
    tsne.OutDim = outDim
    tsne.MetricType = metricType
    tsne.CacheLimit = 23000
    tsne.MaxCpuCacheSize = 26000
    tsne.ExaggerationInit = 6.0
    tsne.ExaggerationFinal = 1.0

    X = X.astype(np.float32)
    Y = tsne.FitBuffer(X.__array_interface__['data'][0], X.shape[0], X.shape[1])
    #Y = tsne.FitNumpy(X)   # simple way, but slow for large file.
    return np.fromiter(Y, float).reshape(X.shape[0], -1)

def ReduceByPca(X, pcaNumber=50):
    pca = TsneDx.FastPca()
    X = X.astype(np.float32)
    X1 = pca.DoPcaBuffer(X.__array_interface__['data'][0], X.shape[0], X.shape[1], pcaNumber)
    return np.fromiter(X1, float).reshape(X.shape[0], -1)
#=================================


print('Loading data from VisuMap...')
X = vm.LoadFromVisuMap()
print('Loaded table ', X.shape)

pcaNr = -100
if pcaNr>0:
    print('Doing PCA-Reduction on table ', X.shape)
    X = ReduceByPca(X, pcaNumber=pcaNr)
    print('Data reduced to: ', X.shape)

print('Fitting table ', X.shape)
t0 = time.time()
Y = DoTsneMap(X, perplexityRatio=0.025, maxEpochs=100, outDim=2, metricType=0)
print('Fitting finished in %.2f seconds'%(time.time()-t0))
print('Map table: ', Y.shape)
vm.ShowToVisuMap(Y, 't-SNE Embedding')
