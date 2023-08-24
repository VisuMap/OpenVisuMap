
# File: BH_SneRun.py
# 
# Run BH_SNE on selected data from VisuMap and show the result to VisuMap.
# 
# The executable BH_SNE.exe is created from source: https://github.com/lvdmaaten/bhtsne
#====================================================================================

print('Loading libraries...')
import sys, time, types, struct, os
import numpy as np
import DataLinkCmd as vm

pyVersion = sys.version.split(' ')[0]
print('Python: %s'%pyVersion)

mapDim, pp, theta = 2, 500.0, 0.2
ds = vm.LoadFromVisuMap('euclidean')
rows = ds.shape[0]

os.chdir(os.path.dirname(os.path.abspath(__file__)))
inFile, outFile = 'data.dat', 'result.dat'

def SetInputData():
    inData = open(inFile, 'wb')
    inData.write(struct.pack('iiddi', rows, ds.shape[1], theta, pp, mapDim))
    ds.astype(np.float64).tofile(inData)
    inData.close()

def LoadResult():
    outData = open(outFile, 'rb')
    outData.read(8)   # skip the result shape
    map = struct.unpack(str(rows*mapDim)+'d', outData.read(rows*mapDim*8))
    map = np.array(map).reshape([rows, mapDim])
    idxList = struct.unpack(str(rows)+'i', outData.read(4*rows))
    return map[idxList, :]

def DoTest():
    t0 = time.time()
    SetInputData()
    os.system('BH_SNE.exe')
    map = LoadResult()
    t1 = time.time()
    title = 'bh-SNE: perplexity: %.1f, theta: %.2f, T: %.3f'%(pp, theta, t1-t0)
    vm.ShowToVisuMap(map, title)
    os.remove(inFile)
    os.remove(outFile)

for pp in [200, 500]:
    DoTest()
