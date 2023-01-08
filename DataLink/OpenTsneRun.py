# File: OpenTsneRun.py
# Script to run OpenTsne on selected data in VisuMap.
#=====================================================
import time, sys, DataLinkCmd
import openTSNE as ot
import numpy as np

pyVersion = sys.version.split(' ')[0]
print('Python: %s; openTSNE: %s'%(pyVersion, str(ot.__version__)))

print("Running Open-Tsne...")
mtrList = {'e':'euclidean', 'c':'correlation', 's':'cosine'}
metric = mtrList['s']
epochs = 1000
pp = 50

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

ds = np.nan_to_num(ds)
print("Loaded table: ", ds.shape)

print('Fitting data...')
for k in range(2):
#for pp in [50, 100, 200]
    t0 = time.time()
    tsne = ot.TSNE(perplexity=pp, metric=metric, n_jobs=4, n_iter=epochs, random_state=42, verbose=True)
    map = tsne.fit(ds)
    tm = time.time() - t0
    title = f'Open-Tsne: Epochs:{epochs}, Metric:{metric}, Perplexity:{pp}, Time:{tm:.1f}'
    
    with DataLinkCmd.DataLinkCmd() as cmd:
        cmd.ShowMatrix(map, view=12, title=title)
        cmd.RunScript('pp.NormalizeView()')
