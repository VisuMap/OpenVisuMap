# TsneMap.py: Sample script to access TsneDx from python 
#
# Before runing the module 'pythonnet' need to be pre-installed; the path
# to TsneDx directory need to be adjusted.
#
import sys, clr, time, System
import numpy as np
sys.path.append('C:\\work\\OpenVisuMap\\TsneDx\\bin\\Debug')
td = clr.AddReference('TsneDx')
import TsneDx

#---------------------------------------------------------------------

def ToNumpyArray(X):
    X1 = np.zeros((X.GetLength(0), X.GetLength(1)), dtype=np.float32)
    for row in range(X1.shape[0]):
        for col in range(X1.shape[1]):
            X1[row, col] = X[row, col]
    return X1

def ToCsArray(X):
    X = X.astype(np.float32)
    X1 = System.Array.CreateInstance(System.Single, X.shape[0], X.shape[1])
    for row in range(X.shape[0]):
        for col in range(X.shape[1]):
            X1[row, col] = X[row, col]
            X1[row, col] = X[row, col]
    return X1

t0 = time.time()
def Msg(msg):
    global t0
    t = time.time()
    print(msg, '  Time:%.2fs'%(t-t0))
    t0 = t

#---------------------------------------------------------------------

tsne = TsneDx.TsneMap()
tsne.OutDim = 3
tsne.PerplexityRatio = 0.05
tsne.MaxEpochs = 2000

fn = 'tasic'
#fn = 'SP500'

Msg('Test started')
X = np.load(fn +'.npy')
Msg('Loaded data table ' + str(X.shape))
np.save('tmp0123.npy', X)
Msg('Started learning...')
Y = tsne.FitNumpy('tmp0123.npy')

Msg('Completed learning...')
Y = np.reshape(np.fromiter(Y, float), (X.shape[0], -1))
np.savetxt(fn+'_map.csv', Y)
Msg('Map saved')



