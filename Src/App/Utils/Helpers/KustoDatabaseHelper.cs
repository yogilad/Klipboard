﻿using System.IO.Compression;
using Azure.Storage.Blobs;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;
using Kusto.Ingest;


namespace Klipboard.Utils
{
    public class KustoDatabaseHelper : IDisposable
    {
        #region Members
        private static readonly HashSet<string> s_localhosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "localhost", "127.0.0.1", "::1", "[::1]" };
        
        private Lazy<ICslAdminProvider> m_engineAdminClient;
        private Lazy<ICslQueryProvider> m_engineQueryClient;
        private Lazy<IKustoIngestClient> m_directIngestClient;
        
        private readonly string m_databaseName = string.Empty;
        private readonly bool m_localhost = false;
        private readonly bool m_userHttps = false;
        #endregion

        #region Construction
        public KustoDatabaseHelper(string connectionString, string databaseName)
            : this(new KustoConnectionStringBuilder(connectionString), databaseName)
        {
        }

        public KustoDatabaseHelper(Cluster cluster)
            : this(cluster.ConnectionString, cluster.DatabaseName)
        {
        }

        public KustoDatabaseHelper(KustoConnectionStringBuilder connectionString, string databaseName)
        {
            m_localhost = s_localhosts.Contains(connectionString.Hostname);
            m_userHttps = connectionString.ConnectionScheme == "https";

            // TODO: This needs to be UX driven
            var engineKcsb = m_userHttps ?
                new KustoConnectionStringBuilder(connectionString).WithAadUserPromptAuthentication() :
                new KustoConnectionStringBuilder(connectionString.DataSource);

            engineKcsb.SetConnectorDetails(AppConstants.ApplicationName, AppConstants.ApplicationVersion.ToString(), sendUser: false);

            m_engineAdminClient = new Lazy<ICslAdminProvider>(() => KustoClientFactory.CreateCslAdminProvider(engineKcsb));
            m_engineQueryClient = new Lazy<ICslQueryProvider>(() => KustoClientFactory.CreateCslQueryProvider(engineKcsb));
            m_directIngestClient = new Lazy<IKustoIngestClient>(() => KustoIngestFactory.CreateDirectIngestClient(engineKcsb));
            m_databaseName = databaseName;
        }

        public void Dispose()
        {
            m_engineAdminClient?.Value.Dispose();
            m_engineAdminClient = null;
            m_engineQueryClient?.Value.Dispose();
            m_engineQueryClient = null;
        }
        #endregion

        #region Public APIs
        public async Task<(bool Success, string? BlobUri, string? Error)> TryUploadFileToEngineStagingAreaAsync(Stream dataStream, string upstreamFileName, FileFormatDefiniton formatDefintion, string? filePath = null)
        {
            var cmd = ".create tempstorage";

            if (m_localhost)
            {
                if (filePath == null)
                {
                    FileHelper.CreateTempFile(upstreamFileName, dataStream, out filePath);
                }

                return (Success: true, BlobUri: filePath, Error: null);
            }

            try
            {
                Logger.Log.Information("KustoDatabaseHelper.TryUploadFileToEngineStagingAreaAsync: Executing control command: {0}", cmd);

                using var res = await m_engineAdminClient.Value.ExecuteControlCommandAsync(m_databaseName, cmd);
                res.Read();

                var tempStorage = res.GetString(0);
                var resp = await TryUploadStreamAync(tempStorage, dataStream, upstreamFileName, formatDefintion);

                return resp;
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "KustoDatabaseHelper.TryUploadFileToEngineStagingAreaAsync: Failed to get engine staging account");
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
                Logger.Log.Information("KustoDatabaseHelper.TryGetBlobSchemeAsync: Executing control command: {0}", Logger.ObfuscateHiddentStrings(cmd));

                using var res = await m_engineQueryClient.Value.ExecuteQueryAsync(m_databaseName, cmd, new ClientRequestProperties());
                var tableScheme = new TableColumns();
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

                Logger.Log.Error("KustoDatabaseHelper.TryGetBlobSchemeAsync: Scheme detection returned no results");
                return (false, null, format, "Scheme detection returned no results");
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "KustoDatabaseHelper.TryGetBlobSchemeAsync: Failed to get engine staging account");
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

