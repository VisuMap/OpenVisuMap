from ModelUtil import ModelDataset
import numpy as np
import tensorflow as tf
import math, random, time
from random import shuffle

def Rotate(m, a, rAxis=2):
    c = np.median(m, axis=0)
    cosA = math.cos(a)
    sinA = math.sin(a)
    if m.shape[1] == 3:
        if rAxis==2:
            R = np.array([[cosA, -sinA, 0], [sinA, cosA, 0], [0,0,1]], dtype=np.float32 )
        elif rAxis==1:
            R = np.array([[cosA, 0, -sinA], [0,1,0], [sinA, 0, cosA]], dtype=np.float32 )
        else:
            R = np.array([[1,0,0], [0, cosA, -sinA], [0, sinA, cosA]], dtype=np.float32 )
    else:
        R = np.array([ [cosA, -sinA], [sinA, cosA] ], dtype=np.float32 )
    R = np.transpose(R)
    return np.matmul(m-c, R)+c

def Scale(m, s):
    c = np.mean(m, axis=0)
    return s * (m - c) + c

def MergeDatasets(dsList):
    inAll = np.concatenate([ d[0] for d in dsList ])
    outAll = np.concatenate([ d[1] for d in dsList ])
    return [(inAll, outAll)]

def MergeDatasetList(dsList, ds):
    ds.X = np.concatenate([ d[0] for d in dsList ])
    ds.Y = np.concatenate([ d[1] for d in dsList ])
    ds.UpdateAux()
    return ds

def aRange(K):
    return [2*math.pi*k/K for k in range(K)]

def RotateX(X, Aug, K):
    X = Scale(X, 0.3)
    dsList = []
    N, yDim = X.shape[0], Aug.shape[1]
    for k, a in enumerate(aRange(K)):
        XX = Rotate(X, a)
        XX += 0.30 * np.array([math.cos(a), math.sin(a), 0], dtype=np.float32)
        YY = np.full((N, yDim), Aug[k,:], dtype=np.float32)
        dsList.append((XX,YY))
    return dsList

def RandomBall(D):
    c = np.median(D, axis=0)

    a = random.uniform(0, 2*math.pi)
    cosA = math.cos(a)
    sinA = math.sin(a)
    R0 = np.array([[cosA, -sinA, 0], [sinA, cosA, 0], [0,0,1]], dtype=np.float32 )

    a = random.uniform(0, 2*math.pi)
    cosA = math.cos(a)
    sinA = math.sin(a)
    R1= np.array([[cosA, 0, -sinA], [0,1,0], [sinA, 0, cosA]], dtype=np.float32 )

    a = random.uniform(0, 2*math.pi)
    cosA = math.cos(a)
    sinA = math.sin(a)
    R2 = np.array([[1,0,0], [0, cosA, -sinA], [0, sinA, cosA]], dtype=np.float32 )

    R = np.transpose(np.matmul(np.matmul(R0, R1), R2))

    s = random.uniform(0.1, 1.2)
    X = s*np.matmul(D-c, R)+c
    X = 0.3*X+np.random.uniform(0,0.7,[3])
    return X

