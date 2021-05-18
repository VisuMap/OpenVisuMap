del *.zip
copy bin\Release\CustomGlyphSets.dll .
zip CustomGlyphSets.zip CustomGlyphSets.dll *.js RedGreenBinary/*.* Phase8/*.* Phase16/*.* AlphaParticles/*.* ColorWheel128/*.* 
del CustomGlyphSets.dll
rem uploadplugin CustomGlyphSets.zip
