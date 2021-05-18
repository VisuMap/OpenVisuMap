import numpy as np
import sys, ModelLog
from keras.models import Sequential
from keras.layers import Activation
from keras.optimizers import SGD
from keras.layers import Dense
import keras

modelName = sys.argv[1]
maxEpochs = int(sys.argv[2])
logLevel = int(sys.argv[3])
refreshFreq = int(sys.argv[4])
log = ModelLog.Logger(8888)
batchSize = 25    

X = np.genfromtxt('inData.csv', delimiter='|', dtype=np.float32)
Y = np.genfromtxt('outData.csv', delimiter='|', dtype=np.float32)
x_dim = np.shape(X)[1]
y_dim = np.shape(Y)[1] 
N = np.shape(X)[0]

#=========================================================================

model = Sequential()
model.add(Dense(units=50, activation='sigmoid', kernel_initializer='uniform', input_dim=x_dim))
model.add(Dense(units=30, activation='sigmoid', kernel_initializer='uniform'))
model.add(Dense(units=20, activation='sigmoid', kernel_initializer='uniform'))
model.add(Dense(units=y_dim, activation='sigmoid', kernel_initializer='uniform'))

model.compile(loss=keras.losses.mean_squared_error, optimizer='adam')

model.fit(X, Y, epochs=maxEpochs, verbose=logLevel, batch_size=batchSize)
output = model.predict(X)
np.savetxt("predData.csv", output, delimiter='|', fmt='%.5f')
