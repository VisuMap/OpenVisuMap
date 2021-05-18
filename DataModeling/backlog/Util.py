import numpy as np
import math, random

def MakePairs(nt, duplicate):
    Rows = np.shape(nt)[0]
    Cols = np.shape(nt)[1]
    pads = Rows % 2
    if pads != 0:
        nt = np.append(nt, [nt[Rows-1]], axis=0)
        Rows += 1
    Rows = int(Rows/2)
    Cols = int(2*Cols)


    if duplicate:
        nt2 = np.copy(nt);
        for i in range(Rows):
            nt2[[2*i, 2*i+1]] = nt2[[2*i+1,2*i]]
        nt2 = np.reshape(nt2, (Rows, Cols))
        nt = np.reshape(nt, (Rows, Cols))
        nt = np.concatenate((nt, nt2))
    else:
        nt = np.reshape(nt, (Rows, Cols))

    return nt

#=========================================================================

def Curvature2(P):
    P *= 1000
    M = np.shape(P)[0]
    L = 20
    Sz = 1
    cList = []
    for i in range(0, M-2*L-1, Sz):
        p0 = P[i+L] - P[i]
        p1 = P[i+2*L] - P[i+L]
        p2 = p0 + p1
        n0 = np.dot(p0, p0)
        n1 = np.dot(p1, p1)
        n2 = np.dot(p2, p2)
        cv = 0
        if n0*n1*n2 > 0 :
            dot = np.dot(p0, p1)
            sinA2 = 1 - (dot*dot)/(n0 * n1);
            if sinA2 > 0: cv = 4*sinA2/n2
        cList.append(cv)
    return cList 

def CurvatureSquared(P):
    return np.sum(Curvature2(P))/np.shape(P)[0]

def CurveLength2(P):
    sum = 0
    M = np.shape(P)[0]
    for i in range(1, M):
        sum += np.linalg.norm(P[i-1] - P[i])
    return sum * sum

def MovingAverage(a, w):
    b = np.zeros(len(a)-w+1)
    c = 1.0/w
    s = np.sum(a[0:w])
    b[0] = c*s
    for k in range(1, len(a)-w+1):
       s += a[w+k-1] - a[k-1]
       b[k] = c*s
    return b

def Diff(a):
    b = np.zeros(len(a)-1)
    for k in range(0, len(a)-1):
        b[k] = a[k+1] - a[k]
    return b

#=========================================================================

def StripArea(P):
    sum = 0;
    P *= 1000
    M = np.shape(P)[0]
    for i in range(2, M):
        p = P[i] - P[i-1]
        q = P[i-2] - P[i-1]
        sum += np.linalg.norm(np.cross(p,q))
    return sum

def CheckOverfitting(sess, var_x, X, keep_prob, y, mapDim, log):
    N = np.shape(X)[0]
    x_dim = np.shape(X)[1]
    random.seed(1111)
    M = 25    # interpolation steps.
    CNT = 100 # repeats count
    XX = np.zeros((M, x_dim))
    stripSize = 0
    for cnt in range(CNT):
        X1 = X[random.randint(0,N-1)]
        X2 = X[random.randint(0,N-1)]
        for i in range(M): 
            f = (i+1.0)/(M+1)
            XX[i] = f*X1 + (1-f)*X2
        YY = sess.run(y, feed_dict={var_x:XX, keep_prob:1.0})
        YY = YY[:, 0:mapDim]
        stripSize += StripArea(YY)
    stripSize /= CNT;
    log.ReportMsg( "Overfitting: " + "{0:.2f}".format(stripSize) )

