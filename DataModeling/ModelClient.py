from ServerUtil import *
from ModelUtil import *

class ModelClient:
    skt = None
    serverPort = 0
    log = None

    def __init__(self, serverPort=7777, visualPort=8888):
        self.skt = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)
        self.skt.connect(('localhost', serverPort))
        self.serverPort = serverPort
        self.skt.settimeout(5)
        self.log = Logger(visualPort)

    def GetModelName(self):
        self.skt.sendall(struct.pack('i', CMD_MODEL_INFO))
        resp, _ = self.skt.recvfrom(BUFSIZE)
        ret =  struct.unpack_from('iii', resp)
        return struct.unpack_from('<%is'%(len(resp)-12), resp, 12)[0].decode('utf-8')

    def ResponseOK(self):
        resp, _ = self.skt.recvfrom(BUFSIZE)
        ret =  struct.unpack_from('<i', resp)[0]
        return ret == CMD_SUCCESS

    def GetTensor(self, input, varName):
        return self.GetTensor0(input, varName, False)

    def GetTensor2(self, input, varName):
        return self.GetTensor0(input, varName, True)

    def ConnectToServer(self):
        tcpCnt = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        tcpCnt.connect(('localhost', self.serverPort))
        return tcpCnt

    def GetTensor0(self, input, varName, pushToLabel):
        cmd = CMD_EVAL_VARIABLE2 if pushToLabel else CMD_EVAL_VARIABLE
        preloaded = 1 if input is None else 0
        self.skt.sendall(struct.pack('ii%ds'%(len(varName)), cmd, preloaded, varName.encode('utf-8')))
        assert self.ResponseOK(), 'GetTensor failed'
        tcpCnt = self.ConnectToServer()
        if input is not None:
            self.log.WriteMatrix(tcpCnt, input)
            assert self.ResponseOK(), 'sending input data failed'
        retMatrix = self.log.ReadMatrix(tcpCnt)
        tcpCnt.close()
        return retMatrix

    def WriteVariable(self, varName, values):
        self.skt.sendall(struct.pack('i%ds'%(len(varName)), CMD_WRITE_VARIABLE, varName.encode('utf-8')))
        assert self.ResponseOK(), 'Eval failed'
        tcpCnt = self.ConnectToServer()
        self.log.WriteMatrix(tcpCnt, values)
        tcpCnt.close()

    def UploadInput(self, input):
        self.skt.sendall(struct.pack('i', CMD_UPLOAD_MATRIX))
        assert self.ResponseOK(), 'Upload failed'
        tcpCnt = self.ConnectToServer()
        self.log.WriteMatrix(tcpCnt, input)
        tcpCnt.close()

