using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using Kusto.Ingest;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Klipboard.Workers.WorkerBase;

namespace Klipboard.Workers
{
    public class TempTableWorker : WorkerBase
    {
        private const string NotifcationTitle = "Temp Table Query";

        public TempTableWorker(WorkerCategory category, ISettings settings, object? icon = null)
            : base(category, ClipboardContent.Files | ClipboardContent.CSV | ClipboardContent.Text, settings, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Paste to Temporary Table";

        public override string GetToolTipText(ClipboardContent content) => "Upload clipboard tabular data or up to 100 files to a temporary table and invoke a query on it";

        public override bool IsMenuVisible(ClipboardContent content) => true;

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            var tempTableName = KustoDatabaseHelper.CreateTempTableName();
            var upstreamFileName = FileHelper.CreateUploadFileName("Table", "tsv");
            var target = GetQuickActionTarget();
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvData));

            if (await HandleSingleTextStreamAsync(databaseHelper, tempTableName, csvStream, "tsv", upstreamFileName, sendNotification))
            {
                InvokeTempTableQuery(tempTableName, sendNotification);
            }
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            var tempTableName = KustoDatabaseHelper.CreateTempTableName();
            var upstreamFileName = FileHelper.CreateUploadFileName("Text", "txt");
            var target = GetQuickActionTarget();
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);
            using var textStream = new MemoryStream(Encoding.UTF8.GetBytes(textData));

            if (await HandleSingleTextStreamAsync(databaseHelper, tempTableName, textStream, AppConstants.UnknownFormat, upstreamFileName, sendNotification))
            {
                InvokeTempTableQuery(tempTableName, sendNotification);
            }
        }

        public override async Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification)
        {
            var firstFile = true;
            var successCount = 0;
            var fileCount = 0;
            var target = GetQuickActionTarget();
            var tempTableName = KustoDatabaseHelper.CreateTempTableName();
            using var databaseHelper = new KustoDatabaseHelper(target.ConnectionString, target.DatabaseName);


            foreach (var path in FileHelper.ExpandDropFileList(filesAndFolders)) 
            {
                if(fileCount++ > 100)
                {
                    sendNotification(NotifcationTitle, "Limit of 100 files reached");
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

                    var success = await HandleSingleTextStreamAsync(databaseHelper, tempTableName, file, format, $"{fileInfo.Name}_{Guid.NewGuid()}", sendNotification);
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
                    IgnoreFirstRecord = false, // TODO consider if there's a way to detect that
                };

                var res = await databaseHelper.TryDirectIngestStorageToTable(path, tempTableName, ingestionProperties, storageOptions);
                if (!res.Success)
                {
                    sendNotification(NotifcationTitle, $"Failed to upload file '{path}' to temp table: {res.Error}");
                    continue;
                }

                successCount++;
            }

            if (successCount > 0) 
            {
                InvokeTempTableQuery(tempTableName, sendNotification);
            }
        }

        private async Task<bool> HandleSingleTextStreamAsync(KustoDatabaseHelper databaseHelper, string tempTableName, Stream dataStream, string format, string upstreamFileName, SendNotification sendNotification)
        {
            var uploadRes = await databaseHelper.TryUploadFileToEngineStagingAreaAsync(dataStream, upstreamFileName);

            if (!uploadRes.Success)
            {
                sendNotification(NotifcationTitle, $"Failed to upload file: {uploadRes.Error}");
                return false;
            }

            string schemaStr = AppConstants.TextLinesSchemaStr;

            if (format == AppConstants.UnknownFormat)
            {
                var autoDetectRes = await databaseHelper.TryAutoDetectTextBlobScheme(uploadRes.BlobUri);
                if (autoDetectRes.Success)
                {
                    schemaStr = autoDetectRes.Schema.ToString();
                    format = autoDetectRes.Format;
                }
                else
                {
                    sendNotification(NotifcationTitle, autoDetectRes.Error);
                }
            }
            else
            {
                var schemaRes = await databaseHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, format);
                if (schemaRes.Success)
                {
                    schemaStr = schemaRes.TableScheme.ToString();
                }
                else
                {
                    sendNotification(NotifcationTitle, schemaRes.Error);
                }
            }

            var createTableRes = await databaseHelper.TryCreateTableAync(tempTableName, schemaStr, ingestionBatchingTimeSeconds: 60, tableLifetimeDays: 3);

            if (!createTableRes.Success)
            {
                sendNotification(NotifcationTitle, createTableRes.Error);
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
                IgnoreFirstRecord = false, // TODO consider if there's a way to detect that
            };


            var uploadBlobRes = await databaseHelper.TryDirectIngestStorageToTable(uploadRes.BlobUri, tempTableName, ingestionProperties, storageOptions);

            if (!uploadBlobRes.Success)
            {
                sendNotification(NotifcationTitle, uploadBlobRes.Error);
                return false;
            }

            return true;
        }

        private void InvokeTempTableQuery(string tempTableName, SendNotification sendNotification)
        {
            var target = GetQuickActionTarget();
            var query = $"['{tempTableName}']\n| take 100";

            if (!InlineQueryHelper.TryInvokeInlineQuery(m_settings.GetConfig(), target.ConnectionString, target.DatabaseName, query, out var error))
            {
                sendNotification(NotifcationTitle, error ?? "Unknown error.");
            }
        }

        private Cluster GetQuickActionTarget()
        {
            return m_settings.GetConfig().ChosenCluster;
        }
    }
}
