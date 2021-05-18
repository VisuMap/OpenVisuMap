print('Loading libraries...')
import umap, time, ModelUtil
import numpy as np

repeats, epochs = 6, 2000
mapDim, nn, md, lc, ns = 3, 1000, 0.25, 5.0, 10

log = ModelUtil.Logger()
print('Loading data from VisuMap...')
ds = log.LoadTable('@', tmout=180)
print("Loaded table: ", ds.shape)
N = ds.shape[0]

for k in range(repeats):
    perm = np.random.permutation(N)
    ds = ds[perm]

    um = umap.UMAP(n_neighbors=nn, min_dist=md, local_connectivity=lc, n_components=mapDim,
        negative_sample_rate=ns, n_epochs=epochs, init='random', learning_rate=1, verbose=True)
    map = um.fit_transform(ds)

    perm = np.arange(N)[np.argsort(perm)]
    map = map[perm]
    ds = map    

    title = '%d, UMAP: NBs:%d, MinDist:%g, LcCnt:%g, NegSpl:%d'%(k, nn, md, lc, ns)    
    log.ShowMatrix(map, view=10+mapDim, title=title)
    log.RunScript('pp.NormalizeView()')
