// File: DoPCA.js
//
// Description: Perform a PCA analysis on selected data and
// save the projection base (eigenvector) in a data table.
//
var nt = pp.GetSelectedNumberTable();
var pca = nt.ShowPcaView();
vv.SetObject("ProjectionBase", pca.EigenVectorTable().Transpose2());
