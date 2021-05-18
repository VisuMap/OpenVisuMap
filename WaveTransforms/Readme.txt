WaveTransforms: A VisuMap plugin module for wave transformations
----------------------------------------------------------------

This directory contains a VisuMap plugin module to extend
VisuMap with wave alike transformation services. The plugin
support the following transformations:

  - Fourier
  - Haar
  - Walsh
  - Wavelet
  - PCA

In order to install this plugin just start VisuMap application
 and run the script SetupWaveTransform.js located in this 
directory. In order to test the plugin, load a dataset with 
some numercial data, then choose the context menu "Filter by>Fourier"; 
this will, by default, filter the data with 35% of the Fourier 
coefficients at low freqency range.

After installed this plugin VisuMap will provide a help
page through the main menu "Help>About WaveTransforms". This
page describes the scripting API of the WaveTransforms plugin. 
The scripts installed by SetupWaveTransform.js use these 
scripting APIs to perform specific data filtering tasks.

For more information about this module please contact
VisuMap Technlogies at http://www.visumap.com/Contact.aspx.

VisuMap Technologies
