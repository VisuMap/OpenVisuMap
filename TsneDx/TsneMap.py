import sys,clr
import numpy as np
import pprint
sys.path.append('C:\\work\\test\\TsneDx\\bin\\Debug\\netcoreapp2.1')
td = clr.AddReference('TsneDx')
for md in td.Modules:
    print(md.FullyQualifiedName)
    print(dir(md))
pprint.pprint(sys.modules)
import TsneDx
print ("Hi")