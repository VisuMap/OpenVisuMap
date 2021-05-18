# File: ModelServer.py
# 
# Serving a tensorflow through requests from UPD ports.
#
# ==========================================================
import numpy as np
import sys, socket, struct, os, math

# create the command port as soon as possible
modelName = sys.argv[1]
portNr = int(sys.argv[2]) if len(sys.argv)>2 else 7777

import tensorflow as tf
import numpy as np
from ModelUtil import *
from ServerUtil import *

#--------------------------------------

md = ModelBuilder()
print('Loading model', modelName, '...')
md.LoadModel(modelName)
mdInfo = eval(md.GetVariableValue('ModelInfo'))
print('Model Loaded:', mdInfo)

if md.keepProbVar != None:
    md.sess.run(md.keepProbVar.assign(1.0))

skt = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
skt.bind(('', portNr))
sktCnt = None

sktListen = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sktListen.bind(('', portNr))
sktListen.listen(1)

inDim = 0
outDim = 0
labelDim = 0
if md.Input() != None:
    inDim = md.Input().get_shape()[1].value
if md.Output() != None:
    outDim = md.Output().get_shape()[1].value
if md.Label() != None:
    labelDim = md.LabelDim()
tracing = False

print('Accepting requests ("%s": %d->%d) at UDP port %d...'%(modelName, inDim, outDim, portNr))

#=============================================================

def Tensor2Matrix(t):
    dim = len(t.shape)
    if dim == 1:
        t = np.reshape(t, (t.shape[0], -1))
    elif dim == 0:
        t = np.reshape(np.array(np.float32(t)), (1, 1))
    else:
        t = np.reshape(t, (t.shape[0], -1))
    return t

def PackFloats(cmd, a=np.array([])):
    a = a.flatten()
    return struct.pack('ii%df'%a.size, cmd, a.size, *a)

def AcceptConnection(sktListen):
    sktCnt, _ = sktListen.accept()
    return sktCnt

def Respond(retAddr, cmd):
    skt.sendto(struct.pack('i', cmd), retAddr)

jobInput = None

def GetInputData(preloaded):
    if (preloaded == 1):
        if (jobInput is None) or (jobInput.shape[1] != inDim):
            Respond(retAddr, CMD_FAIL)
            return None
        Respond(retAddr, CMD_SUCCESS)
        return jobInput, None
    else:
        Respond(retAddr, CMD_SUCCESS)
        cnt = AcceptConnection(sktListen)
        input = md.log.ReadMatrix(cnt)
        if input.shape[1] != inDim:
            Respond(retAddr, CMD_FAIL)
            cnt.close()
        else:
            Respond(retAddr, CMD_SUCCESS)
            return input, cnt

#=============================================================

fctTable = {}

def fct_CMD_SHUTDOWN(buf, retAddr):
    print('Server shuting down...')
fctTable[CMD_SHUTDOWN] = fct_CMD_SHUTDOWN

def fct_CMD_TRACING(buf, retAddr):
    cmdValue = struct.unpack_from('<i', buf, 4)[0]
    global tracing
    tracing = True if (cmdValue > 0) else False
    if tracing:
        print('Tracing enabled.')
    else:
        print('Tracing disabled.')
fctTable[CMD_TRACING] = fct_CMD_TRACING

def fct_CMD_EVAL(buf, retAddr):
    preloadedInput =  struct.unpack_from('<i', buf, 4)[0]
    input, cnt = GetInputData(preloadedInput)
    if input is None: 
        return
    if cnt is None:
        sktCnt = AcceptConnection(sktListen)
    else: 
        sktCnt = cnt
    output = md.Eval(input)
    md.log.WriteMatrix(sktCnt, output)
    skt.sendto(PackFloats(CMD_SUCCESS), retAddr)
fctTable[CMD_EVAL] = fct_CMD_EVAL

def fct_CMD_MODEL_INFO(buf, retAddr):
    resp = struct.pack('iiii%is'%(len(modelName)), CMD_SUCCESS, inDim, outDim, labelDim, modelName.encode('utf-8'))
    skt.sendto(resp, retAddr)
fctTable[CMD_MODEL_INFO] = fct_CMD_MODEL_INFO

def fct_CMD_EXEC(buf, retAddr):
    sz =  struct.unpack_from('<i', buf, 4)
    script = struct.unpack_from('<' + str(sz[0]) + 's', buf, 8)
    script = script[0].decode("utf-8")
    exec(script)
    resp = struct.pack('i', CMD_SUCCESS)
    skt.sendto(resp, retAddr)
fctTable[CMD_EXEC] = fct_CMD_EXEC

def fct_CMD_SHOW_GRAPH(buf, retAddr):
    md.ShowGraph()
fctTable[CMD_SHOW_GRAPH] = fct_CMD_SHOW_GRAPH