                setTableLifetimeCommand = $".alter table [\"{tableName}\"] policy auto_delete @'{{ \"ExpiryDate\" : \"{tableExpiryDate.Year:D4}-{tableExpiryDate.Month:D2}-{tableExpiryDate.Day:D2}\", \"DeleteIfNotEmpty\": true }}'";
            }

            createTableCommand = $".create table [\"{tableName}\"] {tableSchemeStr} with (docstring = \"{docstring}\")";

            // Create the table
            try
            {
                Logger.Log.Information("KustoDatabaseHelper.TryCreateTableAync: Executing control command: {0}", createTableCommand);

                using var res = await m_engineAdminClient.Value.ExecuteControlCommandAsync(m_databaseName, createTableCommand);
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "KustoDatabaseHelper.TryCreateTableAync: Failed to create table {0}", tableName);

                return (false, $"Failed to create table '{tableName}': {ex.Message}");
            }

            // Set ingestion time batching
            try
            {
                if (!string.IsNullOrWhiteSpace(alterIngestionBatchingCommand))
                {
                    Logger.Log.Information("KustoDatabaseHelper.TryCreateTableAync: Executing control command: {0}",alterIngestionBatchingCommand);

                    using var res = await m_engineAdminClient.Value.ExecuteControlCommandAsync(m_databaseName, alterIngestionBatchingCommand);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "KustoDatabaseHelper.TryCreateTableAync: Failed to create table {0}", tableName);

                return (false, $"Failed to create table '{tableName}': {ex.Message}");
            }

            // Set auto delete policy
            try
            {
                if (!string.IsNullOrWhiteSpace(setTableLifetimeCommand))
                {
                    Logger.Log.Information("KustoDatabaseHelper.TryCreateTableAync: Executing control command: {0}",setTableLifetimeCommand);

                    using var res = await m_engineAdminClient.Value.ExecuteControlCommandAsync(m_databaseName, setTableLifetimeCommand);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "KustoDatabaseHelper.TryCreateTableAync: Failed to create table {0}", tableName);

                return (false, $"Failed to create table '{tableName}': {ex.Message}");
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> TryDirectIngestStorageToTable(string blobUriOrFile, KustoIngestionProperties ingestionProperties, StorageSourceOptions sourceOptions)
        {
            try
            {
                ///////////////////////////////////////
                // Ingest files to an online cluster
                ///////////////////////////////////////
                if (!m_localhost)
                {
                    Logger.Log.Information("KustoDatabaseHelper.TryDirectIngestStorageToTable: Direct ingest from storage: {0}", Logger.ObfuscateHiddentStrings(blobUriOrFile));

                    var res = await m_directIngestClient.Value.IngestFromStorageAsync(blobUriOrFile, ingestionProperties, sourceOptions);
                    var ingestStatus = res.GetIngestionStatusBySourceId(sourceOptions.SourceId);

                    if (ingestStatus.Status == Kusto.Ingest.Status.Succeeded)
                    {
                        return (true, null);
                    }

                    return (false, $"Failed to ingest stream to Kusto: Status='{ingestStatus.Status}', ErrorCode='{ingestStatus.ErrorCode}', FailureStatus='{ingestStatus.FailureStatus}'");
                }
                ///////////////////////////////////////
                // Ingest files to a local cluster
                ///////////////////////////////////////
                else
                {
                    sourceOptions.IsLocalFileSystem = true;
                    sourceOptions.Compress = false;
                    var command =
                        CslCommandGenerator.GenerateTableIngestPullCommand(
                            ingestionProperties.TableName,
                            new[] { blobUriOrFile },
                            isAsync: false,
                            extensions: ingestionProperties.GetIngestionProperties());

                    Logger.Log.Information("KustoDatabaseHelper.TryDirectIngestStorageToTable: Executing control command: {0}", command);

                    using var resultReader = await m_engineAdminClient.Value.ExecuteControlCommandAsync(m_databaseName, command).ConfigureAwait(false);

                    resultReader.Read();

                    var hasErrors = resultReader.GetBoolean(3);
                    var error = hasErrors ? $"Ingest oprtation '{resultReader.GetString(4)}' failed" : null;

                    return (Success: !hasErrors, Error: error);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "KustoDatabaseHelper.TryDirectIngestStorageToTable: Failed to ingest stream to Kusto");

                return (false, $"Failed to ingest stream to Kusto: {ex.Message}");
            }
        }

        public static string CreateTempTableName()
        {
            var stamp = FileHelper.GenerateUniqueStamp();
            var upsteramFileName = $"{AppConstants.ApplicationName}TempTable_{stamp}";
            return upsteramFileName;
        }

        public IKustoIngestClient GetDirectIngestClient()
        {
            return m_directIngestClient.Value;
        }
        #endregion

        #region Private APIs
        private static async Task<(bool Success, string? BlobUri, string? Error)> TryUploadStreamAync(string blobContainerUriStr, Stream dataStream, string upstreamFileName, FileFormatDefiniton formatDefiniton)
        {
            var disposeOfStream = false;

            Logger.Log.Information("KustoDatabaseHelper.TryUploadStreamAsync");

            if (!formatDefiniton.DoNotCompress)
            {
                var compRes = await TryCreateZipStreamAsync(dataStream, upstreamFileName);
                if (!compRes.Success)
                {
                    return (false, null, compRes.Error);
                }

                dataStream = compRes.MemoryStream;
                disposeOfStream = true;
                upstreamFileName += ".zip";
            }

            try
            {
                var blobContainerUri = new Uri(blobContainerUriStr);
                var containerClient = new BlobContainerClient(blobContainerUri);
                var blobClient = containerClient.GetBlobClient(upstreamFileName);
                var blobRes = await blobClient.UploadAsync(dataStream, false);
                var blobResp = blobRes.GetRawResponse();

                if (blobResp.IsError)
                {
                    Logger.Log.Error("KustoDatabaseHelper.TryUploadStreamAsync: Failed to upload blob: {0}", blobResp.ReasonPhrase);
                    return (false, null, "Failed to upload blob: " + blobResp.ReasonPhrase);
                }

                var blobUri = blobContainerUriStr.Replace("?", $"/{upstreamFileName}?");
                return (true, blobUri, null);
            }
            finally
            {
                if (disposeOfStream)
                {
                    dataStream.Dispose();
                }
            }
        }

        private static async Task<(bool Success, MemoryStream? MemoryStream, string? Error)> TryCreateZipStreamAsync(Stream dataStream, string upsteramFileName)
        {
            Logger.Log.Information("KustoDatabaseHelper.TryUploadStreamAsync");

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
                Logger.Log.Error(ex, "KustoDatabaseHelper.TryUploadStreamAsync: Failed to compress memory stream");

                return (false, null, "Failed to compress memory stream: " + ex.Message);
            }
        }
        #endregion
    }
}
