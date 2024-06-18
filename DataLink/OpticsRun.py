# File: OpticsRun.py
#
import DataLinkCmd, sys, numpy, time
from sklearn.cluster import OPTICS

print(f'Loaded OPTICS.')

cmd = DataLinkCmd.DataLinkCmd()
D = cmd.LoadMapXyz()

print(f'Loaded data: {D.shape}')

minSamples = 20
if len(sys.argv) > 1:
    minSamples = int( sys.argv[1] )
print(f'Running OPTICS with min-samples:{minSamples}...')
C = OPTICS(min_samples=minSamples, n_jobs=6).fit(D)
labels = C.labels_

cmd.UpdateLabels(labels)
print(f'Completed: Data:{D.shape}; Clusters:{C.labels_.max()}')
