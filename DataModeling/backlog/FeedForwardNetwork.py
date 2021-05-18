#=========================================================================
# FeedforwardNetwork.py
# Invoke the feedforward network traning procedure.
# code is based on  CNTK/Tutorials/CNTK_102_FeedForward.ipynb 
#=========================================================================

import sys, os, time, ModelLog, math
import numpy as np
from cntk.device import cpu, gpu, set_default_device
from cntk import Trainer
from cntk.learner import sgd, learning_rate_schedule, UnitType
from cntk.ops import *
from cntk.utils import ProgressPrinter,TensorBoardProgressWriter
from cntk.initializer import glorot_uniform, he_normal

#=========================================================================

nettype = sys.argv[1]
modelName = sys.argv[2]
epochs = int(sys.argv[3])
logLevel = int(sys.argv[4])
refreshFreq = int(sys.argv[5])
log = ModelLog.Logger(8888)

#=========================================================================
def linear_layer(input_var, output_dim):
    times_param = parameter(shape=(list(input_var.shape)+[output_dim]), init=glorot_uniform())
    bias_param = parameter(shape=(output_dim), init=0)
    return bias_param + times(input_var, times_param)

def dense_layer(input, output_dim, nonlinearity):
    p = linear_layer(input, output_dim)
    return nonlinearity(p)

def create_model(input, output_dim, hidden_dims, nonlinearity):
    r = dense_layer(input, hidden_dims[0], nonlinearity)
    for dim in hidden_dims:
        r = dense_layer(r, dim, nonlinearity)
    return linear_layer(r, output_dim)

def LogReport(ep, error):
    error2 = math.sqrt(error)
    print(ep, ": ", error2)
    log.ReportCost(ep, error2)
    if logLevel >= 1:
        #CreatePrediction()
        log.RefreshMap()
    
# Creates and trains a feedforward classification model

# set_default_device(gpu(0))
# set_default_device(cpu())

features = np.genfromtxt('inData.csv', delimiter='|', dtype=np.float32)
labels = np.genfromtxt('outData.csv', delimiter='|', dtype=np.float32)
N = np.shape(features)[0]
input_dim = np.shape(features)[1]
output_dim = np.shape(labels)[1] 

# Input variables denoting the features and label data
input = input_variable((input_dim), np.float32, name='in_var')
label = input_variable((output_dim), np.float32, name='out_var')

# Instantiate the feedforward classification model

#available activation function: relu, leaky_relu, sigmoid, tanh

if nettype == "cls":
    model = create_model(input, output_dim, [25, 25, 25], sigmoid)
    ce = cross_entropy_with_softmax(model, label)
    lr_per_minibatch=learning_rate_schedule(0.25, UnitType.minibatch)
else:
    if output_dim == 3:
        model = create_model(input, output_dim, [100, 80, 50, 20], sigmoid)
    else:
        model = create_model(input, output_dim, [50, 30, 20], sigmoid)
    lr_per_minibatch=learning_rate_schedule(0.25, UnitType.minibatch)
    ce = squared_error(model, label)

pe = classification_error(model, label)
trainer = Trainer(model, (ce, pe), sgd(model.parameters, lr=lr_per_minibatch))

mini_batch_sz = 25    
error = 0.0;
expFactor = 0.001;

for ep in range(epochs):
    for i in range(0, N, mini_batch_sz):
        j = i + mini_batch_sz
        trainer.train_minibatch({input: features[i:j], label: labels[i:j]})
        error = (1-expFactor)*error + expFactor*trainer.previous_minibatch_loss_average * mini_batch_sz;
    if (ep+1) % refreshFreq == 0:
        LogReport(ep+1, error)

if ep % refreshFreq != 0 :
    LogReport(ep, 1.00)

model.save_model(modelName + ".mod")

# evaluate the model once with the training data.
output = model.eval({"in_var":features})
output = output.reshape(N, output_dim)
np.savetxt("predData.csv", output, delimiter='|', fmt='%.5f')

log.Completed()
time.sleep(10)
    
