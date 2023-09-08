using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using Kusto.Data.Linq;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Text;

namespace Klipboard.Workers
{
    public class ExternalDataQueryWorker : WorkerBase
    {
        private const string NotifcationTitle = "External Data Query";
        private const string UnknownFormat = "unknown";
        private const string TextLinesScheme = "(Line:string)";

        public ExternalDataQueryWorker(WorkerCategory category, AppConfig config, object? icon = null)
            : base(category, ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files, config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content) => $"Paste to External Data Query";

        public override bool IsMenuVisible(ClipboardContent content) => true;

        public override string GetToolTipText(ClipboardContent content) => "Upload clipboard tabular data , free text or a single file to a blob and invoke a an external data query on it";

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            var upstreamFileName = CreateUploadFileName("Table", "tsv");
            using var csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvData)); ;

            await HandleStreamAsync(csvStream, "tsv", upstreamFileName, sendNotification);
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            var upstreamFileName = CreateUploadFileName("Text", "txt");
            using var textStream = new MemoryStream(Encoding.UTF8.GetBytes(textData)); ;

            await HandleStreamAsync(textStream, UnknownFormat, upstreamFileName, sendNotification);
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
            var upsteramFileName = CreateUploadFileName(fileInfo.Name);
            var dataStream = new FileStream(file, FileMode.Open, FileAccess.Read);

            await HandleStreamAsync(dataStream, fileInfo.Extension, upsteramFileName, sendNotification);
        }

        public async Task HandleStreamAsync(Stream dataStream, string format, string upstreamFileName, SendNotification sendNotification)
        {
            using var kustoHelper = new KustoDatabaseHelper(m_appConfig.DefaultClusterConnectionString, m_appConfig.DefaultClusterDatabaseName);
            var uploadRes = await kustoHelper.TryUploadFileToEngineStagingAreaAsync(dataStream, upstreamFileName);

            if (!uploadRes.Success)
            {
                sendNotification(NotifcationTitle, $"Failed to upload file: {uploadRes.Error}");
                return;
            }

            string schemaStr = TextLinesScheme;

            format = format.ToLower().TrimStart(".");
            switch (format)
            {
                case UnknownFormat:
                    var csvSchemaRes = await kustoHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, format: "csv");
                    var tsvSchemaRes = await kustoHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, format: "tsv");
                    var multiJsonSchemaRes = await kustoHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, format: "multijson");
                    var jsonSchemaRes = await kustoHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, format: "json");

                    var curScheme = csvSchemaRes;
                    if (tsvSchemaRes.Success && (!curScheme.Success || curScheme.TableScheme.Columns.Count < tsvSchemaRes.TableScheme.Columns.Count))
                    {
                        curScheme = tsvSchemaRes;
                    }

                    if (multiJsonSchemaRes.Success && (!curScheme.Success || curScheme.TableScheme.Columns.Count < multiJsonSchemaRes.TableScheme.Columns.Count))
                    {
                        curScheme = multiJsonSchemaRes;
                    }

                    if (jsonSchemaRes.Success && (!curScheme.Success || curScheme.TableScheme.Columns.Count < jsonSchemaRes.TableScheme.Columns.Count))
                    {
                        curScheme = jsonSchemaRes;
                    }

                    if (curScheme.Success)
                    {
                        schemaStr = curScheme.TableScheme.ToString();
                        format = curScheme.format;
                        break;
                    }

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
                    var schemaRes = await kustoHelper.TryGetBlobSchemeAsync(uploadRes.BlobUri, format: format);
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

            if (format != UnknownFormat)
            {
                queryBuilder.Append("format = '");
                queryBuilder.Append(format);
                queryBuilder.Append("', ");
            }
            
            queryBuilder.AppendLine("ignoreFirstRecord = false);");
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

        private static string CreateUploadFileName(string filename)
        {
            var file = filename.SplitLast(".", out var extension);

            return CreateUploadFileName(file, extension);
        }

        private static string CreateUploadFileName(string filename, string extension)
        {
            var dt = DateTime.Now;
            var upsteramFileName = $"Klipboard_{filename}_{dt.Year}{dt.Month}{dt.Day}_{dt.Hour}{dt.Minute}{dt.Second}_{Guid.NewGuid().ToString()}.{extension}";
            return upsteramFileName;
        }
    }
}
