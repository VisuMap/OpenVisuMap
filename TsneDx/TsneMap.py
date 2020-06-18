# Copyright (C) VisuMap Technologies Inc. 2020
#
# TsneMap.py: Sample script to access TsneDx from python 
#
# Before runing the module 'pythonnet' need to be pre-installed; the path
# to TsneDx directory need to be adjusted.
#
import sys, clr, time, System
import numpy as np
sys.path.append('C:\\work\\OpenVisuMap\\TsneDx\\bin\\Release')
td = clr.AddReference('TsneDx')
import TsneDx

t0 = time.time()
def Msg(msg):
    global t0
    t = time.time()
    print(msg, '  Time:%.2fs'%(t-t0))
    t0 = t

tsne = TsneDx.TsneMap(MaxEpochs=1000, OutDim=2)

Msg('Test started')
X = np.genfromtxt('SP500.csv')
np.save('tmp0123.npy', X)
Msg('Started fitting %dx%d table...'%(X.shape[0], X.shape[1]))
Y = tsne.FitNumpy('tmp0123.npy')
Msg('Completed learning')
Y = np.fromiter(Y, float).reshape(X.shape[0], -1)
np.savetxt('SP500_map.csv', Y)
Msg('Map saved')



