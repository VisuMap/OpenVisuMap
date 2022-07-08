# File: SciKitTsne.py
# Script to run SciKit tSNE on selected data in VisuMap.
import time, DataLinkCmd
import numpy as np
from sklearn.manifold import TSNE

print("Running sklearn.manifold.TSNE...")
repeats, mapDim, pPerplexity, epochs = 1, 2, 100, 5000

log = DataLinkCmd.DataLinkCmd()
print('Loading data from VisuMap...')
ds = log.LoadTable(dsName='+')
if (ds.shape[0]==0) or (ds.shape[1]==0):
    ds = log.LoadTable('@')
    if (ds.shape[0]==0) or (ds.shape[1]==0):
        print('No data has been selected')
        time.sleep(4.0)
        quit()
print("Loaded table: ", ds.shape)

print('Fitting data...')
for rp in range(repeats):
    t0 = time.time()
    tsne = TSNE(n_components=mapDim, perplexity=pPerplexity, 
        learning_rate=200.0, n_iter=epochs, angle=0.5, n_jobs=-1, verbose=2, init='random')
    map = tsne.fit_transform(ds)
    tm = time.time() - t0
    title = 'SciKit-TSNE: Dimension %d, Perplexity=%.1f, Time: %.1f'%(mapDim, pPerplexity, tm)
    if mapDim >= 3:
        log.ShowMatrix(map, view=2, title=title)
    else:
        log.ShowMatrix(map, view=12, title=title)
        log.RunScript('pp.NormalizeView()')
