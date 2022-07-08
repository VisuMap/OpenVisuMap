# File: AffinityPropagation.py
# Run python AffinityPropagation on the current map
#
import DataLinkCmd, sys, numpy
from sklearn.cluster import AffinityPropagation

pref = float(sys.argv[1])       if len(sys.argv)>1 else 0.0

cmd = DataLinkCmd.DataLinkCmd()
D = cmd.LoadMapXyz()
print(f'Loaded Data:{D.shape}')

C =  AffinityPropagation(preference=pref).fit(D)
cmd.UpdateLabels(numpy.array(C.labels_))
print(f'Data:{D.shape}; Preference:{pref}; Clusters:{C.labels_.max()}')
