REM * Where is MSBUILD 
set MSBUILD=msbuild.exe
where msbuild.exe 
if %errorlevel% equ 1 ( set MSBUILD="C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" )

REM * Clean
rmdir App\Klipboard\bin\Release /s /q
rmdir App\Klipboard\bin\Publish /s /q

REM * Build 
%MSBUILD% -p:configuration=Release -t:restore
if %errorlevel% neq 0 ( goto BuildFailed)

%MSBUILD% -p:configuration=Release -t:build
if %errorlevel% neq 0 ( goto BuildFailed)

REM * Publish
%MSBUILD% /t:publish /p:PublishProfile=ClickOnce -p:PublishDir="bin\Publish\Klipboard_ClickOnce"
if %errorlevel% neq 0 ( goto BuildFailed)

%MSBUILD% /t:publish /p:PublishProfile=SingleFile -p:PublishDir="bin\Publish\Klipboard"
if %errorlevel% neq 0 ( goto BuildFailed)

REM * Zip results
cd App\Klipboard\bin\Publish\
tar -acf Klipboard.zip "Klipboard\Klipboard.pdb" "Klipboard\Klipboard.exe"
if %errorlevel% neq 0 ( goto BuildFailed)

tar -acf Klipboard_ClickOnce.zip "Klipboard_ClickOnce"
if %errorlevel% neq 0 ( goto BuildFailed)

echo Build Succeeded!
goto End

:BuildFailed
echo Build Failed!

:End
cd ..\..\..\..