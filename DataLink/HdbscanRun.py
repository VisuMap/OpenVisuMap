# File: HdbscanRun.py
# Run python HDBSCAN on the current map
#
import hdbscan, DataLinkCmd, sys, numpy, time

print(f'Loaded hdbscan.')

cSize, minSamples = 15, 15

cmd = DataLinkCmd.DataLinkCmd()
D = cmd.LoadMapXyz()

print(f'Loaded data: {D.shape}')

C = hdbscan.HDBSCAN(min_cluster_size=cSize, min_samples=minSamples)

print(f'Doing HDBSCAN...')
labels = C.fit_predict(D)

#print(labels[:200])

cmd.UpdateLabels(labels)
print(f'Completed: Data:{D.shape}; MinSize:{cSize}; MinSamples:{minSamples};  Clusters:{C.labels_.max()}')
time.sleep(3.0)
