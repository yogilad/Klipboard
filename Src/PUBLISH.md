# Publishing a New Release

> Note! At the moment the files published are not signed and warn when run by the user.

## Versioning 
* Klipboard uses a `v[Major].[Minor].[Build]` version format
* Major version changes include major feature changes, overhauls or breaking changes
* Minor version changes include feature additions, fixes and improvements
* Build version changes include bug fixes and minor changes

> Note! ClickOnce relies on a 4 segment version notation.
When setting the ClickOnce version, use the following format `[Major].[Minor].[Build].0`

## Before Publishing a New Release
1. Create a new PR named `v[Major].[Minor].[Build]`
1. Update the version in _AppConstants.cs_ to `v[Major].[Minor].[Build]`
1. Update the version in _ClickOnce.pubxml_ to `v[Major].[Minor].[Build].0`

## Build
1. Run `publish.bat'
1. Go to `src\App\Klipboard\bin\Publish
1. Make sure two zip files were created 
	1. **Klipboard.zip** - Contains a single file exe
	1. **Klipboard_ClickOnce.zip** - Contains a ClickOnce installer

## Create a New Release
1. Go to release and create a new release.
1. Set the release name to `v[Major].[Minor].[Build]`
1. Set the release notes including major changes included in the version
1. Upload the two zip files 