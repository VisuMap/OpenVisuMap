# File: OpenTsneRun.py
# Script to run OpenTsne on selected data in VisuMap.
import time, sys, DataLinkCmd
from openTSNE import TSNE
import numpy as np

print("Running Open-Tsne...")
metric = {'e':'euclidean', 'c':'correlation', 's':'cosine'}['e']
repeats, epochs, perplexity = 1, 2000, 50

print('Loading data from VisuMap...')
log = DataLinkCmd.DataLinkCmd()
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
    t0 = time.time()
    tsne = TSNE(perplexity=perplexity, metric=metric, n_jobs=4, n_iter=epochs, random_state=42, verbose=True)
    map = tsne.fit(ds)
    tm = time.time() - t0
    title = f'Open-Tsne: Perplexity: {perplexity}, Time: {tm:.1f}'
    
    log = DataLinkCmd.DataLinkCmd()
    log.ShowMatrix(map, view=12, title=title)
    log.RunScript('pp.NormalizeView()')
