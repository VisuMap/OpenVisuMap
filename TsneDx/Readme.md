
# GPU based Implementation of the tSNE algorithm

This module is an open source implementation of the tSNE algorithm for the Windows+DirectX+GPU platform. The module implements the full tSNE algorithm as described in the initial tSNE paper. For large datasets (with more 30 000 data points) the program doesn't cache the distance matrix, but calculate the distance on-fly, so that the memory complexity is about O(N) instead of O(N^2). 

This GPU based implementation gives a 10x to 40x speedup comparing to CPU implementation on normal desktop machines. The computation complexity of the implementation is still O(N^2),  so that it is only suitable for dataset with less that one million data points.

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
  The file TsneDx.exe is a .NET framework assemly, it can be integreted into any .NET project by adding TsneDx.exe to the reference. For ptyhon users, this package inclues a sample program TsneMap.py that shows how to access TsneDx from python programs.
  
### Implementation and Build:

  The module is developed with Visual Studio 2017 with code in C#, HLSL and Python. It uses the SharpDX run-time library to access DirectX system from C# code. It uses python.net to enable python programs to access TsneDx services. 
  
  The following are steps to build the project:
  
    * Clone the project to a local Windows machine.
    
    * Insall DirectX11 and SharpDX on the machine, if it is not already done.
    
    * Build the HLSL shaders with NMake and the makefile BuildShaders.mk. You find out the location of NMake.exe on your machine.
    
    * Load the visual studio project TsneDx.csproj into Visual Studio and build the Release configuration.
    
    * Run the batch script Package.bat to package executables and sample code into a zip file for deployment.
  

