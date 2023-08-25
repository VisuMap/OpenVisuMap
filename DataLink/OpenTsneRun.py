# File: OpenTsneRun.py
# Script to run OpenTsne on selected data in VisuMap.
#=====================================================

import time, sys, types, numpy, openTSNE
import DataLinkCmd as vm

# tested with python 3.9 and openTSNE 1.0.0
pyVersion = sys.version.split(' ')[0]
print('Python: %s; openTSNE: %s'%(pyVersion, str(openTSNE.__version__)))

print('Fitting data...')
A = types.SimpleNamespace(s='spectral', r='random', p='pca')
M = types.SimpleNamespace(e='euclidean', c='correlation', s='cosine')
mtr, A0, epochs, pp, exa = M.e, A.s, 2000, 150, 4.0
randomizeOrder = True
ds = vm.LoadFromVisuMap(mtr)

def DoTest():
    global ds
    if randomizeOrder:
        perm = numpy.random.permutation(ds.shape[0])
        ds = ds[perm]
        perm = numpy.arange(ds.shape[0])[numpy.argsort(perm)]

    t0 = time.time()
    tsne = openTSNE.TSNE(perplexity=pp, metric=mtr, early_exaggeration=exa,
        n_jobs=6, n_iter=epochs, initialization=A0, verbose=True)
    map = tsne.fit(ds)
    tm = time.time() - t0
    title = f'OpenTsne: Epochs:{epochs}, Mtr:{mtr}, Perplexity:{pp}, Init:{A0}, T:{tm:.1f}'
    
    if randomizeOrder: # reverse the random order.
        map = map[perm]
        ds = ds[perm]
    vm.ShowToVisuMap(map, title)

#=====================================================
mtr, pp, exa = M.e, 1024, 4.0
for k in [0, 1]: 
	DoTest()

#vm.DataLinkCmd().RunScript('New.Atlas().Show().CaptureAllOpenViews().Close()')

'''
for k in [0,1,2]: DoTest()

PX = lambda aL, bL: [(x,y) for x in aL for y in bL]

for mtr, pp in PX([M.e, M.s], [100, 200, 400]): DoTest();

for A0 in [A.s, A.r, A.p]: DoTest()

'''
