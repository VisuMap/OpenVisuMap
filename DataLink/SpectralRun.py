# File: SpectralRun.py
# Script to run spectral embedding on selected data in VisuMap.
import sys, time, DataLinkCmd
import sklearn
from sklearn.manifold import SpectralEmbedding

pyVersion = sys.version.split(' ')[0]
print('Python: %s; sklearn: %s'%(pyVersion, str(sklearn.__version__)))
ds = DataLinkCmd.LoadFromVisuMap()

print('Fitting data...')
t0 = time.time()
spr = SpectralEmbedding(n_components=2, n_jobs=-1)
map = spr.fit_transform(ds)
tm = time.time() - t0
DataLinkCmd.ShowToVisuMap(map, 'Spectral-Embedding')
