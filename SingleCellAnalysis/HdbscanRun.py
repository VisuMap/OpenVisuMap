# File: HdbscanRun.py
# Script to run HDBSCAN on selected data in VisuMap.
print('Loading libraries...')
import time, sys, ModelUtil
import numpy as np
import hdbscan

mtr = {'e':'euclidean', 'c':'correlation', 's':'cosine', 'p':'precomputed', 'x':'xyz'}['p']
log = ModelUtil.Logger()

minClusterSize = 15
minSamples = 5
if len(sys.argv)>=2:
    minClusterSize = int(sys.argv[1])
if len(sys.argv)>=3:
    minSamples = int(sys.argv[2])

print('Loading data from VisuMap...')

if mtr == 'precomputed':
    ds = log.LoadDistances(tmout=600)
elif mtr == 'xyz':
    ds = log.LoadMapXyz()
    mtr = 'euclidean'
else:
    ds = log.LoadTable(dsName='+')
    if (ds is None) or (ds.shape[0]==0) or (ds.shape[1]==0):
        ds = log.LoadTable('@', tmout=180).astype(np.double)
        if (ds.shape[0]==0) or (ds.shape[1]==0):
            print('No data has been selected')
            time.sleep(4.0)
            quit()

print('Data loaded: ', ds.shape)

t0 = time.time()
clusterer = hdbscan.HDBSCAN(min_cluster_size=minClusterSize, min_samples=minSamples, metric=mtr)
labels = clusterer.fit_predict(ds) + 1
log.UpdateLabels(labels)
tm = time.time() - t0

unique, frequency = np.unique(labels,  return_counts = True) 
print("HDBSCAN: MinClusterSize: %d; MinSamples: %d; Metric: %s; Time: %.1fsec\nLabel \tClusterSize"%(minClusterSize, minSamples, mtr, tm))
for i in range(len(unique)):
    print("%d\t%d"%(unique[i], frequency[i]))
