import sys, os
import numpy as np

def LoadPdb(pdbFile):
    chainIdx = []
    posTable = []
    ch2idx = {}
    with open(pdbFile) as inFile:
        for L in inFile:
            if L.startswith('_atom_site.pdbx_PDB_model_num'):
                break
        for L in inFile:
            if L[0] == '#':
                break 
            if not L.startswith('ATOM'):
                continue
            fs = L.split()
            if len(fs) != 21:
                print(f'Invalid record: {len(fs)}: |{L}|')
                quit()
            if fs[3] != 'CA':
                continue
            rsX, rsY, rsZ = float(fs[10]), float(fs[11]), -float(fs[12])
            chName = fs[6] + '_' + fs[20]
            if chName not in ch2idx:
                ch2idx[chName] = len(ch2idx)
            chainIdx.append(ch2idx[chName])
            posTable.append([rsX, rsY, rsZ])
    return np.array(chainIdx), np.array(posTable)

def ChainInterpolate(D, repeats, convexcity):
    if D.shape[0] <= 2: 
        return D
    for n in range(repeats):
        L = D.shape[0]
        K = 2*L - 1
        P = np.ndarray([K, 3])
        P[0] = D[0]
        # initializing the new points with the median points
        for k in range(1, L):
            P[2*k-1] = 0.5*(D[k-1] + D[k])
            P[2*k] = D[k]
        # added the local convexity to the new points.
        for k in range(3, K-2, 2):
            P[k] += convexcity*(2*P[k] - P[k-3] - P[k+3])
        if K > 4:
            P[1] += convexcity*(P[1] - P[4])
            P[K-2] += convexcity*(P[K-2] - P[K-5])
        D = P
    return D


def ConvexInterpolate(chainIdx, posTable, repeats, convexcity):
    N = len(chainIdx)
    posChain = []
    k0, t0 = 0, chainIdx[0]
    for k in range(N+1):
        if (k == N) or (chainIdx[k] != t0):
            D = posTable[k0:k, :]
            D = ChainInterpolate(D, repeats, convexcity)
            posChain.append(D)
            if k < N:
                k0, t0 = k, chainIdx[k]
    posTable = np.vstack(posChain)
    chainIdx = np.array([ len(ch) for ch in posChain ])
    return chainIdx, posTable

if __name__ == '__main__':
    import matplotlib.pyplot as plt
    from TsneMap import CallTsne 

    if len(sys.argv) != 2 or not sys.argv[1].endswith('.cif'):
        print('Usage: TsnePDB <PDB-cif-File>')
        quit()
    chainIdx, posTable = LoadPdb(sys.argv[1])
    chainSize, posTable = ConvexInterpolate(chainIdx, posTable, repeats=3, convexcity=0.1)

    print('Fitting table ', posTable.shape)
    Y = CallTsne(posTable, perplexityRatio=0.15, epochs=1000, mapDim=2, initExaggeration=10.0, finalExaggeration=1.0)
    Y[:,0] = -Y[:,0]
    k0 = 0
    for k, sz in enumerate(chainSize):
        k1 = k0 + sz
        plt.scatter(Y[k0:k1,0], Y[k0:k1,1], k+1)
        k0 = k1
    plt.show()
