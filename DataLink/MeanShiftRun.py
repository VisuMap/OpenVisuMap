# File: MeanShift.py
# Run python MeanShift on the current map
#
import DataLinkCmd, sys, numpy
from sklearn.cluster import MeanShift

bw = float(sys.argv[1])       if len(sys.argv)>1 else 50.0

cmd = DataLinkCmd.DataLinkCmd()
D = cmd.LoadMapXyz()
print(f'Loaded Data:{D.shape}')

C =  MeanShift(bandwidth=bw).fit(D)
cmd.UpdateLabels(numpy.array(C.labels_))
print(f'Data:{D.shape}; BandWidth:{bw}; Clusters:{C.labels_.max()}')
