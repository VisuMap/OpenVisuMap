import h5py
import numpy
f = h5py.File('GSM4339771_C143_filtered_feature_bc_matrix.h5', 'r')
d = f['matrix']
d.visit(lambda name: print(d[name]))
for key in ['shape', 'indptr', 'barcodes', 'features/id']:
    print(key, ': ', d[key].value)

