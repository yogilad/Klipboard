echo off

REM * Clean
rmdir App\Klipboard\bin\Release /s /q
rmdir App\Klipboard\bin\Publish /s /q

REM * Build 
dotnet restore -p:configuration=Release
if %errorlevel% neq 0 ( goto BuildFailed)

dotnet build -p:configuration=Release
if %errorlevel% neq 0 ( goto BuildFailed)

REM * Publish
dotnet publish -p:configuration=Release /p:PublishProfile=SetupProfile -p:PublishDir="bin\Publish\KlipboardSetup"
if %errorlevel% neq 0 ( goto BuildFailed)

dotnet publish -p:configuration=Release /p:PublishProfile=SingleFile -p:PublishDir="bin\Publish\Klipboard"
if %errorlevel% neq 0 ( goto BuildFailed)

REM * Zip results
cd App\Klipboard\bin\Publish\
tar -acf Klipboard.zip Klipboard
if %errorlevel% neq 0 ( goto BuildFailed)
cd ..\..\..\..

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
