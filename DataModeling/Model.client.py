# File: Model.client.py
# 
# Testing tensorflow model through requests to UPD ports.
#
# ==========================================================

import sys, socket, struct
import numpy as np
from ServerUtil import *

serverPort = int(sys.argv[1])

skt = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)
skt.connect(('localhost', serverPort))
skt.settimeout(5)

cmd = int(sys.argv[2])

if cmd == 0:
    skt.sendall(struct.pack('i', CMD_SHUTDOWN))

elif cmd == 1:
    skt.sendall(struct.pack('i', CMD_MODEL_INFO))
    resp, _ = skt.recvfrom(BUFSIZE)
    ret =  struct.unpack_from('iii', resp)
    mdName = struct.unpack_from('<%is'%(len(resp)-12), resp, 12)[0].decode('utf-8')
    print('Model Name: %s, InputDim: %d, OutputDim: %d'%(mdName, ret[1], ret[2]))
