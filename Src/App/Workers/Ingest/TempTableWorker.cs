using System.Text;
using Kusto.Data.Common;
using Kusto.Ingest;

using Klipboard.Utils;

namespace Klipboard.Workers
{
    public class TempTableWorker : WorkerBase
    {
        private const string NotificationTitle = "Temp Table Query";
        private const string FirstRowIsHeader = "First Row Is Header";
        private const string NoHeaderRow = "No Header Row";


        public TempTableWorker(ISettings settings, INotificationHelper notificationHelper)
            : base(ClipboardContent.Files | ClipboardContent.CSV | ClipboardContent.Text, settings, notificationHelper, new List<string>() { FirstRowIsHeader, NoHeaderRow })
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Paste to Temporary Table";

        public override string GetToolTipText() => "Upload clipboard tabular data or up to 100 files to a temporary table and invoke a query on it";

        public override bool IsMenuVisible() => true;

        public override async Task HandleCsvAsync(string csvData, string? chosenOptions)
        {
            var progressNotification = m_notificationHelper.ShowProgressNotification(NotificationTitle, $"Clipboard Table", "Preparing Data", "");
            var tempTableName = KustoDatabaseHelper.CreateTempTableName();
            var upstreamFileName = FileHelper.CreateUploadFileName("Table", "tsv");
            var target = GetQuickActionTarget();
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvData));

