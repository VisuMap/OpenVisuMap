import time, sys, ModelUtil
import numpy as np
sys.path.append('/work/VisuMapPlugin/SingleCellAnalysis/rna-seq-tsne/FIt-SNE-master')
from fast_tsne import fast_tsne

print("Running FIt-SNE...")
log = ModelUtil.Logger()
X = log.LoadTable(dsName='+')
if (X is None) or (X.shape[0]==0) or (X.shape[1]==0):
    X = log.LoadTable('@')
    if (X.shape[0]==0) or (X.shape[1]==0):
        print('No data has been selected')
        time.sleep(4.0)
        quit()

print("Loaded table: ", X.shape)

repeats, perplexity, epochs = 1, 500, 500

print('Fitting data...')
for rp in range(repeats):
    t0 = time.time()
    Y = fast_tsne(X, perplexity=perplexity, max_iter=epochs, stop_early_exag_iter=int(epochs/4))
    tm = time.time() - t0
    log.ShowMatrix(Y, view=12, title='FItSNE: Time: %.1f, Perplexity: %d, Iterations: %d'%(tm, perplexity, epochs))
    log.RunScript('pp.NormalizeView()')

