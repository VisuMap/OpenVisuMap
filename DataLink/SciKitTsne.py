# File: SciKitTsne.py
# Script to run SciKit tSNE on selected data in VisuMap.
import sys, time, numpy, DataLinkCmd
from sklearn.manifold import TSNE

print("Running sklearn.manifold.TSNE...")
epochs = 1000
mapDim = 2
pPerplexity = 100
ds = DataLinkCmd.LoadFromVisuMap('euclidean')

print('Fitting data...')
for k in range(2):
    t0 = time.time()
    tsne = TSNE(n_components=mapDim, perplexity=pPerplexity, 
        learning_rate=200.0, n_iter=epochs, angle=0.5, n_jobs=-1, verbose=2, init='pca')
    map = tsne.fit_transform(ds)
    tm = time.time() - t0
    title = 'SciKit-TSNE: Dimension %d, Perplexity=%.1f, Time: %.1f'%(mapDim, pPerplexity, tm)
    DataLinkCmd.ShowToVisuMap(map, title)