            if (await HandleSingleTextStreamAsync(databaseHelper, tempTableName, csvStream, FileHelper.TsvFormatDefinition, upstreamFileName, chosenOptions, progressNotification))
            {
                InvokeTempTableQuery(tempTableName, progressNotification);
            }
        }

        public override async Task HandleTextAsync(string textData, string? chosenOptions)
        {
            var progressNotification = m_notificationHelper.ShowProgressNotification(NotificationTitle, $"Clipboard Text", "Preparing Data", "");
            var tempTableName = KustoDatabaseHelper.CreateTempTableName();
            var upstreamFileName = FileHelper.CreateUploadFileName("Text", "txt");
            var target = GetQuickActionTarget();
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            using var textStream = new MemoryStream(Encoding.UTF8.GetBytes(textData));

            if (await HandleSingleTextStreamAsync(databaseHelper, tempTableName, textStream, FileHelper.UnknownFormatDefinition, upstreamFileName, chosenOptions, progressNotification))
            {
                InvokeTempTableQuery(tempTableName, progressNotification);
            }
        }

        public override async Task HandleFilesAsync(List<string> filesAndFolders, string? chosenOption)
        {
            var progressNotification = m_notificationHelper.ShowProgressNotification(NotificationTitle, $"Clipboard Files", "Preparing Data", "");
            var log = new List<string>();
            var mutex = new object();
            var firstFile = true;
            var fileCount = 0.0;
            var progressCount = 0.0;
            var target = GetQuickActionTarget();
            var tempTableName = KustoDatabaseHelper.CreateTempTableName();
            var firstRowIsHeader = FirstRowIsHeader.Equals(chosenOption);
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            var m_ingestionRunner = new KustoIngestRunner(databaseHelper.TryDirectIngestStorageToTable, degreeOfParallelism: 2);

            m_ingestionRunner.TraceEvent += (level, message) => 
            {
                if (!level.Equals("Info", StringComparison.OrdinalIgnoreCase))
                {
                    log.Add($"{level}: {message}");
                }
            };

            m_ingestionRunner.ReportProgress += (workItem, success) =>
            {
                lock(mutex)
                {
                    progressCount++;
                    progressNotification.UpdateProgress("Uploading data", (progressCount / fileCount) * 0.4 + 0.5, $"{progressCount}/{fileCount}");
                }
            };

            foreach (var path in FileHelper.ExpandDropFileList(filesAndFolders)) 
            {
                var fileInfo = new FileInfo(path);
                var formatResult = FileHelper.GetFormatFromFileName(fileInfo.Name);
                var upstreamFileName = FileHelper.CreateUploadFileName(fileInfo.Name);

                if (firstFile)
                {
                    using var file = File.OpenRead(path);

                    var success = await HandleSingleTextStreamAsync(databaseHelper, tempTableName, file, formatResult, upstreamFileName, chosenOption, progressNotification, path);
                    if (!success)
                    {
                        // A notification was sent from the failed function
                        return;
                    }

                    progressCount++;
                    firstFile = false;
                    continue;
                }

                lock (mutex)
                {
                    fileCount++;
                    
                    if (fileCount > 100)
                    {
                        log.Add("Warning: Limit of 100 files reached");
                        break;
                    }

                    progressNotification.UpdateProgress("Uploading data", (progressCount / fileCount) * 0.4 + 0.5, $"{progressCount}/{fileCount}");
                }

                var storageOptions = new StorageSourceOptions()
                {
                    Compress = !formatResult.DoNotCompress,
                };

                var ingestionProperties = new KustoIngestionProperties()
                {
                    DatabaseName = target.DatabaseName,
                    TableName = tempTableName,
                    Format = formatResult.Format,
                    IgnoreFirstRecord = firstRowIsHeader,
                };

                await m_ingestionRunner.QueueWorkItemAsync(new IngestFileWorkItem(path, ingestionProperties, storageOptions));
            }

            await m_ingestionRunner.CloseAndWaitForCompletionAsync();

            InvokeTempTableQuery(tempTableName, progressNotification);
            if (log.Count > 0)
            {
                m_notificationHelper.ShowExtendedNotification(NotificationTitle, "Some errors occured during run", string.Join("\n", log));
            }
        }

        private async Task<bool> HandleSingleTextStreamAsync(KustoDatabaseHelper databaseHelper, 
            string tempTableName, 
            Stream dataStream, 
            FileFormatDefiniton formatDefinition, 
            string upstreamFileName, 
            string? chosenOption,
            IProgressNotificationUpdater progressNotification,
            string? filePath = null)
        {
            const double steps = 5 * 2; // double 2 => (step / steps) * (50 / 100) = step / (steps * 2)

            // Steps #1
            progressNotification.UpdateProgress("Uploading initial data", 1 / steps, "");
            var uploadRes = await databaseHelper.TryUploadFileToEngineStagingAreaAsync(dataStream, upstreamFileName, formatDefinition, filePath);
            var firstRowIsHeader = FirstRowIsHeader.Equals(chosenOption);

            if (!uploadRes.Success)
            {
                progressNotification.CloseNotification();
                m_notificationHelper.ShowExtendedNotification(NotificationTitle, "Failed to upload file", uploadRes.Error);
                return false;
            }

            // #step #2
            progressNotification.UpdateProgress("Detecting schema", 2 / steps, "");

            string schemaStr = AppConstants.TextLinesSchemaStr;
            IngestionMapping mapping = null;

            if (formatDefinition.Extension == AppConstants.UnknownFormat)
            {
                var autoDetectRes = await databaseHelper.TryAutoDetectTextBlobScheme(uploadRes.BlobUri, firstRowIsHeader);
                if (autoDetectRes.Success)
                {
                    schemaStr = autoDetectRes.Schema.ToSchemaString(strictEntityNaming: true);
                    formatDefinition = FileHelper.GetFormatFromExtension(autoDetectRes.Format);
                    
                    switch(formatDefinition.Format) 
                    {
                        case DataSourceFormat.json:
                        case DataSourceFormat.singlejson:
                        case DataSourceFormat.multijson:
                            mapping = autoDetectRes.Schema.ToJsonMapping();
                            break;
                    }
                }
                else
                {
                    progressNotification.CloseNotification();
                    m_notificationHelper.ShowExtendedNotification(NotificationTitle, "Failed to detect file fromat and schema", autoDetectRes.Error);
                    return false;
                }
            }
            else
            {
                var schemaRes = await databaseHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, formatDefinition.Extension, firstRowIsHeader);
                if (schemaRes.Success)
                {
                    schemaStr = schemaRes.TableScheme.ToSchemaString(strictEntityNaming: true);

                    switch (formatDefinition.Format)
                    {
                        case DataSourceFormat.json:
                        case DataSourceFormat.singlejson:
                        case DataSourceFormat.multijson:
                            mapping = schemaRes.TableScheme.ToJsonMapping();
                            break;
                    }
                }
                else
                {
                    progressNotification.CloseNotification();
                    m_notificationHelper.ShowExtendedNotification(NotificationTitle, "Failed to detect file schema", schemaRes.Error);
                    return false;
                }
            }

            // #step #3
            progressNotification.UpdateProgress("Creating table", 3 / steps, "");
            var createTableRes = await databaseHelper.TryCreateTableAync(tempTableName, schemaStr, ingestionBatchingTimeSeconds: 60, tableLifetimeDays: 3);

            if (!createTableRes.Success)
            {
                progressNotification.CloseNotification();
                m_notificationHelper.ShowExtendedNotification(NotificationTitle, "Failed to create a temp table", createTableRes.Error);
                return false;
            }

            // #step #4
            progressNotification.UpdateProgress("Uploading data", 4 / steps, "1/1");

            var storageOptions = new StorageSourceOptions()
            {
                Compress = !formatDefinition.DoNotCompress,
            };

            var ingestionProperties = new KustoIngestionProperties()
            {
                DatabaseName = GetQuickActionTarget().DatabaseName,
                TableName = tempTableName,
                Format = formatDefinition.Format,
                IgnoreFirstRecord = firstRowIsHeader,
            };

            if (mapping != null)
            {
                ingestionProperties.IngestionMapping = mapping;
            }

            var uploadBlobRes = await databaseHelper.TryDirectIngestStorageToTable(uploadRes.BlobUri, ingestionProperties, storageOptions);

            if (!uploadBlobRes.Success)
            {
                progressNotification.CloseNotification();
                m_notificationHelper.ShowExtendedNotification(NotificationTitle, "Failed to upload first file", uploadBlobRes.Error);
                return false;
            }

            progressNotification.UpdateProgress("Uploading data", 5 / steps, "1/1");
            return true;
        }

        private void InvokeTempTableQuery(string tempTableName, IProgressNotificationUpdater progressNotification)
        {
            progressNotification.UpdateProgress("Running Query", 0.9, "");
            var target = GetQuickActionTarget();
            var query = new StringBuilder()
                .AppendLine("// Query Created With Klipboard (https://github.com/yogilad/Klipboard/wiki)")
                .Append("['")
                .Append(tempTableName)
                .AppendLine("']")
                .AppendLine("| take 100")
                .AppendLine()
                .AppendLine("// Rename the table if needed")
                .Append(".rename table ['")
                .Append(tempTableName)
                .AppendLine("'] to ['<new name>']")
                .AppendLine()
                .AppendLine("// Cancel auto deletion")
                .Append(".delete table ['")
                .Append(tempTableName)
                .AppendLine("'] policy auto_delete")
                .AppendLine()
                .AppendLine("// Manually delete the table")
                .Append(".drop table ['")
                .Append(tempTableName)
                .AppendLine("']")
                .ToString();

            if (!InlineQueryHelper.TryInvokeInlineQuery(m_settings.GetConfig(), target.ConnectionString, target.DatabaseName, query, out var error))
            {
                progressNotification.CloseNotification();
                m_notificationHelper.ShowExtendedNotification(NotificationTitle, $"Failed to invoke query on temp table '{tempTableName}'", error ?? "Unknown error.");
                return;
            }

            progressNotification.UpdateProgress("Query Launched", 1.0, "");
            progressNotification.CloseNotification(withinSeconds: 5);
        }

        private Cluster GetQuickActionTarget()
        {
            return m_settings.GetConfig().ChosenCluster;
        }
    }
}
