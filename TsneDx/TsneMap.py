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
X = np.genfromtxt(inFile) if inFile.endswith('.csv') else np.load(inFile)

doPca = False
if doPca:
    # Reduce the input data to 25 dimension.
    pca = TsneDx.FastPca()
    X = np.fromiter(pca.DoPcaNumpy(X, 25), float).reshape(X.shape[0], -1)
    print('Reduced data to ', X.shape)    

print('Start fitting %dx%d table...'%(X.shape[0], X.shape[1]))
tsne = TsneDx.TsneMap(PerplexityRatio=0.05, MaxEpochs=1000, OutDim=2)

np.save('tmp0123.npy', X)
Y = tsne.FitNumpyFile('tmp0123.npy')
os.remove('tmp0123.npy')
#Y = tsne.FitNumpy(X)   # simple way, but slow for large file.
Y = np.fromiter(Y, float).reshape(X.shape[0], -1)

import matplotlib.pyplot as plt
plt.scatter(Y[:,0], Y[:,1], 1)
plt.xlabel('tSNE-1')
plt.ylabel('tSNE-2')
plt.show()

