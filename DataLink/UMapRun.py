# File: UMapRun.py
# 
# Run UMAP on selected data from VisuMap and show the result to VisuMap.
#====================================================================================
print('Loading libraries...')
import sys
#sys.path.insert(0, 'C:/temp/umap-master/umap-master')
import time, types, umap, DataLinkCmd
import numpy as np
from types import SimpleNamespace

pyVersion = sys.version.split(' ')[0]
print('Python: %s; UMAP: %s'%(pyVersion, str(umap.__version__)))

mtrList = SimpleNamespace(e='euclidean', c='correlation', s='cosine', p='precomputed')
initList = SimpleNamespace(s='spectral', r='random', p='pca')

mtr = mtrList.e
initType = initList.s
epochs = 1000
mapDim = 2
nn = 2000
md = 0.99
lc = 20
ns = 20
sp = 25
randomizeOrder = True
stateSeed = None
zeroMean = False

ds = DataLinkCmd.LoadFromVisuMap(mtr)

# centralize the training data
if zeroMean:
   ds = ds - np.mean(ds, axis=0)

print('Fitting data...')
for k in range(2):
#for sp in [20, 30, 40, 50]:
    if randomizeOrder:
        perm = np.random.permutation(ds.shape[0])
        ds = ds[perm]
        if mtr == 'precomputed':
            ds[:,:] = ds[:, perm]
        perm = np.arange(ds.shape[0])[np.argsort(perm)]

    t0 = time.time()
    um = umap.UMAP(n_neighbors=nn, min_dist=md, local_connectivity=lc,
        n_components=mapDim, metric=mtr, negative_sample_rate=ns, random_state=stateSeed,
        n_epochs=epochs, init=initType, learning_rate=1, verbose=True, spread=sp)
    map = um.fit_transform(ds)
    tm = time.time() - t0
    title = 'UMAP: Nbs:%d, MinDist:%g, L.Cnt:%g, N.S.:%d, Spr:%.1f, Mtr:%s, T:%.1fs'%(nn, md, lc, ns, sp, mtr, tm)
    
    if randomizeOrder: # reverse the random order.
        map = map[perm]
        ds = ds[perm]
        if mtr == 'precomputed':
            ds[:,:] = ds[:, perm]
  
    DataLinkCmd.ShowToVisuMap(map, title)

