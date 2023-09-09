using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using Kusto.Ingest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Klipboard.Workers.WorkerBase;

namespace Klipboard.Workers
{
    public class TempTableWorker : WorkerBase
    {
        private const string NotifcationTitle = "Temp Table Query";

        public TempTableWorker(WorkerCategory category, AppConfig config, object? icon = null)
            : base(category, ClipboardContent.Files | ClipboardContent.CSV | ClipboardContent.Text, config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Paste to Temporary Table";

        public override string GetToolTipText(ClipboardContent content) => "Upload clipboard tabular data or up to 100 files to a temporary table and invoke a query on it";

        public override bool IsMenuVisible(ClipboardContent content) => true;

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            var upstreamFileName = FileHelper.CreateUploadFileName("Table", "tsv");
            using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvData));

            await HandleSingleTextStreamAsync(csvStream, "tsv", upstreamFileName, sendNotification);
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            var upstreamFileName = FileHelper.CreateUploadFileName("Text", "txt");
            using var textStream = new MemoryStream(Encoding.UTF8.GetBytes(textData));

            await HandleSingleTextStreamAsync(textStream, AppConstants.UnknownFormat, upstreamFileName, sendNotification);
        }

        public override async Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification) => sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleFilesAsync)}");

        private async Task HandleSingleTextStreamAsync(Stream dataStream, string format, string upstreamFileName, SendNotification sendNotification)
        {
            using var databaseHelper = new KustoDatabaseHelper(m_appConfig.DefaultClusterConnectionString, m_appConfig.DefaultClusterDatabaseName);
            var uploadRes = await databaseHelper.TryUploadFileToEngineStagingAreaAsync(dataStream, upstreamFileName);

            if (!uploadRes.Success)
            {
                sendNotification(NotifcationTitle, $"Failed to upload file: {uploadRes.Error}");
                return;
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

            var tempTableName = KustoDatabaseHelper.CreateTempTableName();
            var createTableRes = await databaseHelper.TryCreateTableAync(tempTableName, schemaStr, ingestionBatchingTimeSeconds: 60, tableLifetimeDays: 3);

            if (!createTableRes.Success)
            {
                sendNotification(NotifcationTitle, createTableRes.Error);
                return;
            }

            var storageOptions = new StorageSourceOptions()
            {
                CompressionType = Kusto.Data.Common.DataSourceCompressionType.GZip,
            };

            var ingestionProperties = new KustoIngestionProperties()
            {
                DatabaseName = m_appConfig.DefaultClusterDatabaseName,
                TableName = tempTableName,
                Format = FileHelper.GetFormatFromExtension(format),
                IgnoreFirstRecord = false, // TODO consider if there's a way to detect that
            };


            var uploadBlobRes = await databaseHelper.TryDirectIngestBlobToTable(uploadRes.BlobUri, tempTableName, ingestionProperties, storageOptions);
            
            if (!uploadBlobRes.Success)
            {
                sendNotification(NotifcationTitle, uploadBlobRes.Error);
                return;
            }

            var query = $"['{tempTableName}']\n| take 100";
            if (!InlineQueryHelper.TryInvokeInlineQuery(m_appConfig, m_appConfig.DefaultClusterConnectionString, m_appConfig.DefaultClusterDatabaseName, query, out var error))
            {
                sendNotification(NotifcationTitle, error ?? "Unknown error.");
                return;
            }
        }
    }
}
