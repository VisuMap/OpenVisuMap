import numpy as np
import sys, socket, struct
from ModelUtil import *

cmdPort = 8888
CMD_TSNE = 200
BUFSIZE = 4096
fileName = 'SP500.csv'
epochs = 400
perplexityR = 0.05
mapDim = 3

#==============================================

def WriteMatrix(tcp, matrix):
    rows = matrix.shape[0]
    columns = matrix.shape[1]
    tcp.send(struct.pack('<ii', rows, columns))
    a = matrix.flatten()
    cnt = 0
    while cnt < a.size:
        n = min(a.size - cnt, int(BUFSIZE/4))
        tcp.send( a[cnt:(cnt+n)].tobytes() )
        cnt += n

def ReadMatrix(tcp):
    rows =  struct.unpack_from('<i', tcp.recv(4))[0]
    columns =  struct.unpack_from('<i', tcp.recv(4))[0]
    out = np.array([], dtype=np.float32)
    while out.size < (rows*columns):
        buf = tcp.recv(BUFSIZE)
        n = len(buf) // 4
        d = struct.unpack_from('<%df'%n, buf)
        out = np.append(out, np.array(d, dtype=np.float32))
    return np.reshape(out, [rows, columns])

#==============================================

D = np.genfromtxt(fileName, delimiter='\t', dtype=np.float32)

skt = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)
skt.connect(('localhost', cmdPort))
skt.sendall(struct.pack('<i', CMD_TSNE))
resp = skt.recv(BUFSIZE)
tcpCnt = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
tcpCnt.connect(('localhost', cmdPort))

tcpCnt.send(struct.pack('<iif', epochs, mapDim, perplexityR))

WriteMatrix(tcpCnt, D)
output = ReadMatrix(tcpCnt)

log = Logger()
log.ShowMatrix(output, view=2, title='TSNE')

