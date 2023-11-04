REM * Build the project before publishing it 
REM * Usage:
REM *   build.bat [version]

echo off

REM * Clean
echo Cleaning up build targets...
rmdir App\Klipboard\bin\Release /s /q
rmdir App\Klipboard\bin\Publish /s /q
rmdir Setup\Release /s /q

REM * Build
echo Starting build...
dotnet restore -p:configuration=Release
if %errorlevel% neq 0 ( goto BuildFailed)

dotnet build -p:configuration=Release
if %errorlevel% neq 0 ( goto BuildFailed)

REM * Publish
echo Starting publish...
dotnet publish -p:configuration=Release /p:PublishProfile=SetupProfile -p:PublishDir="bin\Publish\KlipboardSetup"
if %errorlevel% neq 0 ( goto BuildFailed)

dotnet publish -p:configuration=Release /p:PublishProfile=SingleFile -p:PublishDir="bin\Publish\Klipboard"
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
