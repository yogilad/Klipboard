# Release Notes

## Version v0.1.2
### Features
* New Command: Inspect Clipboard Content (shows a preview of the content of the clipboard)
* Add progress notification for temp table and external data query.
* Better error reporting notifcation.
* Add basic logging with Serilog, and include Kusto SDK logs (warnings and errors)
* Single process - the app will not run twice.
* Published assemblies are versioned

### Bug Fixes
* Column name auto-correct for inline query and external table query is less strict, preserving the source column name
* When creating a temp table from json data and auto correcting column names, create a mapping policy to fix map between the json file and the column names

## Version v0.1.1
### Features
* MSI Installation Support
* Added Sample Data folder to support new Wiki
* App links and readme point to new Wiki

### Bug Fixes
* Force name fixes on external data schema detected column names
* Redo format detection
* External data works with parquet
* Parquet ingestion to temp table works
* Fix temp file name creator
* Fix auto detection priorities

## Version v0.1.0
### Features
* First version release
* Inline structured and free-text query
* External table query
* Temp table query