def Trans0(D, K, tType=0):
    if tType == 16:
        dsList = []
        L = len(D)//2
        D1, D2 = D[:L], D[L:]
        c = np.mean(D2, axis=0)
        K3 = K//3
        for a in aRange(K3):
            X = 0.25*(D2-c) + c
            X = Rotate(X, a, rAxis=2)
            dsList.append( np.concatenate((D1, X), axis=0) )
        for k in range(K3):
            s =  1.25*k/K3 + 0.25
            X = s*(D2-c) + c 
            dsList.append( np.concatenate((D1, X), axis=0) )
        for a in aRange(K3):
            X = 1.25*(D2-c) + c
            X = Rotate(X, a, rAxis=1)
            dsList.append( np.concatenate((D1, X), axis=0) )
        return dsList

    # scaling co-centered ball.
    if tType == 15:  
        dsList = []
        L = len(D)//2
        D1, D2 = D[:L], D[L:]
        for a in aRange(K):
            X = Rotate(D1, a)
            dsList.append( np.concatenate((D2, X), axis=0) )
        return dsList

    # scaling co-centered ball.
    if tType == 14:  
        dsList = []
        L = len(D)//2
        D1, D2 = D[:L], D[L:]
        c = np.mean(D1, axis=0)
        for k in range(K):
            s = 1.0 + 0.75*k/K 
            X = s * (D1 - c) + c
            dsList.append( np.concatenate((D2, X), axis=0) )
        return dsList

    # shift the left ball
    if tType == 13:  
        dsList = []
        L = len(D)//2
        D1, D2 = D[:L], D[L:]
        c12 = np.mean(D2, axis=0) - np.mean(D1, axis=0)
        for k in range(K):
            X = D2 - k/K*c12
            dsList.append( np.concatenate((D1, X), axis=0) )
        return dsList

    # scale the left ball.
    if tType == 12:  
        dsList = []
        L = len(D)//2
        D1, D2 = D[:L], D[L:]
        c = np.mean(D2, axis=0)
        for k in range(K):
            s =  1.25 * k/K + 0.25
            X = s*(D2-c) + c 
            dsList.append( np.concatenate((D1, X), axis=0) )
        return dsList

    # rotate a single for 0.65 * 2*pi angle
    if tType == 11:
        dsList = []
        D = Scale(D, 0.3)
        for a in aRange(K):
            a *= 0.65
            X = Rotate(D, a)
            X += 0.30 * np.array([math.cos(a), math.sin(a), 0], dtype=np.float32)
            dsList.append(X)
        return dsList

    # Rotate the left ball along given axis
    if tType >= 8:
        dsList = []
        L = len(D)//2
        D1, D2 = D[:L], D[L:]
        axis = tType - 8
        for a in aRange(K):
            rd = Rotate(D2, a, rAxis=axis)
            dsList.append( np.concatenate((D1, rd), axis=0) )
        return dsList

    # Random shift
    if tType == 7:
        dsList = []
        for k in range(K):
            dsList.append(RandomBall(D))
        return dsList

    # simple shifting
    if tType == 6:
        return [ 0.3*D+3*[0.7*k/K] for k in range(K) ]

    if tType>=3:
        return [ Rotate(D, a, rAxis=tType-3) for a in aRange(K) ]

    dsList = []
    D = Scale(D, 0.3)
    for a in aRange(K):
        if tType==0:
            X = np.copy(D)
        elif tType==1:
            X = Rotate(D, a)
        else:
            c = np.median(D, axis=0)
            X = np.copy(D) - c
            X[:,0] *= abs(math.cos(a))*0.9 + 0.1
            X += c
        X += 0.30 * np.array([math.cos(a), math.sin(a), 0], dtype=np.float32)
        dsList.append(X)
    return dsList
    
def TransY(X, Y, K, tType=0):
    return [(X, Y) for Y in Trans0(Y, K, tType)]

def ShowDatasetList(dsList, log):
    print('Datasets: ', len(dsList))
    for k, d in enumerate(dsList): 
        print(k, ': ', d[0].shape, '->', d[1].shape)
        log.ShowMatrix(d[1]*750.0, view=13, access='r', title='k: %d'%k,)
        time.sleep(0.5)

def Make3DReference(rows, freq):
    R = np.empty([rows, 3], dtype=np.float32)
    for i in range(rows):
        t = i/rows
        a = freq * t * 2 * math.pi
        R[i] = [math.sin(a), math.cos(a), 2*t - 1.0]
    return R

# Convert a single data table to list of in-out dataset list. Each row will be 
# reshaped to a dataset table.
def TableToList(X, columns, freq):
    rows = X.shape[1]//columns
    Z = np.reshape(X, [-1, rows, columns])
    #R = np.mean(Z, axis=0)
    R = Make3DReference(rows, freq)
    return [(R, Z[i]) for i in range(X.shape[0])]

def RowToTable(R, columns):
    rows = math.ceil(R.shape[0]/columns)
    pads = rows * columns - R.shape[0]
    if pads > 0:
        R = np.pad(R, (0, pads), 'constant')
    return np.reshape(R, [rows, columns])

def RefTable2List(R, X, columns):
    rows = R.shape[0]
    pads = rows*columns - X.shape[1]
    if pads > 0:
        X = np.pad(X, ((0, 0),(0,pads)), 'constant')        
    X = np.reshape(X, [-1, rows, columns])
    return [(R, X[i]) for i in range(X.shape[0])]

#=================================================================

#Show datasets as single table with one row for each dataset.
def ShowDatasetList2(dsList, log):
    dss = np.array([d[1] for d in dsList])
    log.ShowMatrix(np.reshape(dss, [-1, dsList[0][1].size]), view=8)
    log.RunScript('pp.Reset().Start()')

def LoadMapList(mapList, log):
    X, Y = log.OpenDataset(mapList[0], target='Shp', dataGroup=3)
    dsList = [(X, Y)]
    for map in mapList[1:]:
        dsList.append( (X, log.OpenDataset(map, target='Shp', dataGroup=2)[1]) )
    return X, Y, dsList

