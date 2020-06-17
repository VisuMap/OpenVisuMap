import sys,clr
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

tsne = TsneDx.TsneMap()
tsne.OutDim = 3
tsne.PerplexityRatio = 0.05
tsne.MaxEpochs = 2000

fn = 'tasic'
X = np.genfromtxt(fn +'.csv')
print('Loaded data table: ', X.shape)
X = ToCsArray(X)
print('Started learning...')
Y = tsne.Fit32(X)
print('Completed learning...')
Y = ToNumpyArray(Y)
np.savetxt(fn+'_map.csv', Y)



