from sklearn.datasets import make_blobs
import numpy as np
X, Y = make_blobs(n_samples=10000, centers=3, n_features=3000, random_state=0)
X = X.astype(np.float32)
np.save('blob.npy', X)
