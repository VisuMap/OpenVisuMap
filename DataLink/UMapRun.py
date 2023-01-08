# File: UMapRun.py
# 
# Run UMAP on selected data from VisuMap and show the result to VisuMap.
#====================================================================================
print('Loading libraries...')
import sys
#sys.path.insert(0, 'C:/temp/umap-master/umap-master')
import time, umap, DataLinkCmd
import numpy as np

pyVersion = sys.version.split(' ')[0]
print('Python: %s; UMAP: %s'%(pyVersion, str(umap.__version__)))

mtrList = {'e':'euclidean', 'c':'correlation', 's':'cosine', 'p':'precomputed'}
initList = {'s':'spectral', 'r':'random', 'p':'pca'}

mtr = mtrList['e']
initType = initList['p']
epochs = 2500
mapDim = 2
nn = 2000
md = 0.99
lc = 20
ns = 25
sp = 20
randomizeOrder = True
stateSeed = None

print('Loading data from VisuMap...')

with DataLinkCmd.DataLinkCmd() as log:
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

print('Fitting data...')
for k in range(2):
#for md in [3.0, 4.0, 5.0, 10.0]:
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

    cmd = DataLinkCmd.DataLinkCmd()
    if mapDim == 1:
        cmd.ShowMatrix(map, view=14, title=title)
        cmd.RunScript('pp.NormalizeView();pp.SortItems(true);')
    elif mapDim == 2:
        cmd.ShowMatrix(map, view=12, title=title)
        cmd.RunScript('pp.NormalizeView()')
    elif mapDim == 3:
        cmd.ShowMatrix(map, view=13, title=title)
        cmd.RunScript('pp.DoPcaCentralize()')
    cmd.Close()
