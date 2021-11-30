//!import "AtlasHelp.js"

var featureMap = vv.AtlasManager.OpenMap('FeatureMaps', vv.Dataset.Name);

if ( featureMap == null ) {
   vv.Message('No Feature Map Found');
   vv.Return();
}

cfg.geneMap = featureMap;
cfg.cellMap = New.MapSnapshot().Show();
cfg.hm = New.HeatMap().Show();
LayoutMaps();

