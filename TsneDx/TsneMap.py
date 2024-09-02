# Copyright (C) 2024 VisuMap Technologies Inc.
#
# TsneMap.py: Sample script to access TsneDx from python 
#
# Usage: 
#    TsneMap.py  <file-name>
#
# where <file-name> is a csv or npy file.
#
import sys, os, time
import numpy as np

def CallTsne(X, perplexityRatio=0.05, epochs=1000, mapDim=2, initExaggeration=4.0, finalExaggeration=1.0):
    np.savetxt('TsneData.csv', X)
    os.system(f'TsneDx.exe TsneData.csv {perplexityRatio} {epochs} {mapDim} {initExaggeration} {finalExaggeration}')
    return np.genfromtxt('TsneData_map.csv', delimiter=',')

if __name__ == '__main__':
    if len(sys.argv) < 2:
        print('Usage TsneMap.py <csv-data-file>')
        quit()

    inFile = sys.argv[1]
    X = np.genfromtxt(inFile) if inFile.endswith('.csv') else np.load(inFile)
    print('Loaded table ', X.shape)

    print('Fitting table ', X.shape)
    t0 = time.time()
    Y = CallTsne(X, perplexityRatio=0.05, epochs=1000, mapDim=2, initExaggeration=4.0, finalExaggeration=1.0)
    print('Fitting finished in %.2f seconds'%(time.time()-t0))

    import matplotlib.pyplot as plt
    plt.scatter(Y[:,0], Y[:,1], 1)
    plt.show()

