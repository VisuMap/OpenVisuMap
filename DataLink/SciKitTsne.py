# File: SciKitTsne.py
# Script to run SciKit tSNE on selected data in VisuMap.
import sys, time, types, numpy, DataLinkCmd
import sklearn
from sklearn.manifold import TSNE

pyVersion = sys.version.split(' ')[0]
print('Python: %s; sklearn: %s'%(pyVersion, str(sklearn.__version__)))
A = types.SimpleNamespace(r='random', p='pca')

epochs = 2000
mapDim = 2
pp = 1000
ds = DataLinkCmd.LoadFromVisuMap('euclidean')
exa = 4.0
initType = A.p
agl = 0.5
lr = 200.0

def DoTest():
    global ds
    perm = numpy.random.permutation(ds.shape[0])
    ds = ds[perm]
    perm = numpy.arange(ds.shape[0])[numpy.argsort(perm)]

    t0 = time.time()
    tsne = TSNE(n_components=mapDim, perplexity=pp, learning_rate=lr, early_exaggeration=exa,
       n_iter=epochs, n_jobs=-1, verbose=2, init=initType, angle=agl)
    map = tsne.fit_transform(ds)
    tm = time.time() - t0

    map = map[perm]
    ds = ds[perm]
    title = 'SciKit-TSNE: Perplexity:%.1f, Angle:%.1f, LR:%.1f, T:%.1f'%(pp, agl, lr, tm)
    DataLinkCmd.ShowToVisuMap(map, title)

print('Fitting data...')
for initType in [A.r, A.p]:
    for pp in [25, 100, 400]:
        DoTest()
