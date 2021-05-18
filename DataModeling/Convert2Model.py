import numpy as np
D = np.genfromtxt('SP500C.csv', names=True, delimiter='\t', dtype=np.float32)
columns = len(D[0])
rows = D.shape[0]
names = D.dtype.names
idxOfName = {}
for i in range(columns):
    idxOfName[names[i]] = i

cNames = []
with open('ModelA.md') as fp:
    while True:
        line = fp.readline()
        if not line: break;
        if line.startswith('InputVariables:'):
            cNames = line.split()
            break
cNames = cNames[1:]

idxOfIdx = []
matches = 0
for i in range(len(cNames)):
    nm = cNames[i]
    if nm in idxOfName:
        idxOfIdx.append(idxOfName[nm])
        matches += 1
    else:
        idxOfIdx.append(-1)

print('Column-Matches: %d out of %d'%(matches, len(cNames)))

T = np.ndarray((0, columns), dtype=np.float32)

for r in D:
    R = np.zeros((columns), dtype=np.float32)
    for i in range(columns):
        idx = idxOfIdx[i]
        if idx>=0: R[i] = r[idx]
    T = np.append(T, [R], axis=0)

print( T )
