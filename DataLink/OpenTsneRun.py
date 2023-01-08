# File: OpenTsneRun.py
# Script to run OpenTsne on selected data in VisuMap.
#=====================================================

import time, sys, numpy, DataLinkCmd, openTSNE
pyVersion = sys.version.split(' ')[0]
print('Python: %s; openTSNE: %s'%(pyVersion, str(openTSNE.__version__)))

print('Loading data from VisuMap...')
log = DataLinkCmd.DataLinkCmd()
ds = log.LoadTable(dsName='+')
if (ds is None) or (ds.shape[0]==0) or (ds.shape[1]==0):
    ds = log.LoadTable('@', tmout=180)
    if (ds.shape[0]==0) or (ds.shape[1]==0):
        print('No data has been selected')
        time.sleep(4.0)
        quit()
log.Close()
ds = numpy.nan_to_num(ds)
print("Loaded table: ", ds.shape)

print('Fitting data...')
mtrList = {'e':'euclidean', 'c':'correlation', 's':'cosine'}
initList = {'s':'spectral', 'r':'random', 'p':'pca'}
metric = mtrList['e']
initType = initList['s']
epochs = 1000
pp = 400
randomizeOrder = True

for k in range(4):
#for pp in [50, 100, 200, 400]:
    if randomizeOrder:
        perm = numpy.random.permutation(ds.shape[0])
        ds = ds[perm]
        perm = numpy.arange(ds.shape[0])[numpy.argsort(perm)]

    t0 = time.time()
    tsne = openTSNE.TSNE(perplexity=pp, metric=metric, n_jobs=4, n_iter=epochs, 
	initialization=initType, random_state=42, verbose=True)
    map = tsne.fit(ds)
    tm = time.time() - t0
    title = f'OpenTsne: Epochs:{epochs}, Mtr:{metric}, Perplexity:{pp}, Init:{initType}, T:{tm:.1f}'
    
    if randomizeOrder: # reverse the random order.
        map = map[perm]
        ds = ds[perm]

    with DataLinkCmd.DataLinkCmd() as cmd:
        cmd.ShowMatrix(map, view=12, title=title)
        cmd.RunScript('pp.NormalizeView()')
