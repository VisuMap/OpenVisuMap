import h5py, sys, math, time, scanpy, ModelUtil
import numpy as np

if len(sys.argv) <= 1:
    print('Usage: readh5ad.py <file_name>')
    quit()
fn = sys.argv[1]

f = scanpy.read(fn)
Y = f.X

'''
# sample randomly 10000 rows.
rows = np.random.randint(Y.shape[0], size=10000)
columns = np.random.randint(Y.shape[1], size=5000)
Y = Y[rows, :]
Y = Y[:,columns]
'''

Y = Y.astype(np.float32)
if hasattr(Y, 'todense'):
    Y = Y.todense()

def exp1p(x):
    if ( x > 0 ):
        return np.float32(math.expm1(x))
    else:
        return np.float32(0)
vexp1p = np.vectorize(exp1p)
#Y = vexp1p(Y)

log = ModelUtil.Logger()
dsName = fn.split('\\')[-1].split('.')[0]
print('Saving dataset: ', Y.shape)
log.SaveTable(Y, dsName, description=fn, tmout=1000)
log.Ping(1000)
log.RunScript('vv.Folder.OpenDataset("%s");'%dsName)
log.Ping(1000)