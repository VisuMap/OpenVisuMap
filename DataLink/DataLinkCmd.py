import socket, struct, time
import numpy as np

#============================================================================================
# class DataLinkCmd
#============================================================================================

class DataLinkCmd:
    skt = None
    port = None
    vmHost = None

    CMD_MSG=102    
    CMD_LOG_MSG=110
    CMD_RUN_SCRIPT=111
    CMD_LOG_CLEAR=112
    CMD_GET_EXPRESSION=114
    CMD_PING = 120
    CMD_OK = 121
    CMD_SH_MATRIX2=122
    CMD_GET_PROPERTY=125
    CMD_SET_PROPERTY=126
    CMD_LOAD_TABLE=127
    CMD_FAIL = 129
    CMD_LOAD_TABLE0=132
    CMD_SAVE_TABLE=133
    CMD_LOAD_BLOB=134
    CMD_SAVE_BLOB=135
    CMD_GET_ITEM_IDS=137
    CMD_SELECT_ITEMS=138
    CMD_UPDATE_LABELS=140
    CMD_LOAD_LABELS=141
    CMD_LOAD_DISTANCES = 142
    CMD_SET_COLUMIDS = 143
    CMD_LOAD_MAP = 144
    CMD_TSNE = 200

    def __init__(self, portNumber=8877, vmHost='localhost'):
        self.skt = socket.socket(socket.AF_INET, socket.SOCK_DGRAM, 0)
        self.port = portNumber
        self.vmHost = vmHost
        self.skt.connect((self.vmHost, portNumber))

    def Close(self):
        if self.skt != None:
            self.skt.shutdown(socket.SHUT_RDWR)
            self.skt.close()
            self.skt = None
    def __enter__(self):
        return self
    def __exit__(self, exc_type, exc_value, exc_tb):
        self.Close()

        
    def IsOK(self):
        r""" Receive a single response and check whether it OK.
        """
        buf = self.skt.recv(24)
        if (len(buf) >= 4):
            resp =  struct.unpack_from('<i', buf)
            if resp[0] == self.CMD_OK:
                return True
            else:
                return False
        else:
            return False

    def DoTsne(self, D, epochs=1000, perplexityRatio=0.05, mapDimension=2):
        r""" Perform t-SNE dimensionality reduction on a data table.
        """
        self.skt.sendall(struct.pack('<i', self.CMD_TSNE))
        self.IsOK()
        with self.ConnectToVisuMap() as tcpCnt:
            # sends the parameters for the command
            tcpCnt.send(struct.pack('<iif', epochs, mapDimension, perplexityRatio))
            # sends the data for the command
            self.WriteMatrix(tcpCnt, D)
            # receives the map
            result = self.ReadMatrix(tcpCnt)
        return result
    
    def UpdateLabels(self, labels):
        r"""Request the server to update labels of current map.
        """
        labels = labels.astype(np.int32)
        self.skt.sendall(bytearray(struct.pack('i', self.CMD_UPDATE_LABELS)))
        if self.IsOK():
            with self.ConnectToVisuMap() as tcpCnt:
                tcpCnt.send(bytearray(struct.pack('i', len(labels))))
                self.WriteArray(tcpCnt, labels)

    def LoadLabels(self):
        r"""Load the labels (data point type) of selected data points.
        """
        self.skt.sendall(bytearray(struct.pack('i', self.CMD_LOAD_LABELS)))
        if self.IsOK():
            with self.ConnectToVisuMap() as tcpCnt:
                labels = self.ReadArray(tcpCnt, valueType=np.int32)
            return labels

    def LoadMapXyz(self):
        r"""Load the coordinators of the data point in the current map
        """
        self.skt.sendall(bytearray(struct.pack('i', self.CMD_LOAD_MAP)))        
        if self.IsOK():
            with self.ConnectToVisuMap() as tcpCnt:
                xyz = self.ReadMatrix(tcpCnt)
            return xyz

    def ReportMsg(self, msg):
        r"""Request the server to display a message.
        """
        self.skt.sendall(bytearray(struct.pack('i'+str(len(msg))+'s', self.CMD_MSG, msg.encode('utf-8'))))

    def LogMsg(self, msg):
        r"""Added a line of message to the information pad of the server.
        """
        msg += '\n' 
        self.LogMsg0(msg)

    def LogMsg0(self, msg):
        r"""Added a message to the information pad of the server.
        """
        self.skt.sendall(bytearray(struct.pack('i'+str(len(msg))+'s', self.CMD_LOG_MSG, msg.encode('utf-8'))))

    def ClearLog(self):
        r"""Clear the information pad.
        """
        self.skt.sendall(bytearray(struct.pack('i', self.CMD_LOG_CLEAR)))

    def RecvMsg(self, skt):
        r"""Receive a message from the server.
        """
        buf, _ = skt.recvfrom(1024)
        if ( len(buf) >= 4):
            sz =  struct.unpack_from('<i', buf)
            msg = struct.unpack_from('<' + str(sz[0]) + 's', buf, 4)
            return msg[0].decode("utf-8")
        return ""

    def RecvMsgTcp(self, tcpSkt):
        buf = tcpSkt.recv(4)
        if ( len(buf) == 4):
            sz =  struct.unpack_from('<i', buf)[0]
            data = tcpSkt.recv(sz)
            msg = struct.unpack_from('<%ds'%sz, data)[0]
            return msg.decode("utf-8")
        else:
            return None
            
    def GetExpression(self, expression):
        r"""Get an expression from the server.
        """
        self.skt.settimeout(5)
        self.skt.sendall(bytearray(struct.pack('i'+str(len(expression))+'s', self.CMD_GET_EXPRESSION, expression.encode('utf-8'))))
        return self.RecvMsg(self.skt)

    def GetProperty(self, propName):
        r"""Get a property from the server.
        """
        self.skt.sendall(bytearray(struct.pack('i'+str(len(propName))+'s', self.CMD_GET_PROPERTY, propName.encode('utf-8'))))
        return self.RecvMsg(self.skt)

    def SetProperty(self, propName, propValue):
        r"""Set a property on the server.
        """
        msg = propName + '|' + propValue
        self.skt.sendall(bytearray(struct.pack('i'+str(len(msg))+'s', self.CMD_SET_PROPERTY, msg.encode('utf-8'))))
        return 
    
    def GetItemIds(self):
        r"""Get the id list of all data points.
        """
        self.skt.sendall(bytearray(struct.pack('i', self.CMD_GET_ITEM_IDS)))
        with self.ConnectToVisuMap() as tcpCnt:
            msg=self.RecvMsgTcp(tcpCnt)        
        return msg.split('|')
    
    def SelectItems(self, itemIndexes):
        r"""Mark a list of data points as selected.
        """
        itemIndexes = np.array(itemIndexes)
        self.skt.sendall(bytearray(struct.pack('i', self.CMD_SELECT_ITEMS)))
        with self.ConnectToVisuMap() as tcpCnt:
            tcpCnt.send(bytearray(struct.pack('i', len(itemIndexes))))
            self.WriteArray(tcpCnt, itemIndexes)

    def ReadArray(self, tcp, length=0, outBuffer=None, valueType=np.float32):
        r"""Receive an array of float32
        """
        if length==0:
            length =  struct.unpack_from('<i', tcp.recv(4))[0]   
        if outBuffer is None:
            outBuffer = np.zeros(length, dtype=valueType)

        bufview = outBuffer.view(dtype=np.byte)
        toread = length*4
        while toread > 0:
            len = tcp.recv_into(bufview, toread)
            if len == 0:
                print('Receiving aborted, rest size: ', toread)
                break
            bufview = bufview[len:]
            toread -= len
        return outBuffer

    def WriteArray(self, sktCnt, values):
        r"""Send an array of values of type int32 or float32.
        """
        view = memoryview(bytearray(values))
        toWrite = 4*values.shape[0]
        while toWrite > 0:
            sent = sktCnt.send(view)
            if sent <= 0:
                print('Sending not completed!  len/toWrite: %d/%d'%(sent,toWrite))
                break
            view = view[sent:]
            toWrite -= sent

    def ReadMatrix(self, tcp):
        r"""Receive a matrix of float32 values.
        """
        rows =  struct.unpack_from('<i', tcp.recv(4))[0]
        columns =  struct.unpack_from('<i', tcp.recv(4))[0]
        out = np.zeros(rows * columns, dtype=np.float32)
        toread = 4 * rows * columns
        if toread == 0:
            return np.zeros((rows, columns), dtype=np.float32)
        
        bufview = out.view(dtype=np.byte)
        while toread>0:  
            len = tcp.recv_into(bufview, toread)
            bufview = bufview[len:]
            toread -= len
        return np.reshape(out, [rows, columns])

    def WriteMatrix(self, sktCnt, matrix):
        r"""Send a matrix of float32 values.
        """
        if matrix is None:
            sktCnt.send(struct.pack('<ii', 0, 0))
            return
        rows = matrix.shape[0]
        columns = matrix.shape[1]
        sktCnt.send(struct.pack('<ii', rows, columns))
        view = memoryview(bytearray(matrix))
        toWrite = 4*rows*columns 
        while toWrite > 0:
            try:
                len = sktCnt.send(view)
                view = view[len:]
                toWrite -= len
            except:
                print("Failed to send the matrix: %d"%toWrite)
                break
    
    def WriteStringArray(self, sktCnt, strList):
        r"""Send a list of string to the server
        """
        pkt = '|'.join(strList).encode()       
        toWrite = len(pkt) 
        sktCnt.send(struct.pack('i', toWrite))
        view = memoryview(bytearray(pkt))
        while toWrite > 0:
            n = sktCnt.send(view)
            view = view[n:]
            toWrite -= n
    
    def LoadTableWithLabel(self, dsName='', tmout=20):
        r""" Load enabled data points and their types from a table
            dsName: the data table name, or '' for the current table.
            tmout: the timeout in seconds.
        """
        self.skt.sendall(bytearray(struct.pack('i%ds'%(len(dsName)), self.CMD_LOAD_TABLE, dsName.encode('utf-8'))))
        self.skt.settimeout(tmout)
        if not self.IsOK(): 
            return None, None
        with self.ConnectToVisuMap(tmout) as tcpCnt:
            dsTable = self.ReadMatrix(tcpCnt)
            rowTypes = self.ReadMatrix(tcpCnt)
        return dsTable, rowTypes

    def LoadTable(self, dsName='', tmout=20):
        r""" Load a data table.
            dsName: the data table name, or '' for the current table; 
                    or '@' for selected data; '$' for selected map point positions; '+' for selected data in current active form.
            tmout: the timeout in seconds.
        """        
        self.skt.sendall(bytearray(struct.pack('i%ds'%(len(dsName)), self.CMD_LOAD_TABLE0, dsName.encode('utf-8'))))        
        self.skt.settimeout(tmout)
        if self.IsOK():
            with self.ConnectToVisuMap(tmout) as tcpCnt:
                dsTable = self.ReadMatrix(tcpCnt)
            return dsTable
        else:
            return None

    def LoadDistances(self, tmout=120):
        r""" Load the distance table of the current data map.
            tmout: the timeout in seconds.
        """        
        self.skt.sendall(bytearray(struct.pack('i', self.CMD_LOAD_DISTANCES)))
        self.skt.settimeout(tmout)
        if self.IsOK():
            with self.ConnectToVisuMap(tmout) as tcpCnt:
                distTable = self.ReadMatrix(tcpCnt)
            return distTable
        else:
            return None

    def LoadBlob(self, blobName, offset=0, size=0, outBuffer=None):
        r"""Obtain a blob as an array of 32-bit values.
        """
        self.skt.sendall(bytearray(struct.pack('iii%ds'%(len(blobName)), self.CMD_LOAD_BLOB, offset, size, blobName.encode('utf-8'))))
        buf = self.skt.recv(8)
        if ( len(buf) >= 8):
            resp =  struct.unpack_from('<ii', buf)
            if resp[0] != self.CMD_OK:
                return None
            length = resp[1]
            with self.ConnectToVisuMap() as tcpCnt:
                dsBlob = self.ReadArray(tcpCnt, length, outBuffer)
            return dsBlob
        else:
            print('Failed to load blob %s, %d %d'%(blobName, offset, size))
            return None

    def SaveBlob(self, blobName, values, baseLocation=0, tmout=20):
        r""" Save an array of float32 values into a blob. If the blob name is in use, an index will be
        appended to the new blob name.
        Arg:
            blobName: the name of the destionation blob.
            values: the values.
            baseLocation: the base-location of the sequence.
        """
        values = values.flatten()
        self.skt.sendall(bytearray(struct.pack('iii%ds'%(len(blobName)), self.CMD_SAVE_BLOB, values.shape[0], baseLocation, blobName.encode('utf-8'))))
        if not self.IsOK(): return
        with self.ConnectToVisuMap(tmout) as tcpCnt:
            self.WriteArray(tcpCnt, values)

    def SaveTable(self, table, dsName, tmout=20, description=None):
        r""" Save a numpy matrix into a VisuMap dataset table.

        Arg:
            table: the table to be saved
            dsName: the name of the saved table.
            tmout: timeout value in second.
            description: a optional description string for the dataset.
        """
        if dsName is None:
            return 
        if table is None:
            return

        msg = dsName
        if description is not None:
            msg = msg + '||' + description
        self.skt.sendall(bytearray(struct.pack('i%ds'%(len(msg)), self.CMD_SAVE_TABLE, msg.encode('utf-8'))))
        self.skt.settimeout(tmout)        
        if not self.IsOK(): 
            self.skt.settimeout(None)
            return
        self.skt.settimeout(None)
        with self.ConnectToVisuMap(tmout) as tcpCnt:
            self.WriteMatrix(tcpCnt, table)

    def SetColumnIds(self, colIds):
        self.skt.sendall(bytearray(struct.pack('ii', self.CMD_SET_COLUMIDS, len(colIds))))
        if not self.IsOK(): 
            return False
        with self.ConnectToVisuMap(24) as tcpCnt:
            self.WriteStringArray(tcpCnt, colIds)
        return True

    def Ping(self, tmout=5):
        r"""Ping the server to validate the connection.
        """
        self.skt.settimeout(tmout)
        self.skt.sendall(bytearray(struct.pack('i', self.CMD_PING)))
        return self.IsOK()

    def RunScript(self, script):
        r"""Run JavaScript string on the server.
        """
        self.skt.sendall(bytearray(struct.pack('i'+str(len(script))+'s', self.CMD_RUN_SCRIPT, script.encode('utf-8'))))

    def BufferedSend(self, data, tcp):
        r"""Send a numpy array to the server.
        """
        a = data.flatten()
        cnt = 0
        BUFSIZE = 16*1024
        while cnt < a.size:
            n = min(a.size - cnt, int(BUFSIZE/4))
            buf = a[cnt:(cnt+n)].tobytes() 
            k = tcp.send( buf )
            if k < n*4 :
                remaining = n*4 - k 
                idx = k
                while remaining > 0:
                    k = tcp.send( buf[idx:] )
                    idx += k
                    remaining -= k
            cnt += n

    def ShowMatrix (self, matrix, rowInfo=None, view=0, access='n', title='Tensor', viewIdx=0):
        r"""Show a matrix on VisuMap server.
        Args:
          matrix: a numpy array; the data to be displayed.
          rowInfo: an integer, a list or a numpy array of integers; for 
            the attributes of the rows. If specified by a numpy array, the first 16 bits are for data point attributes.
          view: the view type to display the data; 0: heatmap; 1: curve chart; 
            2: PCA; 3: bar band; 4: XY-Plot; 5: spectrum band; 6: mount view; 
            7: digram grid; 8: mapping rows; 9: mapping columns; 10: sort rows; 
            11: sort columns; 12: 2D map; 13: 3D map; 14: Bar view; 
            15: Spectrum view; 16: Big bar view; 17: The main view map;
          access: relationship to previous data view: 'r' for replacing 
            existing view; 'n' for a new view; 'a' appending to previous view; 
            'R': For view type 13: the map will be additionally recorded by a clip-recorder.
          title: the window title.
          viewIdx: an index to support multiple views of the same view type.
        """
        if matrix.dtype != np.float32: 
            matrix = matrix.astype(np.float32)
        flags = 0 if rowInfo is None else 1
        if len(matrix.shape) == 1:
            rows = 1
            columns = matrix.shape[0]
        else:
            rows = matrix.shape[0]
            columns = matrix.shape[1]
        msg = title + '||' + access
        self.skt.sendall(bytearray(struct.pack('iiiiii%ds'%(len(msg)), self.CMD_SH_MATRIX2, view, rows, columns, flags, viewIdx, msg.encode('utf-8') )))
        if not self.IsOK(): return

        with self.ConnectToVisuMap(2) as tcpCnt:
            self.BufferedSend(matrix, tcpCnt)
            if rowInfo is not None:
                if isinstance(rowInfo, np.ndarray):
                    types = rowInfo
                else:
                    types = []
                    if type(rowInfo) is int:
                        types = [rowInfo] * rows
                    else:
                        for t, rInfo in enumerate(rowInfo):
                            K = rInfo if type(rInfo) is int else int( np.prod(rInfo.get_shape()) )
                            types += [t] * K
                    types = np.array(types, dtype=np.int32)
                self.BufferedSend(types, tcpCnt)
        return

    def ConnectToVisuMap(self, tmout=0):
        r"""Initialize a connection to the VisuMap server.
        """
        tcpCnt = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        tcpCnt.connect((self.vmHost, self.port))
        if tmout != 0:
            tcpCnt.settimeout(tmout)
        return tcpCnt

def LoadFromVisuMap(metric='euclidean'):
    print('Loading data from VisuMap...')
    with DataLinkCmd() as log:
        if mtr == 'precomputed':
            ds = log.LoadDistances(tmout=600)
        else:
            ds = log.LoadTable(dsName='+')
            if (ds is None) or (ds.shape[0]==0) or (ds.shape[1]==0):
                ds = log.LoadTable('@', tmout=180)
                if (ds.shape[0]==0) or (ds.shape[1]==0):
                    print('No data has been selected')
                    time.sleep(4.0)
                    quit()
    ds = np.nan_to_num(ds)
    print("Loaded table: ", ds.shape)
    return ds

def ShowToVisuMap(map, title):
  with DataLinkCmd() as cmd:
    mapDim = map.shape[1]
    if mapDim == 1:
        cmd.ShowMatrix(map, view=14, title=title)
        cmd.RunScript('pp.NormalizeView();pp.SortItems(true);')
    elif mapDim == 2:
        cmd.ShowMatrix(map, view=12, title=title)
        cmd.RunScript('pp.NormalizeView()')
    elif mapDim == 3:
        cmd.ShowMatrix(map, view=13, title=title)
        cmd.RunScript('pp.DoPcaCentralize()')