def LoadTransMap(K, typeList, log):
    X, Y = log.OpenDataset('', target='Shp', dataGroup=3, tmout=60)
    dsList = sum([TransY(X, Y, K, t) for t in typeList], [] ) 
    return X, Y, dsList

#=================================================================
# class to feed list of datasets to tensor graphs.
#=================================================================
class DatasetList(ModelDataset):
    dsList = None
    dsIdx = 0      # index of current dataset
    totalN = 0     # total number of sample rows.
    totalIdx = 0   # index of current row in current epoch
    _randomizing = False
    md = None

    def __init__ (self, dsList):
        self.dsList = dsList
        self.totalN = sum([ds[0].shape[0] for ds in dsList]) 
        self.N = self.totalN      # The SetAdamOptimizer() will use this number.
        self.xDim = dsList[0][0].shape[1]
        self.yDim = dsList[0][1].shape[1]

    def InitEpoch(self, md):
        self.totalIdx = 0
        self.dsIdx = 0
        self.md = md
        self._randomizing = md.randomizingData
        if self._randomizing: 
            shuffle(self.dsList)
        self.X, self.Y = self.dsList[self.dsIdx]
        self.idxMap = None
        super(DatasetList, self).InitEpoch(md)

    def BeginStep(self):
        if self.totalIdx >= self.totalN:
            return False
        if not super(DatasetList, self).BeginStep():
            # move to the next dataset
            self.dsIdx += 1
            self.X, self.Y = self.dsList[self.dsIdx]
            self.idxMap = None
            super(DatasetList, self).InitEpoch(self.md)
            super(DatasetList, self).BeginStep()
        self.totalIdx += self.md.feed[self.md.inputHod].shape[0]
        return True

#==================================================================
# Base class for AugDataset* classes
#===================================================================
class AugDataset0:
    aug = None    # the values of augmented variables for each dataset.
    augTensor = None # the tensor variable for the augented variables.
    augDim = 0
    augVar1 = None
    augVar2 = None
    var1Dim = 0
    var2Dim = 0
    md = None

    def __init__(self):
        pass

    def InitAug(self, N, augVar):
        if augVar == None: 
            return
        if isinstance(augVar, tf.Variable):
            self.augVar1 = augVar
        else:
            augVar = [x for x in augVar if x is not None]
            if len(augVar) >= 1:
                self.augVar1 = augVar[0]
            if len(augVar) >= 2:
                self.augVar2 = augVar[1]
            elif len(augVar) >= 3:
                print('More than two augment variable not supported')
                return
        if self.augVar1 == None:
            return
        
        if self.augVar2 == None:
            self.augTensor = self.augVar1
        else:
            self.augTensor = tf.concat([self.augVar1, self.augVar2], axis=0)
        
        if self.augVar1 is not None:
            self.var1Dim = int(self.augVar1.shape[0])
        if self.augVar2 is not None:
            self.var2Dim = int(self.augVar2.shape[0])
        self.augDim = self.var1Dim + self.var2Dim

        self.aug = np.zeros([N, self.augDim], np.float32)
    
    def PushAug(self, augIdx):
        if self.augVar1 is not None:
            self.augVar1.load(self.aug[augIdx, :self.var1Dim], self.md.sess)
        if self.augVar2 is not None:
            self.augVar2.load(self.aug[augIdx, self.var1Dim:], self.md.sess)

    def PushFeed(self, augIdx):
        if self.var2Dim == 0:
            return {self.augVar1:self.aug[augIdx]}
        else:
            return {self.augVar1:self.aug[augIdx,:self.var1Dim], self.augVar2:self.aug[augIdx, self.var1Dim:] }

    def PushFeed0(self, augVector):
        if self.var2Dim == 0:
            return {self.augVar1:augVector}
        else:
            return {self.augVar1:augVector[:self.var1Dim], self.augVar2:augVector[self.var1Dim:] }

    def PullAug(self, augIdx):
        self.aug[augIdx] = self.md.sess.run(self.augTensor)

    def SaveAugment(self, modelName):  
        if (self.aug is None) or  modelName.startswith('<NotSave>'):
            return

        np.savetxt(modelName + '.aug', self.aug, delimiter='|', fmt='%.8f')
        var2Name = self.augVar2.name if (self.augVar2 != None) else ''
        self.md.modelInfo.update({'AugVar1':self.augVar1.name, 'AugVar2':var2Name, 
                                  'Var1Dim':self.var1Dim, 'Var2Dim':self.var2Dim,
                                  'AugDim':self.augDim, 'AugLen':self.aug.shape[0]})

