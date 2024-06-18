import DataLinkCmd as vm
import pymde

D = vm.LoadFromVisuMap('euclidean')
for n in [0, 1]:
	M = pymde.preserve_neighbors(D, embedding_dim=2, 
        	n_neighbors=50, init='random', 
	        device='cpu', verbose=True).embed().numpy()
	vm.ShowToVisuMap(M, 'PyMDE test')

