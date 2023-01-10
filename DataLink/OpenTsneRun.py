# File: OpenTsneRun.py
# Script to run OpenTsne on selected data in VisuMap.
#=====================================================

import time, sys, types, numpy, DataLinkCmd, openTSNE
pyVersion = sys.version.split(' ')[0]
print('Python: %s; openTSNE: %s'%(pyVersion, str(openTSNE.__version__)))

print('Fitting data...')
initList = types.SimpleNamespace(s='spectral', r='random', p='pca')
mtrList = types.SimpleNamespace(e='euclidean', c='correlation', s='cosine')
metric = mtrList.e
initType = initList.s
epochs = 1000
pp = 200
randomizeOrder = True
exa = 4.0

ds = DataLinkCmd.LoadFromVisuMap(metric)

for k in range(2):
#for initType in [initList.s, initList.r, initList.p]:
    if randomizeOrder:
        perm = numpy.random.permutation(ds.shape[0])
        ds = ds[perm]
        perm = numpy.arange(ds.shape[0])[numpy.argsort(perm)]

    t0 = time.time()
    tsne = openTSNE.TSNE(perplexity=pp, metric=metric, early_exaggeration=exa,
        n_jobs=6, n_iter=epochs, initialization=initType, verbose=True)
    map = tsne.fit(ds)
    tm = time.time() - t0
    title = f'OpenTsne: Epochs:{epochs}, Mtr:{metric}, Perplexity:{pp}, Init:{initType}, T:{tm:.1f}'
    
    if randomizeOrder: # reverse the random order.
        map = map[perm]
        ds = ds[perm]

    DataLinkCmd.ShowToVisuMap(map, title)
