# Building and Using Klipboard

## Prerequisites
* Download a local copy of the source code from the GitHub or sync the repo using [git](https://git-scm.com/downloads).  
* Install [dotnet 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)

# Instrcutions 
* Open a command line windows
* CD into the `Src` directorry of the repository
* Run `build.bat`
* Copy the content of `Src\App\Klipboard\bin\Publish\Klipboard` to a directory of your choice.
* Run `Klipboard.exe` 

# Publishing a New Release

> Note! At the moment the files published are not signed and warn when run by the user.

## Versioning 
* Klipboard uses a _v[Major].[Minor].\[Build\]_ version format
* Major version changes include major feature changes, overhauls or breaking changes
* Minor version changes include feature additions, fixes and improvements
* Build version changes include bug fixes and minor changes

## Before Publishing a New Release
1. Create a new PR named _v[Major].[Minor].\[Build\]_
1. Edit `ReleaseNotes.md`
1. In `Src\App\Utils\Common\AppConstants.cs` update the value of `ApplicationVersion` to _[Major].[Minor].\[Build\]_
1. in `Src\directory.build.props` update the values of `<Version>`, `<AssemblyVersion>`, and `<FileVersion>` to to _[Major].[Minor].\[Build\]_
1. In Visual Studio, 
	1. Select the _Setup_ project 
	1. Set the `version` in the properties pane to _v[Major].[Minor].\[Build\]_
	1. Confirm the dialog requesting _ProductCode_ change

## Build
1. Open a shell (cmd) window and CD into the Src directory
1. Run the build script `build.bat` 
1. Confirm the build had ended sucessfully
1. In Visual Studio, set the build type to `Release` and build the `Setup` project

## Pack
1. Open a shell (cmd) window and CD into the Src directory
1. Run `pack.bat` and confirm it ended successfully
1. Make sure the following 2 zip files were created
	1. `Src\App\bin\Publish\Klipboard.zip`
	1. `Src\Setup\Release\KlipboardSetup.zip`

## Create a New Release
1. Complete the version PR
1. Go to release and create a new release.
1. Set the release name to _v[Major].[Minor].\[Build\]_
1. Set the release to create a tag with the value of _v[Major].[Minor].\[Build\]_
1. Add release notes for major changes included in the version
1. Upload `Src\App\bin\Publish\Klipboard.zip`
1. Upload `Src\Setup\Release\KlipboardSetup.zip`
