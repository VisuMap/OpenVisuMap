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
import matplotlib.pyplot as plt

inFile = sys.argv[1]
X = np.genfromtxt(inFile) if inFile.endswith('.csv') else np.load(inFile)

print('Start fitting %dx%d table...'%(X.shape[0], X.shape[1]))
tsne = TsneDx.TsneMap(PerplexityRatio=0.05, MaxEpochs=1000, OutDim=2)
np.save('tmp0123.npy', X)
Y = tsne.FitNumpy('tmp0123.npy')
os.remove('tmp0123.npy')
Y = np.fromiter(Y, float).reshape(X.shape[0], -1)

size = 3 if Y.shape[0] < 1000 else 1
plt.scatter(Y[:,0], Y[:,1], size)
plt.xlabel('tSNE-1')
plt.ylabel('tSNE-2')
plt.show()



