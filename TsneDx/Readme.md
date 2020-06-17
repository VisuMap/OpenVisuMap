
# GPU based Implementation of the tSNE algorithm

This module is an open source implementation of the tSNE algorithm for the Windows+DirectX+GPU platform. The module implements the full tSNE algorithm as described in the initial tSNE paper. For large datasets (with more 30 000 data points) the program doesn't cache the distance matrix, but calculate the distance on-fly, so that the memory complexity is about O(N) instead of O(N^2). 

### Usage:
  
  TsneDx &lt;input-file&gt;.csv [perplexity-ratio]  [learning-epochs]  [out-dim]
  
  where [perplexity-ratio] is the perpelxity-ratio; [learning-epochs] is the learning epochs; [out-dim] is the output dimension. Example:
  
  C:&gt;TsneDx.exe SP500.csv 0.05 500 2

  Result will be stored in the file SP500_map.csv

### Requirements:
  Windows 10/64bit; DirectX 11 or higher; GPU NVIDIA or AMD.

### Limitations:
  The program only supports the Euclidean distance. All numerical calculations are done with float32 precision.

### Installation:
  Download and unzip TsneDx.zip; Then add the directory to the PATH environment variable. 

### Programmatical Integration:
  The file TsneDx.exe is a .NET framework assemly, it can be integreted into any .NET project by adding TsneDx.exe to the reference. 
