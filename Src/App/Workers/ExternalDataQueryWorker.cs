using System.Text;
using Kusto.Cloud.Platform.Utils;

using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class ExternalDataQueryWorker : WorkerBase
    {
        private const string NotifcationTitle = "External Data Query";

        public ExternalDataQueryWorker(WorkerCategory category, ISettings mSettings, object? icon = null)
            : base(category, ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files, mSettings, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content) => $"Paste to External Data Query";

        public override bool IsMenuVisible() => true;

        public override string GetToolTipText(ClipboardContent content) => "Upload clipboard tabular data , free text or a single file to a blob and invoke a an external data query on it";

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            var upstreamFileName = FileHelper.CreateUploadFileName("Table", "tsv");
            using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvData));

            await HandleStreamAsync(csvStream, "tsv", upstreamFileName, sendNotification);
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            var upstreamFileName = FileHelper.CreateUploadFileName("Text", "txt");
            using var textStream = new MemoryStream(Encoding.UTF8.GetBytes(textData));

            await HandleStreamAsync(textStream, AppConstants.UnknownFormat, upstreamFileName, sendNotification);
        }

        public override async Task HandleFilesAsync(List<string> files, SendNotification sendNotification)
        {
            if (files.Count > 1)
            {
                sendNotification(NotifcationTitle, "External data query only supports a single file.");
            }

            var file = files[0];
            var fileInfo = new FileInfo(file);

            if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                sendNotification(NotifcationTitle, "External data query does not support directories.");
                return;
            }

            if (!fileInfo.Exists)
            {
                sendNotification(NotifcationTitle, $"File '{file}' does not exist.");
            }

            var dt = DateTime.Now;
            var upsteramFileName = FileHelper.CreateUploadFileName(fileInfo.Name);
            var dataStream = new FileStream(file, FileMode.Open, FileAccess.Read);

            await HandleStreamAsync(dataStream, fileInfo.Extension, upsteramFileName, sendNotification);
        }

        private async Task HandleStreamAsync(Stream dataStream, string format, string upstreamFileName, SendNotification sendNotification)
        {
            using var databaseHelper = new KustoDatabaseHelper(m_settings.GetConfig().ChosenCluster);
            var uploadRes = await databaseHelper.TryUploadFileToEngineStagingAreaAsync(dataStream, upstreamFileName);

            if (!uploadRes.Success)
            {
                sendNotification(NotifcationTitle, $"Failed to upload file: {uploadRes.Error}");
                return;
            }

            string schemaStr = AppConstants.TextLinesSchemaStr;

            format = format.ToLower().TrimStart(".");
            switch (format)
            {
                case AppConstants.UnknownFormat:
                    var autoDetectRes = await databaseHelper.TryAutoDetectTextBlobScheme(uploadRes.BlobUri);

                    if (autoDetectRes.Success)
                    {
                        schemaStr = autoDetectRes.Schema.ToString();
                        format = autoDetectRes.Format;
                        break;
                    }

                    schemaStr = AppConstants.TextLinesSchemaStr;
                    format = "txt";
                    break;

                case "csv":
                case "tsv":
                case "tsve":
                case "json":
                case "multijson":
                case "orc":
                case "parquet":
                case "avro":
                    var schemaRes = await databaseHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, format: format);
                    if (schemaRes.Success)
                    {
                        schemaStr = schemaRes.TableScheme.ToString();
                        break;
                    }

                    format = "txt";
                    break;

                default:
                    format = "txt";
                    break;
            }

            var blobPath = uploadRes.BlobUri.SplitFirst("?", out var blboSas);
            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine("let Klipboard =");
            queryBuilder.Append("externaldata");
            queryBuilder.AppendLine(schemaStr);
            queryBuilder.AppendLine("[");
            queryBuilder.Append(" '");
            queryBuilder.Append(blobPath);
            queryBuilder.Append("' h'?");
            queryBuilder.Append(blboSas);
            queryBuilder.AppendLine("'");
            queryBuilder.AppendLine("]");
            queryBuilder.Append("with(");

            if (format != AppConstants.UnknownFormat)
            {
                queryBuilder.Append("format = '");
                queryBuilder.Append(format);
                queryBuilder.Append("', ");
            }

            queryBuilder.AppendLine("ignoreFirstRecord = false);");
            queryBuilder.AppendLine("Klipboard");

            var appConfig = m_settings.GetConfig();
            if (format == "txt" && !string.IsNullOrWhiteSpace(appConfig.PrependFreeTextQueriesWithKql))
            {
                queryBuilder.AppendLine(appConfig.PrependFreeTextQueriesWithKql);
            }

            queryBuilder.AppendLine("| take 100");

            var query = queryBuilder.ToString();
            if (!InlineQueryHelper.TryInvokeInlineQuery(appConfig, appConfig.ChosenCluster.ConnectionString, appConfig.ChosenCluster.DatabaseName, query, out var error))
            {
                sendNotification(NotifcationTitle, error ?? "Unknown error.");
                return;
            }
        }
    }
}