def fct_CMD_READ_VARIABLE(buf, retAddr):
    varName =  struct.unpack_from('<%is'%(len(buf)-4), buf, 4)[0].decode('utf-8')
    if tracing: print('VAR: ', varName)
    values = md.GetVariableValue(varName)
    if values is None:
        skt.sendto(struct.pack('i', CMD_FAIL), retAddr)
    else:            
        values = Tensor2Matrix(values)
        skt.sendto(struct.pack('i', CMD_SUCCESS), retAddr)
        sktCnt = AcceptConnection(sktListen)
        md.log.WriteMatrix(sktCnt, values)
fctTable[CMD_READ_VARIABLE] = fct_CMD_READ_VARIABLE

def fct_CMD_READ_STRING(buf, retAddr):
    varName =  struct.unpack_from('<%is'%(len(buf)-4), buf, 4)[0].decode('utf-8')
    if tracing: print('VAR: ', varName)
    strValue = md.GetVariableValue(varName)
    if (strValue is None) or (type(strValue) is not bytes):
        retPkt = struct.pack('i', CMD_FAIL)
    else:            
        retPkt = struct.pack('i%ds'%(len(strValue)), CMD_SUCCESS, strValue)
    skt.sendto(retPkt, retAddr)
fctTable[CMD_READ_STRING] = fct_CMD_READ_STRING

def EvalVariable(buf, retAddr, pushToLabel):
    preloadedInput =  struct.unpack_from('<i', buf, 4)[0]
    varName =  struct.unpack_from('<%is'%(len(buf)-8), buf, 8)[0].decode('utf-8')
    tn = md.ToTensor(varName)
    if tracing: print('VAR:', varName)
    if tn is None:
        print('Unknown name: ', varName)
        Respond(retAddr, CMD_FAIL)
        return
    input,cnt = GetInputData(preloadedInput)
    if input is None: return
    if cnt is None:
        sktCnt = AcceptConnection(sktListen)
    else:
        sktCnt = cnt
    if pushToLabel:
        output = md.sess.run(tn, {md.Label():input})
    else:
        output = md.Eval2(tn, input)
    output = Tensor2Matrix(output)
    md.log.WriteMatrix(sktCnt, output)

def fct_CMD_EVAL_VARIABLE(buf, retAddr):
    EvalVariable(buf, retAddr, False)
fctTable[CMD_EVAL_VARIABLE] = fct_CMD_EVAL_VARIABLE 

def fct_CMD_EVAL_VARIABLE2(buf, retAddr):
    EvalVariable(buf, retAddr, True)
fctTable[CMD_EVAL_VARIABLE2] = fct_CMD_EVAL_VARIABLE2

# Evaluate variable with values for augment variable.
def EvalVarAug(buf, retAddr, withInput):
    varName =  struct.unpack_from('<%is'%(len(buf)-4), buf, 4)[0].decode('utf-8')
    aug1Name = mdInfo['AugVar1']
    aug2Name = mdInfo['AugVar2'] 
    if tracing: print('Var & augs:', varName, aug1Name, aug2Name)
    tn = md.ToTensor(varName)
    aug1 = md.GetVar(aug1Name)   
    aug2 = md.GetVar(aug2Name, showError=False) 
    if (tn is None) or (aug1 is None):
        print('Unknown name: %s or %s'%(varName, aug1Name))
        Respond(retAddr, CMD_FAIL)
        return
    if (mdInfo['Var2Dim'] > 0) and (aug2 is None):
        print('Unknown name: %s'%(aug2Name))
        Respond(retAddr, CMD_FAIL)
        return
    if withInput:
        if (jobInput is None) or (jobInput.shape[1] != inDim):
            print("No input!")
            Respond(retAddr, CMD_FAIL)
            return

    Respond(retAddr, CMD_SUCCESS)
    sktCnt = AcceptConnection(sktListen)
    augInput = md.log.ReadMatrix(sktCnt)
    rows = augInput.shape[0]
    columns = 1
    for dim in tn.get_shape().as_list():
        if dim is not None:
            columns *= dim

    dim1 = mdInfo['Var1Dim']

    feed = {}
    if withInput:
        output = np.empty([rows, jobInput.shape[0]*columns], dtype=np.float32)
        feed[md.inputHod] = jobInput
    else:
        output = np.empty([rows, columns], dtype=np.float32)
    for row in range(rows):
        feed[aug1] = augInput[row,:dim1]
        if aug2 is not None:
            feed[aug2] = augInput[row, dim1:]
        output[row] = md.sess.run(tn, feed).flatten()

    md.log.WriteMatrix(sktCnt, output)

# Get tensor for augments
def fct_CMD_AUG_2_VAR(buf, retAddr):
    EvalVarAug(buf, retAddr, False)
fctTable[CMD_AUG_2_VAR] = fct_CMD_AUG_2_VAR

# Get tensor for augments and input
def fct_CMD_IN_AUG_2_VAR(buf, retAddr):
    EvalVarAug(buf, retAddr, True)
fctTable[CMD_IN_AUG_2_VAR] = fct_CMD_IN_AUG_2_VAR

