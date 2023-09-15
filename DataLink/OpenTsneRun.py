# File: OpenTsneRun.py
# Script to run OpenTsne on selected data in VisuMap.
#=====================================================

import time, sys, types, numpy, openTSNE
import DataLinkCmd as vm

# tested with python 3.9 and openTSNE 1.0.0
pyVersion = sys.version.split(' ')[0]
print(f'openTSNE: {openTSNE.__version__}; Python: {pyVersion}')

A = types.SimpleNamespace(s='spectral', r='random', p='pca')
M = types.SimpleNamespace(e='euclidean', c='correlation', s='cosine')
epochs, pp, exa = 1000, 1000.0, 4.0
mtr, A0 = M.e, A.s
randomizeOrder = True
sta = 0.1
ds = vm.LoadFromVisuMap(mtr)
gdMd = 'bh'  # 'fft', 'bh' or 'auto'

def DoTest():
    global ds
    if randomizeOrder:
        perm = numpy.random.permutation(ds.shape[0])
        ds = ds[perm]
        perm = numpy.arange(ds.shape[0])[numpy.argsort(perm)]

    t0 = time.time()
    tsne = openTSNE.TSNE(perplexity=pp, metric=mtr, early_exaggeration=exa, theta=sta, negative_gradient_method=gdMd,
        n_jobs=6, n_iter=epochs, initialization=A0, verbose=True)
    map = tsne.fit(ds)
    tm = time.time() - t0
    title = f'OpenTsne: epochs:{epochs}, pp:{pp}, theta:{sta:g}, mtr:{mtr}, A0:{A0}, T:{tm:.1f}'
    
    if randomizeOrder: # reverse the random order.
        map = map[perm]
        ds = ds[perm]
    vm.ShowToVisuMap(map, title)

#=====================================================
try:
  for k in [0,1]:
    DoTest()

except Exception as e:
  print( 'Exception: ' + str(e) )
  print('Exiting...')
  time.sleep(7)

'''
vm.DataLinkCmd().RunScript('New.Atlas().Show().CaptureAllOpenViews().Close()')

for pp in [500, 1500]:
for A0 in [A.s, A.r, A.p]:
for sta in [0.1, 0.2, 0.3]:

PX = lambda aL, bL: [(x,y) for x in aL for y in bL]
for mtr, pp in PX([M.e, M.s], [100, 200, 400]):
'''
