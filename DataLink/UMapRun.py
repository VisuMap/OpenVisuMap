# File: UMapRun.py
# 
# Run UMAP on selected data from VisuMap and show the result to VisuMap.
#====================================================================================
print('Loading libraries...')
import sys
sys.path.insert(0, 'C:/temp/umap-master/umap-master')
import time, types, umap
import numpy as np
import DataLinkCmd as vm
from types import SimpleNamespace

pyVersion = sys.version.split(' ')[0]
print('Python: %s; UMAP: %s'%(pyVersion, str(umap.__version__)))

A = SimpleNamespace(s='spectral', r='random', p='pca')
M = SimpleNamespace(e='euclidean', c='correlation', s='cosine', p='precomputed')
nr = 0
ds = None

#====================================================================================

PX = lambda aL, bL: [(x,y) for x in aL for y in bL]

def ResetTest():
    global mtr, A0, epochs, mapDim, randomizeOrder, stateSeed
    global nn, md, lc, ns, sp
    mtr = M.e
    A0 = A.s
    epochs = 2000
    mapDim = 2
    randomizeOrder = False
    stateSeed = None
    nn = 500
    md = 0.1
    lc = 5
    ns = 20
    sp = 15

def DoTest():
    global ds, nr
    if ds is None:        
        ds = vm.LoadFromVisuMap(mtr)
        # centralize the training data
        ds = ds - np.mean(ds, axis=0)
    print('Fitting data...')  

    if randomizeOrder:
        #ds = (2*np.random.randint(2, size=ds.shape[1]) - 1) * ds
        perm = np.random.permutation(ds.shape[0])
        ds = ds[perm]
        if mtr == 'precomputed':
            ds[:,:] = ds[:, perm]
        perm = np.arange(ds.shape[0])[np.argsort(perm)]

    t0 = time.time()
    um = umap.UMAP(n_neighbors=nn, min_dist=md, local_connectivity=lc,
        n_components=mapDim, metric=mtr, negative_sample_rate=ns, random_state=stateSeed,
        n_epochs=epochs, init=A0, learning_rate=1, verbose=True, spread=sp)
    map = um.fit_transform(ds)
    tm = time.time() - t0
    nr += 1
    title = 'UMAP.%d: nn:%d, md:%g, lc:%g, ns:%d, sp:%.1f, mtr:%s, A0:%s, T:%.1fs'%(nr, nn, md, lc, ns, sp, mtr, A0, tm)
    
    if randomizeOrder: # reverse the random order.
        map = map[perm]
        ds = ds[perm]
        if mtr == 'precomputed':
            ds[:,:] = ds[:, perm]
  
    vm.ShowToVisuMap(map, title)

#====================================================================================

ResetTest()
sp = 25
ns = 50
lc = 3
for nn in [200, 1000, 2000]:
	DoTest()

#vm.DataLinkCmd().RunScript('New.Atlas().Show().CaptureAllOpenViews().Close()')

'''
for md in [0.1, 0.4, 0.8]: DoTest()

for mtr, nn in PX([M.s], [500, 1000, 2000]): DoTest()

for k in [0,1,2]: DoTest()

for A0 in [A.s, A.r, A.p]: DoTest()

for mtr in [M.e, M.c, M.s]: DoTest()

for nn in [200, 1000, 2000]: DoTest()

for md in [0.1, 0.5, 0.9]: DoTest()

for lc in [3, 5, 10]: DoTest()

for ns in [5, 15, 25]: DoTest()

for sp in [5, 15, 25]: DoTest()
'''
