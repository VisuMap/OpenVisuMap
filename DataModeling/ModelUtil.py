#================================================
# ModelUtil.py
# classes to build tensorflow models.
#================================================
import math, time, os, sys, csv, struct, socket
import tensorflow as tf
import tensorflow.compat.v1 as tf1
import numpy as np
from random import shuffle
from ModelLogger import Logger

#================================================

class CmdOptions:
    modelName = None
    epochs = 0
    logLevel = 2
    refreshFreq = 20
    job = 0
    jobArgument = 'A'
    arg = 0

    r"""Load the command line options intp an object.
    """
    def __init__(self, argv=None):
        if argv == None: argv = sys.argv
        print("TensorFlow version:", tf.__version__, " Model script ", argv[0], "...")
        argcnt = len(argv)
        self.modelName = argv[1]        if argcnt>1 else '<NotSave>'
        self.epochs = int(argv[2])      if argcnt>2 else 100
        self.logLevel = int(argv[3])    if argcnt>3 else 2
        self.refreshFreq = int(argv[4]) if argcnt>4 else 20
        self.job = int(argv[5])         if argcnt>5 else 0
        self.jobArgument = argv[6]      if argcnt>6 else 'A'
        self.arg = (ord(self.jobArgument) - ord('A')) if (self.modelName.find('_') > 0) else 0
        self.jj = self.job + 1
        print('Model: %s, Epochs: %d, Job: %d'%(self.modelName, self.epochs, self.job))

    def Train(self, md, ds, epCall=None):
        md.SetAdamOptimizer(self.epochs, ds.N)
        md.Train(ds, self.epochs, self.logLevel, self.refreshFreq, epCall=epCall)
        md.Save(self.modelName)
        time.sleep(1.0)

    def Save(self, md):
        md.modelInfo.update({'epochs':self.epochs, 'logLevel':self.logLevel, 'refreshFreq':self.refreshFreq})

#============================================================================================
# Dataset class: load and manage data from the VisuMap environment
#============================================================================================

class ModelDataset:
    r"""Load training data from current VisuMap session. 

      Args:      
      mdInput: '*': all table columns; 
        '+': all table data enabled by the current filter of the current map.
        '<C1>|<C2>|....': A list of data column identifiers.
        <FilterName>|<GroupName>: A filter name or a group name.
      mdOutput: 'Var:*': All table columns;
        '+': all table data enabled by the current filter of the current map.
        'Var:<C1>|<C2>|....': A list of data column identifiers.
        'Var:<FilterName>|<GroupName>: A filter name or a group name.
        'Shp': The current data point coordinates.
        'Clr': The current data point types.
        'ClrShp': The data point types and coordinates.      
    """
    X = None
    Y = None
    xDim = 0
    yDim = 0
    N = 0
    mdInput = ''
    mdOutput = ''

    def __init__(self, mdInput='Nul', mdOutput='Nul', vmHost='localhost'):
        if mdInput=='Nul' and mdOutput=='Nul':
            return
        self.mdInput = mdInput
        self.mdOutput = mdOutput
        self.X, self.Y, self.V, self.mapDim = Logger(vmHost=vmHost).LoadTrainingData(mdInput, mdOutput)
        self.UpdateAux()

        if self.xDim != 0:
            print("Loaded Data: %d: [%d] => [%d]"%(self.X.shape[0], self.xDim, self.yDim))
        elif self.yDim != 0:
            print('Loaded output data: %d x %d'%(self.Y.shape[0], self.Y.shape[1]))
        else:
            print("Dataset: no data loaded!")

    def LoadFromFile(self, inputFile=None, outputFile=None):
        if (inputFile!=None) and os.path.isfile(inputFile):
            self.X = np.genfromtxt(inputFile, delimiter='|', dtype=np.float32)
            self.xDim = self.X.shape[1]
            self.N = self.X.shape[0]
        if (outputFile!=None) and os.path.isfile(outputFile):
            self.Y = np.genfromtxt(outputFile, delimiter='|', dtype=np.float32)
            if (len(self.Y.shape)==1): self.Y = np.reshape(self.Y, (len(self.Y),1))
            self.yDim = self.Y.shape[1]

    def UpdateAux(self):
        if self.X is not None:
            self.N = self.X.shape[0]
            self.xDim = self.X.shape[1]
        if self.Y is not None:
            self.yDim = self.Y.shape[1]

    #------------------------------------------------
    # API for the Train() method
    batchSize = 0
    rowIdx = 0
    idxMap = None
    md = None

    def InitEpoch(self, md):
        self.UpdateAux()
        self.md = md
        self.batchSize = md.batchSize
        if self.idxMap is None:
            self.idxMap = np.arange(self.N)
        if md.randomizingData:
            np.random.shuffle(self.idxMap)
        self.rowIdx = 0

    def BeginStep(self):
        if self.rowIdx >= self.N:
            return False
        idx1 = min(self.rowIdx + self.batchSize, self.N)
        idxList =  self.idxMap[self.rowIdx : idx1]
        if self.md.inputHod is not None:
            self.md.feed[self.md.inputHod] = np.take(self.X, idxList, axis=0)
        if self.md.outputHod is not None:
            self.md.feed[self.md.outputHod] = np.take(self.Y, idxList, axis=0)
        self.rowIdx = idx1
        return True
    #------------------------------------------------

