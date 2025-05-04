# File: PaCMapRun.py
# Script to run PaCMap on selected data in VisuMap.
#=====================================================

import time, sys, types, numpy, openTSNE
import DataLinkCmd as vm
import pacmap

pyVersion = sys.version.split(' ')[0]
print(f'PaCMAP: {pacmap.__version__}; Python: {pyVersion}')
ds = vm.LoadFromVisuMap('euclidean')

def DoTest(cmps=2, nbs=10, mnr=0.5, fpr=2.0):
    global ds
    t0 = time.time()
    embedding = pacmap.PaCMAP(n_components=cmps, n_neighbors=nbs, MN_ratio=mnr, FP_ratio=fpr)
    map = embedding.fit_transform(ds, init='pca')
    tm = time.time() - t0
    title = f'PaCMAP: Components:{cmps}, Neighbors:{nbs}, MN_Ration:{mnr}, FP_Ratio:{fpr}, T:{tm:.1f}'
    vm.ShowToVisuMap(map, title)

'''
for mnr in [0.1, 0.5, 0.9]:
    DoTest(mnr=mnr)

for fpr in [0.5, 2.0, 5.0]:
    DoTest(fpr=fpr)
'''

for nbs in [5, 10, 50]:
    DoTest(nbs=nbs)
