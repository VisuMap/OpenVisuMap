# File: LargeVis.py
# Script to run LargeVis on selected data in VisuMap.
# Notice: LargeVis.exe has been modified to accept and produce csv files.
#
import sys, os, time, ModelUtil
import numpy as np

log = ModelUtil.Logger()

print('Loading data from VisuMap...')
ds = log.LoadTable(dsName='+')
if (ds is None) or (ds.shape[0]==0) or (ds.shape[1]==0):
    ds = log.LoadTable('@')
    if (ds.shape[0]==0) or (ds.shape[1]==0):
        print('No data has been selected')
        time.sleep(4.0)
        quit()
print("Loaded table: ", ds.shape)

mapDim = 2
perp = 1000
prog = 'C:/work/VisuMap/extern/LargeVis/LargeVis.exe'
infile = 'C:/temp/LargeVis_in.csv'
outfile = 'C:/temp/LargeVis_out.csv'
np.savetxt(infile, ds, delimiter=' ')

t0 = time.time()
exArgs = '-threads 2'
os.system('cmd /c %s -input %s -output %s -outdim %d -perp %d %s'%(prog, infile, outfile, mapDim, perp, exArgs))
tm = time.time() - t0

map = np.loadtxt(outfile)
title = 'LargeVis: Perplexity: %d, Time: %.1f'%(perp, tm)
log = ModelUtil.Logger()
if mapDim >= 3:
    log.ShowMatrix(map, view=2, title=title)
else:
    log.ShowMatrix(map, view=12, title=title)
    log.RunScript('pp.NormalizeView()')

os.remove(infile)
os.remove(outfile)