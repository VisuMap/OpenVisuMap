from theano import *
import theano as T
import theano.tensor as TT
import numpy as np
import sys, ModelLog

modelName = sys.argv[1]
epochs = int(sys.argv[2])
logLevel = int(sys.argv[3])
refreshFreq = int(sys.argv[4])
log = ModelLog.Logger(8888)
batch_sz = 25    

X = np.genfromtxt('inData.csv', delimiter='|', dtype=np.float32)
Y = np.genfromtxt('outData.csv', delimiter='|', dtype=np.float32)
x_dim = np.shape(X)[1]
y_dim = np.shape(Y)[1] 
N = np.shape(X)[0]

#=========================================================================

var_X = T.shared(value=X, name='X')
var_Y = T.shared(value=Y, name='Y')
rng = np.random.RandomState(1234)
LEARNING_RATE = 0.01

if y_dim == 3:
    dims = [x_dim, 100, 80, 50, 20, y_dim]
else:
    dims = [x_dim, 50, 30, 20, y_dim]

len_dims = len(dims)
output = var_X
for k in range(1, len_dims):
    W = T.shared(value=np.asarray(rng.uniform(low=-1.0, high=1.0, size=(dims[k-1], dims[k])), dtype=T.config.floatX), name='W', borrow=True)
    b = T.shared(value=np.zeros(dims[k], T.config.floatX), name='b', borrow=True )
    output = TT.nnet.sigmoid(TT.dot(output, W) + b)

cost = TT.sum((var_Y - output) ** 2)

#train_fct = T.function(inputs=[var_X, var_Y], output=cost, updates=sgd, allow_input_downcast=True)

 
