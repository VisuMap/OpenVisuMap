import numpy as np
import sys, socket, struct
from DataLinkCmd import *

fileName = 'SP500.csv'

D = np.genfromtxt(fileName, delimiter='\t', dtype=np.float32)

cmd = DataLinkCmd()
result = cmd.DoTsne(D, epochs=500, perplexityRatio=0.01, mapDimension=2)

# display the result
import matplotlib.pyplot as plt
plt.scatter(result[:,0], result[:,1], 0.5)
plt.xlabel('tSNE-1'); plt.ylabel('tSNE-2'); plt.show()

# display the result map on VisuMap
cmd.ShowMatrix(result, view=2, title='TSNE')
