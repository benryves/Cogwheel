del Cogwheel.zip
set path=%path%;"C:\Program Files\7-Zip"
pushd "..\Cogwheel SlimDX\bin\Release"
7z u -tzip -mx=9 ..\..\..\Deployment\Cogwheel.zip "Cogwheel Interface.exe*" *.dll "ROM Data\*.romdata"
popd