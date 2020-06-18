import sys,clr,time
import numpy as np
import System
sys.path.append('C:\\work\\OpenVisuMap\\TsneDx\\bin\\Debug')
td = clr.AddReference('TsneDx')
import TsneDx

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
    return X1

t0 = time.time()
def Msg(msg):
    global t0
    t = time.time()
    print(msg, '  Time:%.2fs'%(t-t0))
    t0 = t

tsne = TsneDx.TsneMap()
tsne.OutDim = 3
tsne.PerplexityRatio = 0.05
tsne.MaxEpochs = 2000

fn = 'tasic'
#fn = 'SP500'

Msg('Test started')
X = np.genfromtxt(fn +'.csv')
Msg('Loaded data table ' + str(X.shape))
X = ToCsArray(X)
Msg('Started learning...')
Y = tsne.Fit32(X)
Msg('Completed learning...')
Y = ToNumpyArray(Y)
np.savetxt(fn+'_map.csv', Y)
Msg('Saved data')



