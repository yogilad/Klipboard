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
1. Update the version in `Src\App\Utils\Common\AppConstants.cs_` to _v[Major].[Minor].\[Build\]_
1. In Visual Studio, 
	1. Select the _Setup_ project 
	1. Set the version in the properties pane to _v[Major].[Minor].\[Build\]_
	1. Confirm the dialog requesting _ProductCode_ change

## Build
1. Open a shell (cmd) window and CD into the Src directory
1. Run the build script `build.bat [version]` where _[version]_ is _[Major].[Minor].\[Build\]_ (do not use the 'v' prefix)
1. Confirm the build had ended sucessfully
1. In Visual Studio, build the _Setup_ project

## Pack
1. Open a shell (cmd) window and CD into the Src directory
1. Run `pack.bat` and confirm it ended sucessfully
1. Make sure the following 2 zip files were created
	1. `Src\App\bin\Publish\Klipboard.zip`
	1. `Src\Setup\Release\KlipboardSetup.zip`

## Create a New Release
1. Complete the version PR
1. Go to release and create a new release.
1. Set the release name to _v[Major].[Minor].\[Build\]_
1. Set the release to create a tag with the value of _v[Major].[Minor].\[Build\]_
1. Set the release notes including major changes included in the version
1. Upload `Src\App\bin\Publish\Klipboard.zip`
1. Upload `Src\Setup\Release\KlipboardSetup.zip`