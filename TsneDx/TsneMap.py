# Copyright (C) 2020 VisuMap Technologies Inc.
#
# TsneMap.py: Sample script to access TsneDx from python 
#
# Installation: The module 'pythonnet' need to be installed; and this directory
# need to be added to the PYTHONPATH environment variable.
#
import sys
sys.path.append('C:\\work\\OpenVisuMap\\TsneDx\\bin\\Debug')
import clr, os, TsneDx, time
import numpy as np

# ----------------------------------------------

def DoTsneMap(X, perplexityRatio=0.05, maxEpochs=1000, outDim=2, metricType=0):
    tmpFile = 'tmp0123.npy'
    tsne = TsneDx.TsneMap(PerplexityRatio=perplexityRatio, MaxEpochs=maxEpochs, OutDim=outDim, MetricType=metricType)
    np.save(tmpFile, X)
    Y = tsne.FitNumpyFile(tmpFile)
    os.remove(tmpFile)
    #Y = tsne.FitNumpy(X)   # simple way, but slow for large file.
    return np.fromiter(Y, float).reshape(X.shape[0], -1)

def ReduceByPca(X, pcaNumber=50):
    tmpFile = 'tmp0123.npy'
    pca = TsneDx.FastPca()
    np.save(tmpFile, X)
    X1 = pca.DoPcaNumpyFile(tmpFile, pcaNumber)
    os.remove(tmpFile)
    return np.fromiter(X1, float).reshape(X.shape[0], -1)

# ----------------------------------------------

inFile = sys.argv[1]
X = np.genfromtxt(inFile) if inFile.endswith('.csv') else np.load(inFile)

pcaNr = 50
if pcaNr>0:
    X = ReduceByPca(X, pcaNumber=pcaNr)
    print('Data reduced to: ', X.shape)

print('Fitting ', X.shape, ' table...')
beginTime = time.time()
Y = DoTsneMap(X, metricType=0)
endTime = time.time()
print('Fitting finished in %.2f seconds'%(endTime-beginTime))

# display the result
import matplotlib.pyplot as plt
plt.scatter(Y[:,0], Y[:,1], 1)
plt.xlabel('tSNE-1')
plt.ylabel('tSNE-2')
plt.show()