#==================================================================
# Class to augment list of dataset with learnable input variables
#===================================================================

class AugDataset(AugDataset0):
    r"""
      Prepare a single dataset for augmented learning:
      The a whole row will be returned in each batch as label; no input 
      data will be returned; 
    """
    Y = None
    Columns = 0
    rowIdx = 0
    N = 0
    epochs = 0

    def __init__ (self, ds, md, augVar=None):
        self.N = ds.Y.shape[0]
        self.Columns = ds.Y.shape[1]
        self.Y = np.reshape(ds.Y, [self.N, 1, self.Columns])
        self.rowMap = np.arange(self.N)
        self.md = md
        md.batchSize = 1
        self.InitAug(self.N, augVar)

    def InitEpoch(self, batchSize, randomizing):
        if self.epochs > 0:
            # save the augument for the last row of last epoch.
            self.PullAug(self.rowMap[self.N-1])
        np.random.shuffle(self.rowMap)
        self.epochs += 1
        self.rowIdx = 0
        
    def Eval(self):
        m = np.zeros([self.N, self.Columns], dtype=np.float32)
        v = self.md.GetVariable('PhenoMap')
        for i in range(self.N):
            m[i] = self.md.sess.run(v,  self.PushFeed(i))[0]
        return m

    def HasMoreData(self):
        return self.rowIdx < self.N

    def NextBatch(self):
        rIdx = self.rowMap[self.rowIdx]
        if self.rowIdx >= 1:
            self.PullAug(self.rowMap[self.rowIdx-1])
        self.PushAug(rIdx)
        self.rowIdx += 1
        return None, self.Y[rIdx]

#==================================================================
# Prepare a single dataset for augmented learning with 1-*-1 architecture.
#===================================================================

class AugDataset2(AugDataset0):
    r"""
      Prepare a single dataset for augmented learning with 1-*-1 architecture. Each batch contains
      a randomly selected subset of columns of input data and label data reshaped as a list of [1,1]
      shaped pairs.
    """
    X = None
    Y = None
    Columns = 0
    rowIdx = 0
    colIdx = 0
    N = 0
    epochs = 0

    def __init__ (self, ds, md, augVar=None, freq=5.5):
        self.N = ds.X.shape[0]
        self.Columns = ds.X.shape[1]
        self.Y = np.reshape(ds.X, [-1, self.Columns, 1])
        self.X = Make3DReference(self.Columns, freq)
        self.colMap = np.arange(self.Columns)
        self.rowMap = np.arange(self.N)
        self.md = md
        self.epochs = 0
        self.InitAug(self.N, augVar)

    def InitEpoch(self, batchSize, randomizing):
        if self.epochs > 0:
            # save the last aug vector of the previouse epochs
            self.PullAug(self.rowMap[self.N-1])
        np.random.shuffle(self.rowMap)
        np.random.shuffle(self.colMap)
        self.epochs += 1
        self.rowIdx = 0
        self.colIdx = 0
        self.curY = self.Y[self.rowMap[0]]
        self.PushAug(self.rowMap[0])
        
    def Eval(self):        
        pred = np.empty([self.N, self.Columns], dtype=np.float32)
        vMap = self.md.GetVariable('PhenoMap')
        feed= {self.md.inputHod : self.X }
        for k in range(self.N):
            feed.update( self.PushFeed(k) )
            pred[k] = self.md.sess.run(vMap,  feed).flatten()
        return pred

    def HasMoreData(self):
        if self.colIdx >= self.Columns:
            return (self.rowIdx+1) < self.N
        else:
            return self.rowIdx < self.N

    def NextBatch(self):
        if self.colIdx >= self.Columns:
            self.PullAug(self.rowMap[self.rowIdx])
            self.rowIdx += 1
            rIdx = self.rowMap[self.rowIdx]
            self.PushAug(rIdx)
            self.curY = self.Y[rIdx]
            self.colIdx = 0            
        bSize = min(self.md.batchSize, self.Columns - self.colIdx)
        idxList = self.colMap[self.colIdx:(self.colIdx+bSize)]    
        bX = np.take(self.X, idxList, 0)
        bY = np.take(self.curY, idxList, 0)        
        self.colIdx += bSize
        return bX, bY

#==================================================================
# Created a augmented dataset from a list of datasets.
#===================================================================

