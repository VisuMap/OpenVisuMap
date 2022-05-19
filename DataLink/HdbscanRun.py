# File: HdbscanRun.py
# Run python HDBSCAN on the current map
#
import hdbscan, DataLinkCmd, sys, numpy

cSize = int(sys.argv[1])       if len(sys.argv)>1 else 5
minSamples = int(sys.argv[2])  if len(sys.argv)>2 else 5

cmd = DataLinkCmd.DataLinkCmd()
D = cmd.LoadMapXyz()
C = hdbscan.HDBSCAN(min_cluster_size=cSize, min_samples=minSamples)
C.fit(D)

cmd.UpdateLabels(numpy.array(C.labels_))
print(f'Data:{D.shape}; MinSize:{cSize}; MinSamples:{minSamples};  Clusters:{C.labels_.max()}')
