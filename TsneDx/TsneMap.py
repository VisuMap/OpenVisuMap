# Copyright (C) 2020 VisuMap Technologies Inc.
#
# TsneMap.py: Sample script to access TsneDx from python 
#
# Installation: The module 'pythonnet' need to be installed; and this directory
# need to be added to the PYTHONPATH environment variable.
#
import sys, os
sys.path.append('C:\\work\\OpenVisuMap\\TsneDx\\bin\\Release')
import clr, TsneDx, time
import numpy as np

inFile = sys.argv[1]
tmpFile = 'tmp0123.npy'
X = np.genfromtxt(inFile) if inFile.endswith('.csv') else np.load(inFile)

def DoTsneMap(X, perplexityRatio=0.05, maxEpochs=1000, outDim=2):
    tsne = TsneDx.TsneMap(PerplexityRatio=perplexityRatio, MaxEpochs=maxEpochs, OutDim=outDim)
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

pcaNr = 0
if pcaNr>0:
    X = ReduceByPca(X, pcaNumber=pcaNr)
    print('Data reduced to: ', X.shape)

print('Fitting %dx%d table...'%(X.shape[0], X.shape[1]))
beginTime = time.time()
Y = DoTsneMap(X, perplexityRatio=0.05, maxEpochs=1000, outDim=2)
endTime = time.time()
print('Fitting finished in %.2f seconds'%(endTime-beginTime))

import matplotlib.pyplot as plt
plt.scatter(Y[:,0], Y[:,1], 1)
plt.xlabel('tSNE-1')
plt.ylabel('tSNE-2')
plt.show()

