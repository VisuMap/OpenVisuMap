# Script to apply UMAP mapping algorithm on selected data.
import umap, time, ModelUtil

mapDim = 3
log = ModelUtil.Logger()
print('Loading data from VisuMap...')
ds = log.LoadTable(dsName='@')
print("Loaded table: ", ds.shape)

nn, md, lc = 15, 0.5, 1.0
print('Fitting data...')
map = umap.UMAP(n_neighbors=nn, 
                min_dist=md, 
                local_connectivity=lc, 
                n_components=mapDim).fit_transform(ds)

if mapDim >= 3:
    log.ShowMatrix(map, view=2, 
        title='Neighbors: %d, MinDist: %g, Connectivity: %g'%(nn, md, lc))
else:
    log.ShowMatrix(map, view=12, 
        title='Neighbors: %d, MinDist: %g, Connectivity: %g'%(nn, md, lc))
    log.RunScript('pp.FitToWindow()')