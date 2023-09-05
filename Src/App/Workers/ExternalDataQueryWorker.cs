using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using Kusto.Data.Linq;
using System.Net;
using System.Text;

namespace Klipboard.Workers
{
    public class ExternalDataQueryWorker : WorkerBase
    {
        public string NotifcationTitle => "External Data Query";

        public ExternalDataQueryWorker(WorkerCategory category, AppConfig config, object? icon = null)
            : base(category, ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files, config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content) => $"Paste to External Data Query";

        public override bool IsMenuVisible(ClipboardContent content) => true;

        public override string GetToolTipText(ClipboardContent content) => "Upload clipboard tabular data , free text or a single file to a blob and invoke a an external data query on it";

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleCsvAsync)}");
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleTextAsync)}");
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
            var upsteramFileName = $"Klipboard_{fileInfo.Name.SplitLast(".", out var _)}_{dt.Year}{dt.Month}{dt.Day}_{dt.Hour}{dt.Minute}{dt.Second}_{Guid.NewGuid().ToString()}{fileInfo.Extension}";
            var dataStream = new FileStream(file, FileMode.Open);

            await HandleStreamAsync(dataStream, fileInfo.Extension, upsteramFileName, sendNotification);
        }

        public async Task HandleStreamAsync(Stream dataStream, string format, string upstreamFileName, SendNotification sendNotification)
        {
            var kustoHelper = new KustoClientHelper(m_appConfig.DefaultClusterConnectionString, m_appConfig.DefaultClusterDatabaseName);
            var uploadRes = await kustoHelper.TryUploadFileToEngineStagingAreaAsync(dataStream, upstreamFileName);

            if (!uploadRes.Success)
            {
                sendNotification(NotifcationTitle, $"Failed to upload file: {uploadRes.Error}");
                return;
            }

            string schemaStr = "(Line:string)";
            format = format.ToLower().TrimStart(".");
            switch (format)
            {
                case "csv":
                case "tsv":
                case "tsve":
                case "json":
                case "multijson":
                case "orc":
                case "parquet":
                case "avro":
                    var schemaRes = await kustoHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, format: format);
                    if (schemaRes.Success)
                    {
                        schemaStr = schemaRes.TableScheme.ToString();
                    }
                    else
                    {
                        format = "txt";
                    }

                    break;

                default:
                    format = "txt";
                    break;
            }

            var queryBuilder = new StringBuilder();

            queryBuilder.AppendLine("let Klipboard =");
            queryBuilder.Append("externaldata");
            queryBuilder.AppendLine(schemaStr);
            queryBuilder.AppendLine("[");
            queryBuilder.Append(" h'");
            queryBuilder.Append(uploadRes.BlobUri);
            queryBuilder.AppendLine("'");
            queryBuilder.AppendLine("]");
            queryBuilder.Append("with(format = '");
            queryBuilder.Append(format);
            queryBuilder.AppendLine("', ignoreFirstRecord = false);");
            queryBuilder.AppendLine("Klipboard");

            if (format == "txt" && !string.IsNullOrWhiteSpace(m_appConfig.PrepandFreeTextQueriesWithKQL))
            {
                queryBuilder.AppendLine(m_appConfig.PrepandFreeTextQueriesWithKQL);
            }

            queryBuilder.AppendLine("| take 100");

            var query = queryBuilder.ToString();
            if (!InlineQueryHelper.TryInvokeInlineQuery(m_appConfig, m_appConfig.DefaultClusterConnectionString, m_appConfig.DefaultClusterDatabaseName, query, out var error))
            {
                sendNotification(NotifcationTitle, error ?? "Unknown error.");
                return;
            }
        }
    }
}