class AugDataset3(AugDataset0):
    X = None
    Y = None
    Ys = None # The list of Ys in all datasets.
    batchSize = 0

    dsIdx = 0     # dsMap[dsIdx] is the index of the current dataset.
    dsIdx2 = 0    # short cut for dsMap[dsIdx]
    dsMap = None  # help array to shuffle the dataset indexes. 

    cuMap = None
    cuIdx = 0
    cuN = 0
    N = 0
    epochs = 0

    def __init__ (self, dsList, md, augVar=None):
        dsN = len(dsList)
        self.Ys = [ dsList[i][1] for i in range(dsN) ]
        self.X = dsList[0][0]
        self.batchSize = md.batchSize
        self.md = md
        self.cuN = self.X.shape[0]
        self.N = dsN * self.cuN

        self.InitAug(dsN, augVar)

        self.dsMap = np.arange(dsN)
        self.cuMap = np.arange(self.cuN)
        self.epochs = 0
        # Only fetch learned aug after first dataset in first epoch:
        self.dsIdx2 = -1

    def Validate(self, cost1, cost2):
        cost = np.zeros([2], np.float32)
        K = len(self.Ys)
        for k in range(K):
            cost += self.md.Validate(self.X, self.Ys[k], self.PushFeed(k), [cost1, cost2])    
        return cost[0], cost[1]

    def Eval(self):
        K =  len(self.Ys)
        sp = self.Ys[0].shape 
        pred = np.empty([K, sp[0] * sp[1]], dtype=np.float32)
        vMap = self.md.GetVariable('PhenoMap')
        feed= {self.md.inputHod : self.X}
        for k in range(K):
            feed.update(self.PushFeed(k))
            pred[k] = self.md.sess.run(vMap,  feed).flatten()
        return pred

    def InitEpoch(self, batchSize, randomizing):
        np.random.shuffle(self.dsMap)
        self.InitDataset(0)
        self.epochs += 1

    def InitDataset(self, dsIndex):
        if self.dsIdx2 >= 0:
            self.PullAug(self.dsIdx2)
        self.dsIdx = dsIndex
        self.dsIdx2 = self.dsMap[self.dsIdx]
        self.Y = self.Ys[self.dsIdx2]
        self.PushAug(self.dsIdx2)

        self.cuN = self.X.shape[0]

        self.cuIdx = 0
        '''
        selectRatio = 0.05
        self.cuIdx = int( (1.0 - selectRatio) * self.cuN )
        self.N = int(selectRatio * self.N)
        '''

        np.random.shuffle(self.cuMap)

    def HasMoreData(self):
        return self.cuIdx < self.cuN

    def NextBatch(self):
        cuIdx2 = min(self.cuIdx+self.batchSize, self.cuN)
        batchIdx = self.cuMap[self.cuIdx : cuIdx2] 
        bX = np.take(self.X, batchIdx, 0)
        bY = np.take(self.Y, batchIdx, 0)
        self.cuIdx = cuIdx2

        if self.cuIdx == self.cuN:
            self.dsIdx += 1
            if self.dsIdx < self.dsMap.shape[0]: 
                self.InitDataset(self.dsIdx)
        return bX, bY


#==================================================================
# Created a augmented dataset from a dataset and mask for missing data.
#===================================================================
class AugDatasetMasked(AugDataset0):
    r"""
      Prepare a single dataset for augmented learning:
      The a whole row will be returned in each batch as label; input and
      output data are the masked data and the data mask (for missing values)
      respectively.
    """
    X = None  # The data mask
    Y = None  # The masked data
    Columns = 0
    rowIdx = 0
    N = 0
    epochs = 0

    def __init__ (self, ds, md, augVar=None):
        self.N = ds.Y.shape[0]
        self.Columns = ds.Y.shape[1]
        self.X = np.reshape(ds.X, [self.N, 1, self.Columns])
        self.Y = np.reshape(ds.Y, [self.N, 1, self.Columns])
        self.rowMap = np.arange(self.N)
        self.md = md
        md.batchSize = 1
        self.InitAug(self.N, augVar)

    def InitEpoch(self, batchSize, randomizing):
        if self.epochs > 0:
            # save the augument for the last row of last epoch.
            self.PullAug(self.rowMap[self.N-1])
        np.random.shuffle(self.rowMap)
        self.epochs += 1
        self.rowIdx = 0

    def HasMoreData(self):
        return self.rowIdx < self.N

    def NextBatch(self):
        rIdx = self.rowMap[self.rowIdx]
        if self.rowIdx >= 1:
            self.PullAug(self.rowMap[self.rowIdx-1])
        self.PushAug(rIdx)
        self.rowIdx += 1
        return self.X[rIdx], self.Y[rIdx]
