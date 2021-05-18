#=========================================================================
# Eval.py
#=========================================================================
import sys,os
os.environ['TF_CPP_MIN_LOG_LEVEL']='3'
import tensorflow as tf
import numpy as np
from ModelUtil import *

modelName = sys.argv[1]
md = ModelBuilder()
md.LoadModel(modelName)
test_data = np.genfromtxt('testData.csv', delimiter='|', dtype=np.float32)
output = md.Eval(test_data)
np.savetxt("predData.csv", output, delimiter='|', fmt='%.6f')
