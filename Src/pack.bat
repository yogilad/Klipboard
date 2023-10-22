echo off

REM * Clean 
echo Cleaning targets
del   App\Klipboard\bin\Publish\Klipboard.zip
rmdir Setup\KlipboardSetup /s /q
del   Setup\KlipboardSetup.zip

REM * Zip results
echo Pack single-file
cd App\Klipboard\bin\Publish\
tar -acf Klipboard.zip Klipboard
if %errorlevel% neq 0 ( goto PackFailed)
dir Klipboard.zip
cd ..\..\..\..

REM * Zip results
echo Pack Setup
cd Setup\
xcopy Release KlipboardSetup /E /H /C /I
tar -acf KlipboardSetup.zip KlipboardSetup
if %errorlevel% neq 0 ( goto PackFailed)
dir KlipboardSetup.zip
cd ..

REM * Announce Success
echo ****************
echo Pack Succeeded!
echo ****************
goto End

REM * Announce Failure
:PackFailed
echo *************
echo Pack Failed!
echo *************

REM * Go back to Src dir
:End