#============================================================================================
# ModelBuilder 
#============================================================================================

class ModelBuilder:
    r"""Create new tensorflow model    
    Args:
      inputDim: the size of the input layer. If 0, no input placeholder will be created.
      outputDim: the size of the label layer. If 0, no label placehodlder layer will be created.
      job: the index to identify the training job.
    """
    top = None
    inputHod = None
    outputHod = None
    keepProbVar = None
    trainTarget = None
    cost = None
    keepProb = 0.75
    batchSize = 25
    learningRate = None
    r0 = 0.001
    decay = 0.8
    sess = None
    randomizingData = True
    log = None
    epochCall = None
    startTime = 0.0
    trainingTime = 0.0
    job = 0
    lastError = 0
    lastEpoch = 0
    globalStep = None
    sktCmd = None
    feed = None
    weightInitializer = tf.glorot_normal_initializer()
    initialized = False
    modelInfo = None
    vmHost = None

    def __init__(self, inputDim=0, outputDim=0, job=0, vmHost='localhost'):
        self.job = job
        self.modelInfo = {}
        self.vmHost = vmHost
        self.InitLogger()
        self.InitSession()
        if inputDim != 0: 
            self.InitModel(inputDim, outputDim)

    def InitModel(self, inputDim, outputDim):
        self.feed = {}
        if inputDim > 0:
            self.inputHod = tf1.placeholder( tf.float32, shape=[None, inputDim], name='InputHolder')
            self.top = self.inputHod
        if outputDim > 0:
            self.outputHod = tf1.placeholder( tf.float32, shape=[None, outputDim], name='LabelHolder')
        self.globalStep = tf.Variable(0, trainable=False, name='globalStep')

    def LoadModel(self, modelName, resetLearningRate = True):
        self.CleanModel()
        self.InitLogger()
        self.InitSession()
        if self.feed == None:
            self.feed = {}

        saver = tf1.train.import_meta_graph(modelName + '.meta')
        #saver.restore(self.sess, tf.train.latest_checkpoint('./', modelName + '.chk'))
        saver.restore(self.sess, modelName)

        self.inputHod = self.GetVariable('InputHolder', False)
        self.outputHod = self.GetVariable('LabelHolder', False)
        self.keepProbVar = self.GetVariable('KeepProbVar:0', False)
        self.trainTarget = self.GetVariable('TrainingTarget')
        self.cost = self.GetVariable('CostTensor')
        self.top = self.GetVariable('OutputTensor')
        self.globalStep = self.GetVar('globalStep:0')

        if resetLearningRate:
            self.SetVar(self.globalStep, 0)
        self.initialized = True

    def CleanModel(self):
        tf1.reset_default_graph()
        self.sess.close()
        self.sess = None
        self.feed = None
        self.initialized = False
        self.InitSession()

    def InitSession(self):
        if self.sess != None: return
        #
        # Notice: setting inter_op_parallelism_threads to 2 or highier value
        # will in general increase the performance of low-load sessions, i.e. 
        # with 2 or 3 parallel jobs; But, will decrease the performance 
        # significantly for high-load sessions where jobs uses 
        # the softmax_cross_entropy_with_logits_v2 function.
        # 
        cfg = tf1.ConfigProto(log_device_placement=False, 
           inter_op_parallelism_threads=1,
           intra_op_parallelism_threads=1,
           allow_soft_placement = True)
           
        cfg.gpu_options.allow_growth = True 
        cfg.graph_options.optimizer_options.global_jit_level = tf1.OptimizerOptions.ON_1
        if 'COLAB_TPU_ADDR' in os.environ:
            self.sess = tf.Session(target='grpc://' + os.environ['COLAB_TPU_ADDR'], config=cfg)
        else:
            self.sess = tf1.Session(config=cfg)
        return self.sess

    def InitAllVariables(self):
        self.sess.run(tf1.global_variables_initializer())
        self.initialized = True

    def OpenCmdPort(self, portNr=3333):
        self.sktCmd = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.sktCmd.settimeout(0)
        self.sktCmd.bind(('', portNr))

    def GetCmd(self):
        if self.sktCmd!=None:
            try:
                buf, _ = self.sktCmd.recvfrom(1024)
                sz =  struct.unpack_from('<i', buf)[0]
                script = struct.unpack_from('<%is'%sz, buf, 4)
                return script[0].decode('utf-8')
            except BlockingIOError:
                return None
        return None

    #============================================================================================

    def NewMatrix(self, m, n, name='mx'):
        if self.weightInitializer == None:
            return tf.Variable( tf.random_uniform([m, n], -0.1, 0.1, dtype=tf.float32), name=name )
        else:
            return tf.Variable( self.weightInitializer([m,n]), name=name )


    def AddMatrixLayer(self, dim):
        W = self.NewMatrix(self.TopDim(), dim)
        self.top = tf.matmul(self.top, W)

    def AddRcpLayer(self, size, eps=0.025):
        def RCP(x): 
            return tf.reciprocal(tf.abs(x) + eps)
        with tf.name_scope('RcpLayer'):
            W = self.NewMatrix(self.TopDim(), size)
            Y = tf.matmul(RCP(self.top), tf.abs(W))
            self.top = size * RCP(Y)

    def AddSquaredLayer(self, size):
        with tf.name_scope('SqLayer'):
            W = self.NewMatrix(self.TopDim(), size)
            Y = tf.matmul(self.top*self.top, W)
            #self.top = tf.abs(Y/self.TopDim())
            Y2 = tf.sqrt(tf.abs(Y/self.TopDim()) + 0.000001)
            self.top = tf.where(Y>0, Y2, -Y2)

    def AddFilter2(self, fct, idxStart, idxEnd=None):
        if idxEnd == None: idxEnd = self.TopDim()
        lanes = []
        ll = self.top
        llDim = self.TopDim()
        if 0 < idxStart: lanes.append(ll[:, 0:idxStart])
        if idxStart < idxEnd: lanes.append(fct(ll[:, idxStart:idxEnd]))
        if idxEnd < llDim: lanes.append(ll[:, idxEnd:llDim])
        self.top = tf.concat(lanes, axis=1)

    def AddFilter(self, fct):
        self.top = fct( self.top )

    def AddDropout(self, keepProb=None):
        if keepProb != None:
            self.keepProb = keepProb
        if self.keepProbVar == None:
            self.keepProbVar = tf.Variable(self.keepProb, trainable=False, name='KeepProbVar', dtype=np.float32)
        self.top = tf.nn.dropout(self.top, rate=1.0-self.keepProbVar)

    def AddBias(self):
        bias = tf.Variable( tf.zeros([self.TopDim()]), name='bias' )
        self.top = tf.add(self.top, bias, name='AddBias')
        return bias

    # Shifting and scaling input tensor to the range of sigmoid() function.
    def AddScalingFrom(self, X):
        vMax = np.amax(X, axis=0)
        vMin = np.amin(X, axis=0)
        vRange = np.maximum(vMax - vMin, 0.000001)
        self.AddFilter(lambda x: (x - vMin)/vRange)

    # Shifting and scaling output of sigmoid() to the range of Y's columns.
    def AddScalingTo(self, Y, gap=0.01, addOutputLayer=True):
        if addOutputLayer:
            self.AddLayers(Y.shape[1], activation=tf.nn.sigmoid)
        vMax = np.amax(Y, axis=0)
        vMin = np.amin(Y, axis=0)
        vRange = np.maximum(vMax - vMin, 0.000001)
        vMin -= gap*vRange
        vRange += 2*gap*vRange
        self.AddFilter(lambda y: y*vRange + vMin)

    #---------------------------------------------------

    def AddCNN(self, winSize, stepSize=1, filters=1, activation=None):
        with tf.name_scope('CNN'):
            self.top = tf.layers.conv2d(
                inputs=tf.reshape(self.top, [-1, self.top.shape[1], 1, 1]), 
                filters=filters, 
                kernel_size=[winSize, 1], 
                strides=[stepSize,1], 
                padding='valid', 
                use_bias=False,
                activation=activation,
                )
            #self.top = tf.transpose(self.top, [0,3,2,1]) # arrange filters' output into different rows
            self.top = tf.reshape(self.top, [-1, self.top.shape[1] * self.top.shape[3]], name='cnn')
    
    def AddDCN(self, filters, winSize, strides=1, activation=None, reuse=False):
        with tf.name_scope('DCN'):
            tDim = self.TopDim()
            self.top = tf.layers.conv2d_transpose(
                inputs=tf.reshape(self.top, [-1,tDim,1,1]),
                filters=filters,
                kernel_size=[winSize,1],
                strides=[strides, 1],
                padding='same',
                activation=activation,    
                use_bias=False,
                reuse=reuse,
            )
            self.top = tf.reshape(self.top, [-1, filters*tDim*strides], name='dcn')        

    def AddLayers(self, layerDims, activation=tf.nn.leaky_relu, addBias=True, nameScope='Layer'):
        r"""Add one or more layers to the current model
        Args:
          layerDims: an integer or a list of integers
            The dimensions of to be added layers.
          activation: An activation function
            The activation function for the nodes.
          addBias: boolean value
            To flag to add bias to each node.
          nameScope: a string.
            The name scope of new layers.        
        """
        if type(layerDims) == int: 
            layerDims = [ layerDims ]        
        for dim in layerDims:
            with tf.name_scope(nameScope):
                self.AddMatrixLayer(dim)
                if addBias: self.AddBias()
                if activation != None: self.AddFilter(activation)

    def AddLanes(self, laneCfg, activation=tf.nn.leaky_relu, lastActivation=None, addBias=False):
        lanes = []
        initTop = self.top
        for laneIdx, laneDims in enumerate(laneCfg):
            self.top = initTop
            with tf.name_scope('Lane'+str(laneIdx)):
                if lastActivation is None:
                    self.AddLayers(laneDims, activation, addBias)
                else:
                    if len(laneDims)>=2:
                        self.AddLayers(laneDims[:-1], activation, addBias)
                    self.AddLayers(laneDims[-1], lastActivation, addBias)
            lanes.append(self.Output())
        with tf.name_scope('LaneEnd'):
            self.top = tf.concat(lanes, axis=1)

    def AddLanes2(self, laneCfg, activation=tf.nn.leaky_relu, lastActivation=None, addBias=False):
        lanes = []
        initTop = self.top
        for laneIdx, laneDims in enumerate(laneCfg):
            self.top = initTop
            with tf.name_scope('Lane'+str(laneIdx)):
                if laneIdx==0:
                    self.AddLayers(laneDims, tf.nn.softmax, addBias)
                elif lastActivation is None:
                    self.AddLayers(laneDims, activation, addBias)
                else:
                    if len(laneDims)>=2:
                        self.AddLayers(laneDims[:-1], activation, addBias)
                    self.AddLayers(laneDims[-1], lastActivation, addBias)
            lanes.append(self.Output())
        with tf.name_scope('LaneEnd'):
            self.top = tf.concat(lanes, axis=1)

    def AddAugment(self, aDim=0, mDim=0, addBias=False):
        r""" Append an augmentation variable to the model tree.
        """
        topDim = self.TopDim()
        augDim = aDim + topDim * mDim
        if addBias: 
            if topDim > 0:
                augDim += mDim
        if augDim == 0:
            return None
        with tf.name_scope('AugLayer'):
            augVar = tf.Variable(tf.zeros([augDim], dtype=tf.float32), trainable=True, name='Augment')
            if self.top is None:
                # special case: we set the additive augment as top tensor
                self.top = tf.reshape(augVar, [1, -1], name='AugLayer')
                return augVar
            newNodes = [self.top]
            if aDim > 0: 
                # raise the augment dimension to that of the input tensor.
                newNodes.append( tf.add(self.top[:,:1]*0, augVar[:aDim], name='AugAdd') )
            if mDim > 0: 
                augMatrix = tf.reshape(augVar[aDim:(aDim+topDim*mDim)], [topDim, mDim], name='AugMatrix')
                mOutNodes = tf.matmul(self.top, augMatrix, name='AugMatMul')
                if addBias:
                    bias = tf.slice(augVar, [aDim+topDim*mDim], [mDim], name='AugBias')
                    mOutNodes = tf.add(mOutNodes, bias, name='AugMatBias')
                newNodes.append(mOutNodes)
            self.top = tf.concat(newNodes, axis=1, name='AugLayer')
        return augVar

    def AddAugment2(self, dim0, dim1):
        r""" Set two augmentation variables at the top of the model tree.
        """
        v0 = tf.Variable(tf.zeros([dim0]))
        v1 = tf.Variable(tf.zeros([dim1]))    
        self.top = tf.reshape(tf.concat([v0, v1], axis=0), [1, -1])
        return [v0, v1]

    def AddAugmentIndexed(self, augLen, augDim, idxVarName='AugX', augVarName='AugRep', binding=1):
        r"""  Extend the current top tensor with an indexed augment.
        Args:
            augLen: the number of different indexes, e.g. length of the augmentation tensor table.
            augDim: the dimension of the augmentation vectors.
            idxVarName: the name of the placehold to feed input indexes
            augVarName: the name of the augmentation tensor table
            binding: how the augmentation tensors binds to the network. 0: not bound; 
                1: bind by concatentation; 2: bind by addition; 3: bind by elementwise multiplication.
        """
        augVar = tf1.placeholder(tf.int32, shape=[None], name=idxVarName)
        R = tf.Variable(np.random.uniform(0, 0.1, [augLen, augDim]).astype(np.float32), name=augVarName)
        augTensor =  tf.gather(R, augVar)
        if binding == 1:
            self.top = tf.concat([self.top, augTensor], axis=1)
        elif binding == 2:
            if augDim == self.TopDim():
                self.top = self.top + augTensor
            elif augDim < self.TopDim():
                self.top = tf.concat([self.top[:, :augDim] + augTensor, self.top[:, augDim:]], axis=1)
            else:
                print('Error: repDim parameter too large')
        elif binding == 3:
            if augDim == self.TopDim():
                self.top = self.top * augTensor
            elif augDim < self.TopDim():
                self.top = tf.concat([self.top[:, :augDim] * augTensor, self.top[:, augDim:]], axis=1)
            else:
                print('Error: augDim parameter too large')
        return augVar, augTensor

    #============================================================================================

    def Input(self):
        return self.inputHod

    def Output(self):
        return self.top

    def TopDim(self):
        return int(self.top.shape[1]) if self.top is not None else 0

    def Label(self):
        return self.outputHod

    def LabelDim(self):
        return int(self.outputHod.shape[1])

    def SetAdamOptimizer(self, epochs, dataSize, varList=None, targetName='TrainingTarget'):
        r"""Set the Adam optimizer
        Args:
          epochs: the number of training opochs
          dataSize: the total number of training samples.
          varList: optional list of variables to be trained.
        """
        assert self.cost is not None,  'SetAdamOptimizer: No cost function defined!'
        assert (epochs * dataSize) != 0
        assert self.batchSize != 0
        assert self.r0 != 0
        assert self.globalStep != None

        coolingPeriod = tf.Variable(int(0.05 * epochs * dataSize / self.batchSize), trainable=False, name='coolingPeriod')
        initLearningRate = tf.Variable(self.r0, trainable=False, name='initLearningRate', dtype=tf.float32)
        learningDecay = tf.Variable(self.decay, trainable=False, name='learningDecay', dtype=tf.float32)
        self.learningRate = tf1.train.exponential_decay(initLearningRate, 
            self.globalStep, coolingPeriod, learningDecay, staircase=False, name='LearningRate')
        self.optimizer = tf1.train.AdamOptimizer(self.learningRate, name=targetName)        
        self.trainTarget = self.optimizer.minimize(self.cost, global_step=self.globalStep, var_list=varList)
        return self.trainTarget

    def AdjustAdamOptimizer(self, epochs, dataSize, r0=-1.0, decay=-1.0):
        r"""Adjust the expontential learning rate decay speed for changed data set size.
        This method should be called after all variables have been initialized.
        Args:
          epochs: the number of training opochs
          dataSize: the total number of training samples.
        """
        self.SetVar(self.GetVar('coolingPeriod:0'), int(0.05 * epochs * dataSize / self.batchSize))
        if r0 >=0:
            self.SetLearningRate(r0)
        if decay>=0:
            self.SetLearningDecay(decay)

    def SetAdamOptimizer2(self, totalBatchs, finalLearningRate=0.0003):
        self.learningRate = tf.train.exponential_decay(self.r0, 
            self.globalStep, totalBatchs, finalLearningRate/self.r0)
        self.trainTarget = tf.train.AdamOptimizer(self.learningRate, 
            name='TrainingTarget').minimize(self.cost, global_step=self.globalStep)

    def SetSgdOptimizer(self, epochs, dataSize):
        coolingPeriod = tf.Variable(int(0.05 * epochs * dataSize / self.batchSize), trainable=False, name='coolingPeriod')
        self.learningRate = tf.train.exponential_decay(self.r0, 
            self.globalStep, coolingPeriod, self.decay, staircase=True, name='LearningRate')
        self.trainTarget = tf.train.GradientDescentOptimizer(self.learningRate, 
            name='TrainingTarget').minimize(self.cost, global_step=self.globalStep)

    def SetLearningRate(self, v):
        self.r0 = v
        varR0 = self.GetVar('initLearningRate:0', False)
        if varR0 is not None:
            self.SetVar(varR0, v)
    
    def SetLearningDecay(self, v):
        self.decay = v
        varDecay = self.GetVar('learningDecay:0', False)
        if varDecay is not None:
            self.SetVar(varDecay, v)
    #=================================================================================
    
    def GetVar(self, vName, showError=True):
        for v in tf1.global_variables():
            if v.name == vName: 
                return v
        if showError:
            print('ERROR: in GetVar() unknown name: ', vName)
        return None
    
    def SetVar(self, variable, value):
        variable.load(value, self.sess)

    def GetTensor(self, v):
        return self.sess.run(v)

    #------------------------------------------------------------------
    def GetVariable(self, vName, showError=True):
        if type(vName) is list:
            return [ self.GetVariable(v) for v in vName ]
        for v in tf1.global_variables():
            if v.name == vName: 
                return v
        try:
            v = tf1.get_default_graph().get_operation_by_name(vName)
            return v.outputs[0] if v != None else None
        except:
            if showError:
                print('ERROR: in GetVariable() unknown name: ', vName)
            return None


    def GetVariableValue(self, vName):
        v = self.GetVariable(vName) if type(vName) is str else vName
        if v != None:
            return self.sess.run(v)
        else:
            return None

    # convert a name, a variable, or an tensor variable to a tensor.
    def ToTensor(self, v):
        tName = type(v).__name__
        if tName == 'str': return self.GetVariable(v)
        return v.outputs[0] if tName == 'Operation' else v

    #============================================================================================
    def Eval2(self, Var, X):
        if type(Var) is list:
            t = []
            for v in Var:
                t0 = self.ToTensor(v)
                if t0 != None: 
                    t.append(t0)
        else:
            t = self.ToTensor(Var)
        return self.Eval0(t, X)

    def Eval0(self, Var, X):
        evalFeed = {self.inputHod:X}
        if self.keepProbVar != None: 
            evalFeed[self.keepProbVar] = 1.0
        return self.sess.run(Var, evalFeed)

    def Eval(self, X):
        return self.Eval2(self.Output(), X)

    def Validate(self, X, Y, feed=None, outVar=None):
        if feed is None: feed = {}
        if self.inputHod is not None:
            feed[self.inputHod] = X
        if self.outputHod is not None:
            feed[self.outputHod] = Y
        if self.keepProbVar != None: 
            feed[self.keepProbVar] = 1.0
        if outVar is None:
            outVar = [self.cost, self.Output()]
        return self.sess.run(outVar, feed)

    def EvalBatch(self, rowList, Var, ret):
        self.feed[self.inputHod] = np.array(rowList, dtype=np.float32)
        rowList.clear()
        if Var == None: 
            Var = self.Output()
        output = self.sess.run(Var, self.feed) 
        return np.append(ret, output, axis=0)

    # squential evaluation of large dataset.
    def EvalSeq(self, fileName, Var=None):
        if self.keepProbVar != None:
            self.feed[self.keepProbVar] = 1.0
        outDim = self.TopDim() if Var==None else Var.get_shape()[1].value
        ret = np.ndarray((0, outDim), dtype=np.float32)
        with open(fileName, 'r') as fin:
            reader = csv.reader(fin, delimiter="|")
            rowList = []
            for row in reader:
                rowList.append(row)
                if len(rowList) >= 50:
                    ret = self.EvalBatch(rowList, Var, ret)
            if len(rowList) > 0:
                ret = self.EvalBatch(rowList, Var, ret)
        if self.keepProbVar != None:
            del self.feed[self.keepProbVar]
        return ret

    def Save(self, modelName, saveAux=True):
        if modelName.startswith('<NotSave>'):
            return

        self.modelInfo.update({'ModelName':modelName, 'BatchSize':self.batchSize, 'r0':self.r0, 'Decay':self.decay, 'LastError':self.lastError})
        tf.constant(str(self.modelInfo), name='ModelInfo')

        # The node 'CostTensor' and 'OutputTensor' exist already if the model is loaded.
        if self.cost != None:
            if self.cost.name != 'CostTensor':
                tf.identity(self.cost, name='CostTensor')
                tf.identity(self.Output(), name='OutputTensor')
        else:
            tf.identity(self.Output(), name='OutputTensor')

        saver = tf1.train.Saver()
        saver.save(self.sess, './' + modelName, None, modelName+'.chk')
        os.unlink(modelName+'.chk')
        
        if saveAux:
            # Notify the server to save its part, i.e. *.md file.
            self.log.SaveModel(self.job)

    def ShowGraph(self):
        if os.path.isdir('tf_log'):
            os.system('rmdir/s /q tf_log')
        tf.summary.FileWriter(logdir="tf_log", graph=self.sess.graph)
        #hostname = os.environ['USERDOMAIN']
        hostname = "localhost"
        os.system('start chrome http://%s:6006'%hostname)
        os.system('start tensorboard --logdir="./tf_log" --port=6006')

    def ListVariables(self):
        print('---- Global Variables ----')
        for k, v in enumerate(tf.global_variables()):
            print('%d: %s'%(k,  v.name))


    #===============================================================================

    def EpochCall(self, ep, ds, logLevel, refreshFreq, epochs):
        print('%d: %.6g'%(ep, self.lastError))
        if logLevel>=1:
            self.log.ReportCost(ep, self.lastError, self.job)
        if ep == epochs:
            self.trainingTime = time.time()-self.startTime
            print('Training completed! Training time: %.2f'%self.trainingTime)

        if logLevel >= 3:
            try:
                if hasattr(ds, "V") and (ds.V is not None): 
                    np.savetxt("validationOut.csv", self.Eval(ds.V), delimiter='|', fmt='%.6f')
                if hasattr(ds, "X") and (ds.X is not None): 
                    self.ShowTensorMap(self.Eval(ds.X))
                if logLevel >=4:
                    self.log.ExtHistogram(2, self.log.GetPredInfo()[0], self.job)
            except IOError as e:
                print(e)

    def Train1(self, ds, epochs, logLevel, refreshFreq, epCall=None):
        r"""Start the training process (obsoluted)
        """
        if (logLevel >= 4) and (self.job==0): 
            self.log.CfgHistogram(2, 'L1 History')
        if hasattr(ds, 'V') and (ds.V is not None) and (self.job==0):
            self.log.CfgHistogram(1, 'Validation Error')
        if not self.initialized:
            self.InitAllVariables()

        self.startTime = time.time()
        for self.lastEpoch in range(1, epochs+1):
            error = 0.0
            ds.InitEpoch(self.batchSize, self.randomizingData)
            while ds.HasMoreData():
                bX, bY = ds.NextBatch()
                if (bX is not None) and (self.inputHod is not None):
                    self.feed[self.inputHod] = bX
                if (bY is not None) and (self.outputHod is not None):
                    self.feed[self.outputHod] = bY
                _, err = self.sess.run([self.trainTarget, self.cost], self.feed) 
                error += err
            if math.isnan(error):
                print("Data corruption")
                quit()
            self.lastError = math.sqrt(error)
            if refreshFreq > 0:
                if (self.lastEpoch % refreshFreq) == 0: 
                    if (epCall == None) or epCall(self.lastEpoch):
                        self.EpochCall(self.lastEpoch, ds, logLevel, refreshFreq, epochs)
        self.trainingTime = time.time() - self.startTime                        

    def Train(self, ds, epochs, logLevel=1, refreshFreq=5, epCall=None):
        r"""Start the training process
        Args:
          ds: A dataset object that exposes InitEpoch and BeginStep method.
          epochs: The number of training epochs.
          logLevel: The logging level. 0: for no logs; 3 for maixmal log information.
          refreshFreq: frequency in epochs to report training status.
          epCall: custom status reporter.
        """
        if hasattr(ds, 'NextBatch'):
            return self.Train1(ds, epochs, logLevel, refreshFreq, epCall)

        if (logLevel >= 4) and (self.job==0): 
            self.log.CfgHistogram(2, 'L1 History')
        if hasattr(ds, 'V') and (ds.V is not None) and (self.job==0):
            self.log.CfgHistogram(1, 'Validation Error')
        if not self.initialized: 
            self.InitAllVariables()
        doEndStep = getattr(ds, 'EndStep', lambda:None)

        self.startTime = time.time()
        for self.lastEpoch in range(1, epochs+1):
            epErr = 0.0
            ds.InitEpoch(self)

            while ds.BeginStep():
                _, err = self.sess.run([self.trainTarget, self.cost], self.feed) 
                epErr += err
                doEndStep()

            if math.isnan(epErr):
                print("Data corruption")
                quit()
            self.lastError = math.sqrt(epErr)
            if (refreshFreq > 0) and ((self.lastEpoch%refreshFreq) == 0): 
                if (epCall == None) or epCall(self.lastEpoch):
                    self.EpochCall(self.lastEpoch, ds, logLevel, refreshFreq, epochs)
        self.trainingTime = time.time() - self.startTime

    def InitLogger(self):
        if self.log == None:
            self.log = Logger(vmHost=self.vmHost)
        return self.log

    def ShowTensorMap(self, outTensor, tmout=20):
        skt = self.log.skt
        skt.settimeout(tmout)        
        skt.sendall(bytearray(struct.pack('ii', self.log.CMD_SH_MAP, self.job)))
        buf = skt.recv(24)
        skt.settimeout(None)
        if ( len(buf) >= 4):
            resp =  struct.unpack_from('<i', buf)
            if resp[0] != self.log.CMD_OK:
                return False
        tcpCnt = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        tcpCnt.connect((self.log.vmHost, self.log.port))
        tcpCnt.settimeout(tmout)        
        self.log.WriteMatrix(tcpCnt, outTensor)
        tcpCnt.close()
        return True

    #=================================================================================
    @staticmethod
    def NewHolder(shape, name):
        return tf1.placeholder(tf.float32, shape=shape, name=name)

    @staticmethod
    def SoftmaxCost(output, label):
        return tf.reduce_sum(tf.nn.softmax_cross_entropy_with_logits_v2(logits=output, labels=label))

    @staticmethod
    def SquaredCost(output, label):
        return tf.reduce_sum(tf.square(output-label))

    @staticmethod
    def MixedCost(output, label, mapDim, cBias=0.1):
        costClr = ModelBuilder.SoftmaxCost(output[:, mapDim:], label[:, mapDim:])
        costPos = ModelBuilder.SquaredCost(output[:, :mapDim], label[:, :mapDim])
        return cBias * costClr + (int(output.shape[1]) - mapDim) * costPos
