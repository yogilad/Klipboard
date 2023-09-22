using System.Text;
using Kusto.Ingest;

using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class TempTableWorker : WorkerBase
    {
        private const string NotificationTitle = "Temp Table Query";
        private const string FirstRowIsHeader = "First Row Is Header";
        private const string NoHeaderRow = "No Header Row";


        public TempTableWorker(ISettings settings)
            : base(ClipboardContent.Files | ClipboardContent.CSV | ClipboardContent.Text, settings, new List<string>() { FirstRowIsHeader, NoHeaderRow })
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Paste to Temporary Table";

        public override string GetToolTipText() => "Upload clipboard tabular data or up to 100 files to a temporary table and invoke a query on it";

        public override bool IsMenuVisible() => true;

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification, string? chosenOptions)
        {
            var tempTableName = KustoDatabaseHelper.CreateTempTableName();
            var upstreamFileName = FileHelper.CreateUploadFileName("Table", "tsv");
            var target = GetQuickActionTarget();
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvData));

            if (await HandleSingleTextStreamAsync(databaseHelper, tempTableName, csvStream, "tsv", upstreamFileName, sendNotification, chosenOptions))
            {
                InvokeTempTableQuery(tempTableName, sendNotification);
            }
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification, string? chosenOptions)
        {
            var tempTableName = KustoDatabaseHelper.CreateTempTableName();
            var upstreamFileName = FileHelper.CreateUploadFileName("Text", "txt");
            var target = GetQuickActionTarget();
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            using var textStream = new MemoryStream(Encoding.UTF8.GetBytes(textData));

            if (await HandleSingleTextStreamAsync(databaseHelper, tempTableName, textStream, AppConstants.UnknownFormat, upstreamFileName, sendNotification, chosenOptions))
            {
                InvokeTempTableQuery(tempTableName, sendNotification);
            }
        }

        public override async Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification, string? chosenOption)
        {
            var firstFile = true;
            var successCount = 0;
            var fileCount = 0;
            var target = GetQuickActionTarget();
            var tempTableName = KustoDatabaseHelper.CreateTempTableName();
            var firstRowIsHeader = FirstRowIsHeader.Equals(chosenOption);
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            var m_ingestionRunner = new KustoIngestRunner(databaseHelper.GetDirectIngestClient(), degreeOfParallelism: 2);

            m_ingestionRunner.TraceEvent += (level, message) => 
            {
                if (!level.Equals("Info", StringComparison.OrdinalIgnoreCase))
                {
                    sendNotification(NotificationTitle, $"{level}: {message}");
                }
            };

            m_ingestionRunner.ReportProgress += (workItem, success) =>
            {
                if (success)
                {
                    Interlocked.Increment(ref successCount);
                }
            };

            foreach (var path in FileHelper.ExpandDropFileList(filesAndFolders)) 
            {
                if(fileCount++ > 100)
                {
                    sendNotification(NotificationTitle, "Limit of 100 files reached");
                    break;
                }

                var format = AppConstants.UnknownFormat;
                var fileInfo = new FileInfo(path);

                if (!string.IsNullOrWhiteSpace(fileInfo.Extension))
                {
                    format = fileInfo.Extension.TrimStart('.');
                }

                if (firstFile)
                {
                    using var file = File.OpenRead(path);

                    var success = await HandleSingleTextStreamAsync(databaseHelper, tempTableName, file, format, $"{fileInfo.Name}_{Guid.NewGuid()}", sendNotification, chosenOption);
                    if (!success)
                    {
                        // A notification was sent from the failed function
                        return;
                    }

                    successCount++;
                    firstFile = false;
                    continue;
                }

                var storageOptions = new StorageSourceOptions();
                var ingestionProperties = new KustoIngestionProperties()
                {
                    DatabaseName = target.DatabaseName,
                    TableName = tempTableName,
                    Format = FileHelper.GetFormatFromExtension(format),
                    IgnoreFirstRecord = firstRowIsHeader,
                };

                await m_ingestionRunner.QueueWorkItemAsync(new IngestFileWorkItem(path, ingestionProperties, storageOptions));
            }

            await m_ingestionRunner.CloseAndWaitForCompletionAsync();

            if (successCount > 0) 
            {
                InvokeTempTableQuery(tempTableName, sendNotification);
            }
        }

        private async Task<bool> HandleSingleTextStreamAsync(KustoDatabaseHelper databaseHelper, string tempTableName, Stream dataStream, string format, string upstreamFileName, SendNotification sendNotification, string? chosenOption)
        {
            var uploadRes = await databaseHelper.TryUploadFileToEngineStagingAreaAsync(dataStream, upstreamFileName);
            var firstRowIsHeader = FirstRowIsHeader.Equals(chosenOption);

            if (!uploadRes.Success)
            {
                sendNotification(NotificationTitle, $"Failed to upload file: {uploadRes.Error}");
                return false;
            }

            string schemaStr = AppConstants.TextLinesSchemaStr;

            if (format == AppConstants.UnknownFormat)
            {
                var autoDetectRes = await databaseHelper.TryAutoDetectTextBlobScheme(uploadRes.BlobUri, firstRowIsHeader);
                if (autoDetectRes.Success)
                {
                    schemaStr = autoDetectRes.Schema.ToString();
                    format = autoDetectRes.Format;
                }
                else
                {
                    sendNotification(NotificationTitle, autoDetectRes.Error);
                }
            }
            else
            {
                var schemaRes = await databaseHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, format, firstRowIsHeader);
                if (schemaRes.Success)
                {
                    schemaStr = schemaRes.TableScheme.ToString();
                }
                else
                {
                    sendNotification(NotificationTitle, schemaRes.Error);
                }
            }

            var createTableRes = await databaseHelper.TryCreateTableAync(tempTableName, schemaStr, ingestionBatchingTimeSeconds: 60, tableLifetimeDays: 3);

            if (!createTableRes.Success)
            {
                sendNotification(NotificationTitle, createTableRes.Error);
                return false;
            }

            var storageOptions = new StorageSourceOptions()
            {
                CompressionType = Kusto.Data.Common.DataSourceCompressionType.GZip,
            };

            var appConfig = m_settings.GetConfig();

            var ingestionProperties = new KustoIngestionProperties()
            {
                DatabaseName = appConfig.ChosenCluster.DatabaseName,
                TableName = tempTableName,
                Format = FileHelper.GetFormatFromExtension(format),
                IgnoreFirstRecord = firstRowIsHeader,
            };


            var uploadBlobRes = await databaseHelper.TryDirectIngestStorageToTable(uploadRes.BlobUri, ingestionProperties, storageOptions);

            if (!uploadBlobRes.Success)
            {
                sendNotification(NotificationTitle, uploadBlobRes.Error);
                return false;
            }

            return true;
        }

        private void InvokeTempTableQuery(string tempTableName, SendNotification sendNotification)
        {
            var target = GetQuickActionTarget();
            var query = new StringBuilder()
                .Append("['")
                .Append(tempTableName)
                .AppendLine("']")
                .AppendLine("| take 100")
                .AppendLine()
                .AppendLine("//Rename the table if needed")
                .Append(".rename table ['")
                .Append(tempTableName)
                .AppendLine("'] to '<new name>'")
                .AppendLine()
                .AppendLine("// Cancel auto deletion")
                .Append(".delete table ['")
                .Append(tempTableName)
                .AppendLine("']  policy auto_delete")
                .ToString();

            if (!InlineQueryHelper.TryInvokeInlineQuery(m_settings.GetConfig(), target.ConnectionString, target.DatabaseName, query, out var error))
            {
                sendNotification(NotificationTitle, error ?? "Unknown error.");
            }
        }

        private Cluster GetQuickActionTarget()
        {
            return m_settings.GetConfig().ChosenCluster;
        }
    }
}
