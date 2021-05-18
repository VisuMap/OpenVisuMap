#=========================================================================
# Common prefix code for model scritps.
#=========================================================================

import sys, os, time
from ModelUtil import *

os.environ['TF_CPP_MIN_LOG_LEVEL']='3'
import tensorflow as tf
import numpy as np

print("TensorFlow version:", tf.__version__, " Model script ", sys.argv[0], "...")

argcnt = len(sys.argv)
modelName = sys.argv[1]        if argcnt>1 else '<NotSave>'
epochs = int(sys.argv[2])      if argcnt>2 else 100
logLevel = int(sys.argv[3])    if argcnt>3 else 1
refreshFreq = int(sys.argv[4]) if argcnt>4 else 20
job = int(sys.argv[5])         if argcnt>5 else 0
jobArgument = sys.argv[6]      if argcnt>6 else ''

print('Model: %s, Epochs: %d, Job: %d'%(modelName, epochs, job))

def TrainAll(md, ds, epCall=None):
    md.SetAdamOptimizer(epochs, ds.N)
    md.Train(ds, epochs, logLevel, refreshFreq, epCall)
    md.Save(modelName)

def FinalReport(md, netCfg, ds=None):
    target = ds.mdOutput if (ds!=None) else ''
    if logLevel >= 2:
        msg = '%d: Ep:%d, %s, LR:%.5g, E:%.4f, T:%.1f, '%(
            job, epochs, netCfg, md.r0, md.lastError, md.trainingTime)
        if (logLevel >= 3) and (target!='') :
            errorL1, mismatches = md.log.GetPredInfo()
            if 'Clr' in target: msg += 'Miss:%d, '%mismatches
            if 'Shp' in target: msg += 'L1:%.1f'%errorL1
        md.log.LogMsg(msg)
    time.sleep(3)
