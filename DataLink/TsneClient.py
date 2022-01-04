import numpy as np
import sys, socket, struct
from DataLinkCmd import *

fileName = 'SP500.csv'
epochs = 400
perplexityR = 0.05
mapDim = 2

D = np.genfromtxt(fileName, delimiter='\t', dtype=np.float32)

cmd = DataLinkCmd()
# sends the command
cmd.skt.sendall(struct.pack('<i', cmd.CMD_TSNE))
cmd.IsOK()

with cmd.ConnectToVisuMap() as tcpCnt:
    # sends the parameters for the command
    tcpCnt.send(struct.pack('<iif', epochs, mapDim, perplexityR))
    # sends the data for the command
    cmd.WriteMatrix(tcpCnt, D)
    # receives the map
    result = cmd.ReadMatrix(tcpCnt)

# display the result
import matplotlib.pyplot as plt
plt.scatter(result[:,0], result[:,1], 0.5)
plt.xlabel('tSNE-1'); plt.ylabel('tSNE-2'); plt.show()

# display the result map on VisuMap
cmd.ShowMatrix(result, view=2, title='TSNE')
