using System.IO.Compression;

using Azure.Storage.Blobs;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;

namespace Klipboard.Utils
{
    public class KustoDatabaseHelper : IDisposable
    {
        private ICslAdminProvider m_engineAdminClient;
        private ICslQueryProvider m_engineQueryClient;
        private string m_databaseName = string.Empty;

        public KustoDatabaseHelper(string connectionString, string databaseName)
            : this(new KustoConnectionStringBuilder(connectionString), databaseName)
        {
        }

        public KustoDatabaseHelper(KustoConnectionStringBuilder connectionString, string databaseName)
        {
            // TODO: This needs to be UX driven
            var engineKcsb = new KustoConnectionStringBuilder(connectionString).WithAadUserPromptAuthentication();
            engineKcsb.SetConnectorDetails(AppConstants.ApplicationName, AppConstants.ApplicationVersion, sendUser: false);

            m_engineAdminClient = KustoClientFactory.CreateCslAdminProvider(engineKcsb);
            m_engineQueryClient = KustoClientFactory.CreateCslQueryProvider(engineKcsb);
            m_databaseName = databaseName;
        }

        public void Dispose() 
        { 
            m_engineAdminClient?.Dispose();
            m_engineAdminClient = null;
            m_engineQueryClient?.Dispose();
            m_engineQueryClient = null;
        }

        public async Task<(bool Success, string? BlobUri, string? Error)> TryUploadFileToEngineStagingAreaAsync(Stream dataStream, string upstreamFileName) 
        {
            try 
            {
                using var res = await m_engineAdminClient.ExecuteControlCommandAsync(m_databaseName, ".create tempstorage");
                res.Read();
                
                var tempStorage = res.GetString(0);
                var resp = await TryUploadStreamAync(tempStorage, dataStream, upstreamFileName);
                
                return resp;
            }
            catch (Exception ex) 
            {
                return (false, null, "Failed to get engine staging account: " + ex.Message);
            }
        }

