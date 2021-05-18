# File: test.client.py
# 
# Testing tensorflow model through requests to UPD ports.
#
# ==========================================================

import sys, socket, struct
import numpy as np
from ServerUtil import *
from DataUtil import *
from ModelUtil import Logger

serverPort = int(sys.argv[1])
skt = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)
skt.connect(('localhost', serverPort))
skt.settimeout(5)
log = Logger()

Arg = np.genfromtxt('A.csv', delimiter='|').astype(np.float32)
__, XX = log.OpenDataset('', target='Shp', dataGroup=2, tmout=60)
K, N, aDim = 12, XX.shape[0], Arg.shape[1]
dsList = RotateX(XX, Arg, K)

#------------------------------------------------------

def ResponseOK():
    resp, _ = skt.recvfrom(BUFSIZE)
    ret =  struct.unpack_from('<i', resp)[0]
    return ret == CMD_SUCCESS

def Eval(input):
    skt.sendall(struct.pack('ii', CMD_EVAL, 0))
    assert ResponseOK(), 'Eval failed'
    tcpCnt = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    tcpCnt.connect(('localhost', serverPort))
    log.WriteMatrix(tcpCnt, input)
    retMatrix =log.ReadMatrix(tcpCnt)
    tcpCnt.close()
    assert ResponseOK(), 'Eval failed(2)'
    return retMatrix

#------------------------------------------------------

'''
for k in range(K):
    ret = Eval(dsList[k][0]).transpose()
    log.ShowMatrix(ret, rowInfo=k, view=5, access='a')
'''

mm = np.zeros([K*aDim, N], dtype=np.float32)
for k in range(K):
    ret = Eval(dsList[k][0]).transpose()
    for i in range(aDim):
        mm[k+i*K, :] = ret[i, :]
log.ShowMatrix(mm, rowInfo=aDim*[K], view=5, access='a')

sep = np.reshape(np.array([-0.75+k*1.55/N for k in range(N)], dtype=np.float32), [1, N])
log.ShowMatrix(sep, view=5, access='a')

