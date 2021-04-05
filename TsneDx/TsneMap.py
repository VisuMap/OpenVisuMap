# Copyright (C) 2020 VisuMap Technologies Inc.
#
# TsneMap.py: Sample script to access TsneDx from python 
#
# Installation: The module 'pythonnet' need to be installed via command 'pip install pythonnet'; 
# and this directory need to be added to the PYTHONPATH environment variable.
#
# Usage: 
#    TsneMap.py  <file-name>
# where <file-name> is a csv or npy file.
#
import sys
sys.path.append('C:\\work\\OpenVisuMap\\TsneDx\\bin\\Release')
import clr, os, TsneDx, time
import numpy as np

# ----------------------------------------------

def DoTsneMap(X, perplexityRatio=0.05, maxEpochs=1000, outDim=2, metricType=0):
    tsne = TsneDx.TsneMap(PerplexityRatio=perplexityRatio, MaxEpochs=maxEpochs, OutDim=outDim, MetricType=metricType)
    X = X.astype(np.float32)
    Y = tsne.FitBuffer(X.__array_interface__['data'][0], X.shape[0], X.shape[1])
    #Y = tsne.FitNumpy(X)   # simple way, but slow for large file.
    return np.fromiter(Y, float).reshape(X.shape[0], -1)

def ReduceByPca(X, pcaNumber=50):
    pca = TsneDx.FastPca()
    X = X.astype(np.float32)
    X1 = pca.DoPcaBuffer(X.__array_interface__['data'][0], X.shape[0], X.shape[1], pcaNumber)
    return np.fromiter(X1, float).reshape(X.shape[0], -1)

# ----------------------------------------------

inFile = sys.argv[1]
X = np.genfromtxt(inFile) if inFile.endswith('.csv') else np.load(inFile)
print('Loaded table ', X.shape)

pcaNr = -100
if pcaNr>0:
    X = ReduceByPca(X, pcaNumber=pcaNr)
    print('Data reduced to: ', X.shape)

print('Fitting table ', X.shape)
t0 = time.time()
Y = DoTsneMap(X, metricType=0)
print('Fitting finished in %.2f seconds'%(time.time()-t0))

# display the result
import matplotlib.pyplot as plt
plt.scatter(Y[:,0], Y[:,1], 1)
plt.xlabel('tSNE-1')
plt.ylabel('tSNE-2')
plt.show()

