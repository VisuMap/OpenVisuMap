setlocal
set PATH=C:\Program Files (86)\HTML Help Workshop;C:\Windows\Microsoft.NET\Framework\v4.0.30319;%PATH%
set SHFBRoot=C:\app\Sandcastle\EWSoftware\Sandcastle Help File Builder
MSBuild.exe MVA.shfbproj
endlocal
