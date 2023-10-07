echo off

REM * Zip results
cd App\Klipboard\bin\Publish\
tar -acf Klipboard.zip Klipboard
if %errorlevel% neq 0 ( goto PackFailed)
cd ..\..\..\..

REM * Zip results
cd Setup\
rename Release KlipboardSetup
tar -acf KlipboardSetup.zip KlipboardSetup
if %errorlevel% neq 0 ( goto PackFailed)
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
