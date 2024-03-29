# File: UMapRun.py
# Script to run UMAP on selected data in VisuMap.
print("Running UMAP...")
print('Loading libraries...')
import umap, time, sys, ModelUtil
import numpy as np

mtr = {'e':'euclidean', 'c':'correlation', 's':'cosine', 'p':'precomputed'}['e']
initType = ['spectral', 'random'][0]
repeats, epochs = 2, 2000
mapDim, nn, md, lc, ns = 2, 2000, 0.25, 5.0, 25
randomizeOrder = True
log = ModelUtil.Logger()

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

print('Fitting data...')

for k in range(repeats):
    if randomizeOrder:
        perm = np.random.permutation(ds.shape[0])
        ds = ds[perm]
        if mtr == 'precomputed':
            ds[:,:] = ds[:, perm]
        perm = np.arange(ds.shape[0])[np.argsort(perm)]

    t0 = time.time()

    um = umap.UMAP(n_neighbors=nn, min_dist=md, local_connectivity=lc, 
        n_components=mapDim, metric=mtr, negative_sample_rate=ns,
        n_epochs=epochs, init=initType, learning_rate=1, verbose=True)
    map = um.fit_transform(ds)
    tm = time.time() - t0
    title = 'UMAP: Neighbors: %d, MinDist: %g, LocCnt: %g, N.Smpl: %d, Metric: %s, Time: %.1f'%(nn, md, lc, ns, mtr, tm)
    
    if randomizeOrder: # reverse the random order.
        map = map[perm]
        ds = ds[perm]
        if mtr == 'precomputed':
            ds[:,:] = ds[:, perm]

    log = ModelUtil.Logger()
    if mapDim == 2:
        if k > 0:
                log.RunScript('vv.Dataset.AddMap(null)')
        log.ShowMatrix(map, view=12, title=title)
        log.RunScript('pp.NormalizeView(); pp.ClickContextMenu("Utilities/Capture Map"); pp.Close();')
	#log.RunScript('pp.NormalizeView()')
    elif mapDim == 3:
        log.ShowMatrix(map, view=13, title=title)
        log.RunScript('pp.DoPcaCentralize()')
    else:
        log.ShowMatrix(map, view=2, title=title)
    