        public async Task<(bool Success, TableColumns? TableScheme, string? format, string? Error)> TryGetBlobSchemeAsync(string blobUri, string? format = null, bool? firstRowIsHeader = null)
        {
            var formatStr = (format != null) ? $", '{format}'" : string.Empty;
            var firstRowStr = (format != null && firstRowIsHeader != null) ? $", dynamic({{'UseFirstRowAsHeader':{firstRowIsHeader}}})" : string.Empty;
            var cmd = $"evaluate external_data_schema('{blobUri}'{formatStr}{firstRowStr})";

            try
            {
                using var res = await m_engineQueryClient.ExecuteQueryAsync(m_databaseName, cmd, new ClientRequestProperties());
                var tableScheme = new TableColumns(disableNameEscaping: true);
                var nameCol = res.GetOrdinal("ColumnName");
                var typeCol = res.GetOrdinal("ColumnType");

                while (res.Read())
                {
                    var colName = res.GetString(nameCol);
                    var typeStr = res.GetString(typeCol);

                    if (!KqlTypeHelper.TryGetTypeDedfinition(typeStr, out var typeDefintions))
                    {
                        return (false, null, format, $"Failed to get the type defintions for type '{typeStr}'");
                    }

                    tableScheme.Columns.Add((colName, typeDefintions));
                }

                if (tableScheme.Columns.Count > 0)
                {
                    return (true, tableScheme, format, null);
                }

                return (false, null, format, "Scheme detection returned no results");
            }
            catch (Exception ex)
            {
                return (false, null, format, "Failed to get engine staging account: " + ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> TryCreateTableAync(string tableName, string tableSceme, int? ingestionBatchingTimeSeconds = null, int? tableLifetimeDays = null)
        {
            var dt = DateTime.Now;
            string tableSchemeStr = tableSceme.ToString();
            string docstring;
            string createTableCommand;
            string alterIngestionBatchingCommand = string.Empty;
            string setTableLifetimeCommand = string.Empty;

            if (ingestionBatchingTimeSeconds != null)
            {
                ingestionBatchingTimeSeconds = ingestionBatchingTimeSeconds < 10 ? 10 : ingestionBatchingTimeSeconds;
                ingestionBatchingTimeSeconds = ingestionBatchingTimeSeconds > 600 ? 600 : ingestionBatchingTimeSeconds;
                var batchingTime = TimeSpan.FromSeconds(ingestionBatchingTimeSeconds.Value);

                alterIngestionBatchingCommand = $".alter-merge table [\"{tableName}\"] policy ingestionbatching '{{ \"MaximumBatchingTimeSpan\" : \"{batchingTime.ToString()}\" }}'";
            }

            if (tableLifetimeDays == null || tableLifetimeDays.Value <= 0)
            {
                docstring = $"Table created by Klipboard on {dt.ToShortDateString()} {dt.ToShortTimeString()}";
            }
            else
            {
                var tableExpiryDate = DateTime.Today.AddDays(tableLifetimeDays.Value + 1); // Round up to midnight the next day
                docstring = $"Temporary table created by klipboard on {dt.ToShortDateString()} {dt.ToShortTimeString()}, and is set for deletion on {tableExpiryDate.ToShortDateString()} {tableExpiryDate.ToShortTimeString()}";

                setTableLifetimeCommand = $".alter table [\"{tableName}\"] policy auto_delete @'{{ \"ExpiryDate\" : \"{tableExpiryDate.Year}-{tableExpiryDate.Month}-{tableExpiryDate.Day}\", \"DeleteIfNotEmpty\": true }}'";
            }

            createTableCommand = $".create table [\"{tableName}\"] {tableSchemeStr} with (docstring = \"{docstring}\")";

            // Create the table
            try
            {
                using var res = await m_engineAdminClient.ExecuteControlCommandAsync(m_databaseName, createTableCommand);
            }
            catch (Exception ex) 
            {
                return (false, $"Failed to create table '{tableName}': {ex.Message}");
            }

            // Set ingestion time batching 
            try
            {
                if (!string.IsNullOrWhiteSpace(alterIngestionBatchingCommand))
                {
                    using var res = await m_engineAdminClient.ExecuteControlCommandAsync(m_databaseName, alterIngestionBatchingCommand);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create table '{tableName}': {ex.Message}");
            }

            // Set auto delete policy
            try
            {
                if (!string.IsNullOrWhiteSpace(setTableLifetimeCommand))
                {
                    using var res = await m_engineAdminClient.ExecuteControlCommandAsync(m_databaseName, setTableLifetimeCommand);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Failed to create table '{tableName}': {ex.Message}");
            }

            return (true, null);
        }

        private static async Task<(bool Success, string? BlobUri, string? Error)> TryUploadStreamAync(string blobContainerUriStr, Stream dataStream, string upstreamFileName)
        {
            var compRes = await TryCreateZipStreamAsync(dataStream, upstreamFileName);
            if (!compRes.Success)
            {
                return (false, null, compRes.Error);
            }

            upstreamFileName += ".zip";
            
            using (compRes.MemoryStream)
            {
                var blobContainerUri = new Uri(blobContainerUriStr);
                var containerClient = new BlobContainerClient(blobContainerUri);
                var blobClient = containerClient.GetBlobClient(upstreamFileName);
                var blobRes = await blobClient.UploadAsync(compRes.MemoryStream, false);
                var blobResp = blobRes.GetRawResponse();

                if (blobResp.IsError)
                {
                    return (false, null, "Failed to upload blob: " + blobResp.ReasonPhrase);
                }

                var blobUri = blobContainerUriStr.Replace("?", $"/{upstreamFileName}?");
                return (true, blobUri, null);
            }
        }

        private static async Task<(bool Success, MemoryStream? MemoryStream, string? Error)> TryCreateZipStreamAsync(Stream dataStream, string upsteramFileName)
        {
            try
            {
                var memoryStream = new MemoryStream();

                if (dataStream.CanSeek)
                {
                    dataStream.Seek(0, SeekOrigin.Begin);
                }

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var zipFile = archive.CreateEntry(upsteramFileName);

                    using (var entryStream = zipFile.Open())
                    {
                        await dataStream.CopyToAsync(entryStream);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                return (true, memoryStream, null);
            }
            catch (Exception ex) 
            {
                return (false, null, "Failed to compress memory stream: " + ex.Message);
            }
        }
    }
}
