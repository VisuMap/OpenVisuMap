#=========================================================================
# FFRegression.py
#=========================================================================

import sys, os, math, time, shutil, ModelLog
os.environ['TF_CPP_MIN_LOG_LEVEL']='3'
import tensorflow as tf
import numpy as np
from Util import MakePairs

#=========================================================================

modelName = sys.argv[1]
epochs = int(sys.argv[2])
logLevel = int(sys.argv[3])
refreshFreq = int(sys.argv[4])

keep_prob_p=0.95
batch_sz = 25    
learning_rate = 0.001

X = np.genfromtxt('inData.csv', delimiter='|', dtype=np.float32)
Y = np.genfromtxt('outData.csv', delimiter='|', dtype=np.float32)
x_dim = np.shape(X)[1]
y_dim = np.shape(Y)[1] 
N = np.shape(X)[0]

#=========================================================================

var_x = tf.placeholder( tf.float32, shape=[None, 2*x_dim] )
var_y = tf.placeholder( tf.float32, shape=[None, 2*y_dim] ) 
keep_prob = tf.placeholder( tf.float32 )

if y_dim == 3:
    dims = [2*x_dim, 100, 80, 50, 20, 2*y_dim]
else:
    dims = [2*x_dim, 50, 30, 20, 2*y_dim]

W, b, H = [], [], [var_x]
len_dims = len(dims)
for k in range(1, len_dims):
    W.append(tf.Variable( tf.random_normal([dims[k-1], dims[k]], 0.0, 0.5) ))
    b.append(tf.Variable( tf.zeros([dims[k]]) ))
    h = tf.nn.sigmoid(tf.matmul(H[k-1], W[k-1]) + b[k-1])
    if k == 1 and keep_prob_p != 1.0:   # add dropout to the first hidden layer
        h = tf.nn.dropout(h, keep_prob)   
    H.append(h)
y = H[-1]

tf.add_to_collection('vars', var_x)
tf.add_to_collection('vars', y)
tf.add_to_collection('vars', keep_prob)
#for lay in range(1, len_dims): tf.add_to_collection('vars', H[lay])

cost = tf.reduce_sum(tf.square(var_y - y))
train_step = tf.train.AdamOptimizer(learning_rate).minimize(cost)
sess = tf.Session()
sess.run(tf.global_variables_initializer())

log = ModelLog.Logger(sess, 8888)

#=========================================================================

def CreatePrediction(input):
    output = sess.run(y, feed_dict={var_x: input, keep_prob:1.0})
    np.savetxt("predData.csv", output, delimiter='|', fmt='%.5f')

def DiffL2(input, output):
    _, err = sess.run([y, cost], feed_dict={var_x:input, var_y:output, keep_prob:1.0})
    err = math.sqrt(err)
    return err

from copy import deepcopy

def DeepValidate():
    np.random.seed(123)
    diffL2 = 0
    K = 10
    stdX = np.std(X, axis=0)
    for k in range(K):
        XX = deepcopy(X)
        for i in range(N):
            for j in range(x_dim):
                XX[i][j] += np.random.uniform(-0.5, +0.5) * stdX[j]
        diffL2 += DiffL2(XX, Y)
    msg = "Recovery Loss: " + '{0:.5f}'.format(DiffL2(X,Y)) + "   Av. Diff-L2: " + '{0:.5f}'.format(diffL2/K) +  "  NN: " + str(dims)
    print(msg)
    log.ReportMsg(msg)


def LogReport(ep, error):
    error2 = math.sqrt(error)
    log.ReportCost(ep, error2)
    if logLevel >= 2:
        print(ep, ": ", error2)
        CreatePrediction(X)
        log.RefreshMap()

#=========================================================================

logTable = []
error = 0.0;
eP = float(batch_sz)/N;
eQ = 1 - eP;

X = MakePairs(X, True)
Y = MakePairs(Y, True)

rOrder = np.random.rand(N).argsort()
rX = np.take(X, rOrder, axis=0)
rY = np.take(Y, rOrder, axis=0)

for ep in range(1, epochs+1):
    for i in range(0, N, batch_sz):
        _, err = sess.run([train_step, cost], feed_dict={
            var_x:rX[i:i+batch_sz], 
            var_y:rY[i:i+batch_sz], 
            keep_prob:keep_prob_p})
        error = eQ*error + eP*err;
    if ep % refreshFreq == 0: LogReport(ep, error)
    #logTable.append(log.VarStates([b[0], b[1]]))
if ep % refreshFreq != 0: LogReport(ep, error)

if len(logTable) > 0: log.ShowMatrix(np.array(logTable).transpose(), 'log.bin')
#log.ShowMatrix(sess.run(H[1], feed_dict={var_x:X, keep_prob:1.0}), 'log1.bin')

#=========================================================================

saver = tf.train.Saver()
saver.save(sess, '.\\' + modelName, None, modelName+'.chk')
log.Completed()

if logLevel>=3: DeepValidate()
