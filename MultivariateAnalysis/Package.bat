del *.zip
copy Document\Help\MultivariateAnalysis.chm .
zip VmMultivariateAnalysis.zip MultivariateAnalysis.dll *.js MultivariateAnalysis.chm  
del .\MultivariateAnalysis.chm
zip VmMultivariateAnalysisSrc.zip *.js  Package.bat *.cs Properties/AssemblyInfo.cs *.csproj Document/*.bat Document/*.html Document/*.shfb Document/*.sitemap
copy VmMultivariateAnalysis.zip ..\..\VisuMapWeb\images
copy VmMultivariateAnalysisSrc.zip ..\..\VisuMapWeb\images