def fct_CMD_LIST_WEIGHTS(buf, retAddr):
    skt.sendto(struct.pack('i', CMD_SUCCESS), retAddr)
    sktCnt = AcceptConnection(sktListen)
    variables = tf.global_variables()
    sktCnt.send(struct.pack('i', len(variables)))
    for v in variables:
        row = v.name + '|' + str(v.shape)
        row_len = len(row)
        sktCnt.send(struct.pack('b%is'%(row_len), row_len, row.encode('utf-8')))
fctTable[CMD_LIST_WEIGHTS] = fct_CMD_LIST_WEIGHTS

varName2Var = None

def fct_CMD_WRITE_VARIABLE(buf, retAddr):
    global varName2Var
    varName =  struct.unpack_from('<%is'%(len(buf)-4), buf, 4)[0].decode('utf-8')
    if tracing: print('VAR: ', varName)
    if varName2Var == None:
        varName2Var = {v.name:v for v in tf.global_variables()}
    if varName not in varName2Var.keys():
        print('Unknown variable: ', varName)
        skt.sendto(struct.pack('i', CMD_FAIL), retAddr)
        return
    v = varName2Var[varName]
    skt.sendto(struct.pack('i', CMD_SUCCESS), retAddr)
    sktCnt = AcceptConnection(sktListen)
    values = md.log.ReadMatrix(sktCnt)
    values = np.reshape(values, v.shape)
    v.load(values, md.sess)
fctTable[CMD_WRITE_VARIABLE] = fct_CMD_WRITE_VARIABLE

def fct_CMD_LIST_OPERATIONS(buf, retAddr):
    #g = tf.get_default_graph().as_graph_def()
    #tf.graph_util.remove_training_nodes(g)
    skt.sendto(struct.pack('i', CMD_SUCCESS), retAddr)
    sktCnt = AcceptConnection(sktListen)
    nodeList = tf.get_default_graph().as_graph_def().node
    sktCnt.send(struct.pack('i', len(nodeList)))
    for v in nodeList:
        row = v.name + '|' + v.op
        if (v.op=='Const') and (v.attr['dtype'].type == 7):
            row += "Str"
        row_len = len(row)
        sktCnt.send(struct.pack('b%is'%(row_len), row_len, row.encode('utf-8')))        
fctTable[CMD_LIST_OPERATIONS] = fct_CMD_LIST_OPERATIONS

def fct_CMD_UPLOAD_MATRIX(buf, retAddr):
    global jobInput
    global jobSkt
    skt.sendto(struct.pack('i', CMD_SUCCESS), retAddr)
    jobSkt = AcceptConnection(sktListen)
    jobInput = md.log.ReadMatrix(jobSkt)
    jobSkt.close()
    if tracing: print('Loaded matrix: ', jobInput.shape)
fctTable[CMD_UPLOAD_MATRIX] = fct_CMD_UPLOAD_MATRIX

def fct_CMD_SET_INPUT(buf, retAddr):
    holderName =  struct.unpack_from('<%is'%(len(buf)-4), buf, 4)[0].decode('utf-8')
    inputVar = md.GetVariable(holderName)
    if inputVar is None:
        Respond(retAddr, CMD_FAIL)
    else:
        global inDim
        md.inputHod = inputVar
        inDim = inputVar.get_shape()[1].value
        if tracing:
            print('Changed input to: ', holderName, ', Dimension: ', inDim)
        skt.sendto(struct.pack('ii', CMD_SUCCESS, inDim), retAddr)
fctTable[CMD_SET_INPUT] = fct_CMD_SET_INPUT
#--------------------------------------------------------------

def fct_CMD_CUSTOM_JOB(buf, retAddr):
    global jobInput
    global jobSkt

    jobOp = struct.unpack_from('<i', buf, 4)[0]

    if jobOp == 0:
        skt.sendto(struct.pack('i', CMD_SUCCESS), retAddr)
        jobSkt = AcceptConnection(sktListen)
        jobInput = md.log.ReadMatrix(jobSkt)
        argColumns = np.empty((jobInput.shape[0], 2), dtype=np.float32)
        jobInput = np.concatenate((jobInput, argColumns), axis=1)
    elif jobOp == 1:
        a, b = struct.unpack_from('<ff', buf, 8)
        jobInput[:, jobInput.shape[1]-2:] = [a, b]
        md.log.WriteMatrix(jobSkt, md.Eval(jobInput))
    else:
        jobInput = None
        jobSkt.close()
fctTable[CMD_CUSTOM_JOB] = fct_CMD_CUSTOM_JOB

#=============================================================

while True:
    buf, retAddr = skt.recvfrom(BUFSIZE)
    cmd =  struct.unpack_from('<i', buf)[0]

    if tracing: 
        print('=>Cmd,Buf:', cmd, len(buf), flush=True)

    try:
        fctTable[cmd](buf, retAddr)
        if cmd == CMD_SHUTDOWN: break
    except Exception as e:
        print('Exception: ', e)
    finally:
        if sktCnt != None:
            sktCnt.close()
            sktCnt = None
        

