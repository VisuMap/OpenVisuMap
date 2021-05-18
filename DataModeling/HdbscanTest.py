
import hdbscan
import ModelUtil as mu
import numpy as np

vm = mu.Logger()
pos = vm.LoadTable('$')
minCluster = 10
minSamples = 10
print('HDBSCAN Clustering!   Loaded table: ', pos.shape)
cluster = hdbscan.HDBSCAN(min_cluster_size=minCluster, min_samples=minSamples)
labels = cluster.fit_predict(pos)
vm.UpdateLabels(labels)

noises = np.count_nonzero(labels==-1)
clusters = len(np.unique(labels))
vm.RunScript("vv.Title='Clusters: %d; Noise: %d, minSz/Sp: %d/%d'"%(clusters, noises, minCluster, minSamples))
