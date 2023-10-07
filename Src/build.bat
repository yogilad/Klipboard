REM * Build the project before publishing it 
REM * Usage:
REM *   build.bat [version]

echo off

REM * Version 
Set VERSION=%1
if defined VERSION (goto Continue)
echo Please pass the build version in the first argument e.g. > build.bat 1.0.0 
goto End

:Continue

REM * Clean
rmdir App\Klipboard\bin\Release /s /q
rmdir App\Klipboard\bin\Publish /s /q

REM * Build 
dotnet restore -p:configuration=Release
if %errorlevel% neq 0 ( goto BuildFailed)

dotnet build -p:configuration=Release
if %errorlevel% neq 0 ( goto BuildFailed)

REM * Publish
dotnet publish -p:Version=%VERSION% -p:configuration=Release /p:PublishProfile=SetupProfile -p:PublishDir="bin\Publish\KlipboardSetup"
if %errorlevel% neq 0 ( goto BuildFailed)

dotnet publish -p:Version=%VERSION% -p:configuration=Release /p:PublishProfile=SingleFile -p:PublishDir="bin\Publish\Klipboard"
if %errorlevel% neq 0 ( goto BuildFailed)

REM * Announce Success
echo ****************
echo Build Succeeded!
echo ****************
goto End

REM * Announce Failure
:BuildFailed
echo *************
echo Build Failed!
echo *************

REM * Go back to Src dir
:End
