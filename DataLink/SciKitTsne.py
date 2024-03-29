# File: SciKitTsne.py
# Script to run SciKit tSNE on selected data in VisuMap.
import sys, time, types, numpy
import DataLinkCmd as vm
import sklearn
from sklearn.manifold import TSNE
from sklearn.decomposition import PCA

# Tested with python 3.9 and sklear 1.3.0
pyVersion = sys.version.split(' ')[0]   
print(f'sklearn: {sklearn.__version__}; Python: {pyVersion}')
A = types.SimpleNamespace(r='random', p='pca')
M = types.SimpleNamespace(e='euclidean', c='correlation', s='cosine')
mt = ['barnes_hut', 'exact'][ 0 ]
ds = vm.LoadFromVisuMap('euclidean')

def PCAandTsne(ds, tsneEpochs=1000, pcaDim=100, mapDim=2):
  cmd = vm.DataLinkCmd()
  print('Doing PCA reduction...')
  dd = PCA(n_components = pcaDim).fit_transform(ds)
  print(f't-SNE Embedding {dd.shape[1]}=>{mapDim}...')
  map = cmd.DoTsne(dd, epochs=tsneEpochs, perplexityRatio=0.1, mapDimension=mapDim, exaInitial=8.0, exaFinal=1.0)
  vw = 12 if mapDim == 2 else 13    
  cmd.ShowMatrix(map, view=vw)
  return map

#PCAandTsne(ds, 500, 50, 2)
#quit()

epochs, pp, exa = 1000, 1000, 4.0
mapDim, mtr, A0 = 2, M.e, A.p
agl, lr = 0.1, 200.0

def DoTest():
    t0 = time.time()
    tsne = TSNE(n_components=mapDim, perplexity=pp, metric=mtr, 
       learning_rate=lr, method=mt, early_exaggeration=exa,
       n_iter=epochs, n_jobs=-1, verbose=2, init=A0, angle=agl)
    map = tsne.fit_transform(ds)
    tm = time.time() - t0
    title = f'sk-TSNE: pp:{pp:g}, exa:{exa:g}, angle:{agl:g}, init:{A0}, mtr:{mtr}, lr:{lr:g}, T:{tm:.1f}'
    vm.ShowToVisuMap(map, title)

#==========================================================================

try:
  for k in [0, 1]:
    DoTest()

except Exception as e:
  print( 'Exception: ' + str(e) )
  print('Exiting...')
  time.sleep(7)

#==========================================================================

'''
for k in range(2):
for A0 in [A.r, A.p]:
for agl in np.arange(0.1, 0.9, 0.2):
for A0 in [A.r, A.p]:
for pp in [25, 100, 400]:
'''
