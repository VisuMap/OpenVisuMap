# File: UMapRun.py
# 
# Run UMAP on selected data from VisuMap and show the result to VisuMap.
#====================================================================================
print('Loading libraries...')
import sys, time, types, umap
import numpy as np
import DataLinkCmd as vm
from types import SimpleNamespace
from sklearn.decomposition import PCA

# tested with python 3.9 and UMAP 0.5.3
pyVersion = sys.version.split(' ')[0]
print(f'UMAP: {umap.__version__}; Python: {pyVersion}')

A = SimpleNamespace(s='spectral', r='random', p='pca')
M = SimpleNamespace(e='euclidean', c='correlation', s='cosine', p='precomputed')
nr = 0
ds = vm.LoadFromVisuMap(M.e)

#ds = PCA(n_components = 100).fit_transform(ds)

#====================================================================================

def ResetTest(cfgIdx):
    global epochs, randomizeOrder, centralizing, stateSeed
    global mapDim, mtr, A0, nn, md, lc, ns, sp, denMap
    randomizeOrder, centralizing, stateSeed = True, False, None
    mapDim, mtr, A0 = 2, M.e, A.s
    epochs, nn = 1000, 1000
    denMap = False
    lc  = 5
    if cfgIdx == 0:
        md, sp, ns = 0.5, 1.0, 15
    elif cfgIdx == 1:
        md, sp, ns = 0.1, 1.5, 30
    else:
        md, sp, ns = 0.23, 1.12, 15

def DoTest():
    global ds, nr
    if centralizing:
        # centralize the training data
        ds = ds - np.mean(ds, axis=0)
    if randomizeOrder:
        perm = np.random.permutation(ds.shape[0])
        ds = ds[perm]
        if mtr == 'precomputed':
            ds[:,:] = ds[:, perm]
        perm = np.arange(ds.shape[0])[np.argsort(perm)]

    t0 = time.time()
    um = umap.UMAP(n_neighbors=nn, min_dist=md, local_connectivity=lc, densmap=denMap,
        n_components=mapDim, metric=mtr, negative_sample_rate=ns, random_state=stateSeed,
        n_epochs=epochs, init=A0, learning_rate=1, verbose=True, spread=sp)
    map = um.fit_transform(ds)
    tm = time.time() - t0
    nr += 1
    title = f'UMAP.{nr}:nn:{nn},md:{md:g},lc:{lc:g},ns:{ns},sp:{sp:g},M:{mtr},A0:{A0},Dm:{ds.shape[1]},T:{tm:.1f}s'
    
    if randomizeOrder: # reverse the random order.
        map = map[perm]
        ds = ds[perm]
        if mtr == 'precomputed':
            ds[:,:] = ds[:, perm]
  
    vm.ShowToVisuMap(map, title)

#====================================================================================
try:
  cmd = vm.DataLinkCmd()
  for cfg in [0, 1, 2]:
    ResetTest(cfg)
    DoTest()
    #cmd.RunScript('vv.GuiManager.TileAllWindows()')

except Exception as e:
  print( 'Exception: ' + str(e) )
  print('Exiting...')
  time.sleep(7)

cmd.Close()

'''
cmd.RunScript('vv.GuiManager.TileAllWindows()')
cmd.RunScript('New.Atlas().Show().CaptureAllOpenViews().Close()')

for A0 in [A.s, A.r, A.p]:
for mtr in [M.e, M.c, M.s]:
for nn in [200, 1000, 2000]:
for lc in [3, 5, 10]:
for ns in [5, 15, 25]:
for md in [0.1, 0.4, 0.8]: 
for sp in np.arange(0.5, 10, 1.0):
'''
