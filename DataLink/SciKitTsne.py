# File: SciKitTsne.py
# Script to run SciKit tSNE on selected data in VisuMap.
import sys, time, types, numpy, DataLinkCmd
import sklearn
from sklearn.manifold import TSNE

# Tested with python 3.9 and sklear 1.3.0
pyVersion = sys.version.split(' ')[0]   
print('Python: %s; sklearn: %s'%(pyVersion, str(sklearn.__version__)))
A = types.SimpleNamespace(r='random', p='pca')
mt = ['barnes_hut', 'exact'][ 0 ]
ds = DataLinkCmd.LoadFromVisuMap('euclidean')

mapDim, pp, epochs = 2, 1000, 2000
exa = 4.0
A0 = A.p
agl = 0.5
lr = 200.0

def DoTest():
    t0 = time.time()
    tsne = TSNE(n_components=mapDim, perplexity=pp, learning_rate=lr, method=mt, early_exaggeration=exa,
       n_iter=epochs, n_jobs=-1, verbose=2, init=A0, angle=agl)
    map = tsne.fit_transform(ds)
    tm = time.time() - t0
    title = 'SciKit-TSNE: Perplexity:%.1f, Angle:%.1f, LR:%.1f, T:%.1f'%(pp, agl, lr, tm)
    DataLinkCmd.ShowToVisuMap(map, title)

print('Fitting data...')

for k in [0, 1]:
#for A0 in [A.r, A.p]:
#for agl in [0.6, 0.3, 0.15]:
    DoTest()

'''
for A0 in [A.r, A.p]:
    for pp in [25, 100, 400]:
'''
