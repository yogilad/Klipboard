# Klipboard - Query Anything With Kusto 

## A Clipboard Companion for Kusto
Kilpboard aims to provide a simple and intuitive way for people to get insights on their data using KQL. Simply copy text, tables or files in windows, then use Klipboard to unleash KQL's power on your data.

Klipboard runs in the System Tray. When clicked it will display a menu of actions that can be done with the content that is currently placed in the operating system clipboard.

**[What's New](https://github.com/yogilad/Klipboard/blob/main/ReleaseNotes.md)**

**[Installation and Quick Start Guide](https://github.com/yogilad/Klipboard/wiki/Quick-Start-Guide)**

**[Wiki with Instructions and Help](https://github.com/yogilad/Klipboard/wiki)**

**[List of 3rd Parties and Open Source Resources Used](https://github.com/yogilad/Klipboard/blob/main/ThirdPartyAtribution.md)**


### Query Anything With Ease 
Klipboard Quick Actions helps you run queries on **structured and unstructured** data without having to go through the trouble of creating tables and uploading data to them.

### Upload Anything With Ease
If you do need the power of Kusto tables, Klipboard will help you instantly upload **structured and unstructured** data to existing, new and even temporaty tables with just a few clicks.

### Query Structured Data from Office Applications 
Copy any tables from Excel to query them using Kusto. 
No need to manually select tables in Excel. If an Excel sheet contains a single table, select the entire sheet from the top left corenr to query it.

You can also copy tables from Word, Outlook and Power Point with some limitations. Tables with new lines or tabs in the content copied from these apps may fail parsing.

### Query Structured Files and Text
Easily query CSV & TSV files. Copy *.csv and *.tsv files to query then with Kusto. 
You can also copy CSV and TSV structured text to query it.

### Query Tables from HTML Pages
Select and copy tables from web pages to query them with KQL. 

Be sure to only copy the table itself and not the surrounding content. Please mind table data in HTML may be represented in multiple ways, so parsing may not always succeed.

### Query Free Text
Easily query unstructured text with Kusto. Copy any multiline text or text file to parse and query it with Kusto.
If the data you commonly query has a fixed structure, you can set a Quick Actions predefined KQL to parse the data with.


# Road Map
## Milestone #2 - Project Maturity
* Logs
* Proper Testing and E2Es
* Signed application and installation

## Milestone #3 - Mass Ingestion & UX Improvements
* Improved Notifications
* Action progress tracking  
* Queued, Direct and Streaming ingestion to new or existing tables

## Milestone #4 - Modern UX & Multi Platform
* UX Overhaul using .NET MAUI
* Multi platform support (Linux and Mac)

## Milestone #5 - Functionality Improvements
* Ingestion Improvements: Ingestion Tags, Creation Time, file name and extension filtering
* Mapping file extensions to data formats
* Supporting multiple free-text parsing rules
