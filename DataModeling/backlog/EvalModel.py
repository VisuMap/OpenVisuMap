import numpy as np
import cntk as C
import sys
from cntk.ops import *
from numpy import genfromtxt

model = C.load_model(sys.argv[1] + ".mod")
#print(str(model))

test_data = genfromtxt('testData.csv', delimiter='|', dtype=np.float32)
N = np.shape(test_data)[0]

output = model.eval({"in_var":test_data})
output_dim = np.shape(output)[2]
output = output.reshape(N, output_dim)
np.savetxt("predData.csv", output, delimiter='|', fmt='%.5f')
