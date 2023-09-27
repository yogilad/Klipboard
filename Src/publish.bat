REM * Where is MSBUILD 
set MSBUILD=msbuild.exe
where msbuild.exe 
if %errorlevel% == 1 ( set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" )

REM * Clean
rmdir App\Klipboard\bin\Release /s /q
rmdir App\Klipboard\bin\Publish /s /q

REM * Build 
%MSBUILD% -p:configuration=Release -t:restore
%MSBUILD% -p:configuration=Release -t:build

REM * Publish
%MSBUILD% /t:publish /p:PublishProfile=ClickOnce -p:PublishDir="bin\Publish\ClickOnce"
%MSBUILD% /t:publish /p:PublishProfile=SingleFile -p:PublishDir="bin\Publish\SingleFile"

REM * Zip results
tar -acf App\Klipboard\bin\Publish\Klipboard.zip ".\App\Klipboard\bin\Publish\SingleFile\Klipboard.pdb" ".\App\Klipboard\bin\Publish\SingleFile\Klipboard.exe"
tar -acf App\Klipboard\bin\Publish\Klipboard_ClickOnce.zip ".\App\Klipboard\bin\Publish\ClickOnce"