# File: UMapRun.py
# Script to run UMAP on selected data in VisuMap.
print("Running UMAP...")
print('Loading libraries...')
import umap, time, sys, DataLinkCmd
import numpy as np

mtr = {'e':'euclidean', 'c':'correlation', 's':'cosine', 'p':'precomputed'}['s']
initType = ['spectral', 'random'][1]
epochs = 2000
mapDim = 2
nn = 2500
md = 0.25
lc = 20.0
ns = 25
sp = 25.0
randomizeOrder = True

log = DataLinkCmd.DataLinkCmd()

print('Loading data from VisuMap...')
if mtr == 'precomputed':
    ds = log.LoadDistances(tmout=600)
else:
    ds = log.LoadTable(dsName='+')
    if (ds is None) or (ds.shape[0]==0) or (ds.shape[1]==0):
        ds = log.LoadTable('@', tmout=180)
        if (ds.shape[0]==0) or (ds.shape[1]==0):
            print('No data has been selected')
            time.sleep(4.0)
            quit()

ds = np.nan_to_num(ds)
print("Loaded table: ", ds.shape)

# centralize the training data
# if mtr == 'cosine': ds = ds - np.mean(ds, axis=0)

parList = [0, 1]
#parList = [10.0, 20.0, 30.0, 40.0, 50.0]

print('Fitting data...')
for par in parList:
    #ns = par
    if randomizeOrder:
        perm = np.random.permutation(ds.shape[0])
        ds = ds[perm]
        if mtr == 'precomputed':
            ds[:,:] = ds[:, perm]
        perm = np.arange(ds.shape[0])[np.argsort(perm)]

    t0 = time.time()
    um = umap.UMAP(n_neighbors=nn, min_dist=md, local_connectivity=lc, 
        n_components=mapDim, metric=mtr, negative_sample_rate=ns,
        n_epochs=epochs, init=initType, learning_rate=1, verbose=True, spread=sp)
    map = um.fit_transform(ds)
    tm = time.time() - t0
    title = 'UMAP: Neighbors: %d, MinDist: %g, LocCnt: %g, N.Smpl: %d, Spread: %.1f, Metric: %s, Time: %.1f'%(nn, md, lc, ns, sp, mtr, tm)
    
    if randomizeOrder: # reverse the random order.
        map = map[perm]
        ds = ds[perm]
        if mtr == 'precomputed':
            ds[:,:] = ds[:, perm]

    log = DataLinkCmd.DataLinkCmd()
    if mapDim == 1:
        log.ShowMatrix(map, view=14, title=title)
        log.RunScript('pp.NormalizeView();pp.SortItems(true);')
    elif mapDim == 2:
        log.ShowMatrix(map, view=12, title=title)
        log.RunScript('pp.NormalizeView()')
    elif mapDim == 3:
        log.ShowMatrix(map, view=13, title=title)
        log.RunScript('pp.DoPcaCentralize()')
