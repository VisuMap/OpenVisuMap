
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

# Notice: set theta to 0 to run the 'exact' mode
mapDim = 2
epochs, pp, theta = 1000, 1000.0, 0.5
ds = vm.LoadFromVisuMap('euclidean')
rows = ds.shape[0]
os.chdir(os.path.dirname(os.path.abspath(__file__)))
inFile, outFile = 'data.dat', 'result.dat'

def SetInputData():
    inData = open(inFile, 'wb')
    inData.write(struct.pack('iiddii', rows, ds.shape[1], theta, pp, mapDim, epochs))
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
    title = f'bh-SNE: pp:{pp:.1f}, theta: {theta:.2f}, T: {(t1-t0):.2f}'
    vm.ShowToVisuMap(map, title)
    os.remove(inFile)
    os.remove(outFile)

#====================================================================================

for k in [0, 1]:
    DoTest()

#====================================================================================

'''
for pp in [500, 1000, 1500]:
for theta in np.arange(0, 0.6, 0.2):
'''