using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Azure.Storage.Blobs;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net;
using Kusto.Data.Net.Client;
using Kusto.Ingest;

namespace Klipboard.Utils
{
    public class KustoClientHelper
    {
        private readonly KustoConnectionStringBuilder m_engineKcsb;
        private readonly KustoConnectionStringBuilder m_dmKcsb;
        private string m_databaseName = string.Empty;

        public KustoClientHelper(string connectionString, string databaseName)
            : this(new KustoConnectionStringBuilder(connectionString), databaseName)
        {
        }

        public KustoClientHelper(KustoConnectionStringBuilder connectionString, string databaseName)
        {
            // TODO: This needs to be UX driven
            m_engineKcsb = new KustoConnectionStringBuilder(connectionString).WithAadUserPromptAuthentication();
            m_engineKcsb.SetConnectorDetails("Klipboard", "0.0.0", sendUser: false);

            m_dmKcsb = new KustoConnectionStringBuilder(connectionString);
            m_dmKcsb.DataSource = "ingest-" + m_dmKcsb.DataSource;

            m_databaseName = databaseName;
        }

        public async Task<(bool Success, string? BlobUri, string? Error)> TryUploadFileToEngineStagingAreaAsync(Stream dataStream, string upstreamFileName) 
        {
            var engineClient = KustoClientFactory.CreateCslAdminProvider(m_engineKcsb);

            try 
            {
                var res = await engineClient.ExecuteControlCommandAsync(m_databaseName, ".create tempstorage");
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

        public async Task<(bool Success, string? blobUri)> TryUploadFileToDataManagementStagingAreaAsync(string filePath)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool Success, TableColumns? TableScheme, string? Error)> TryGetBlobSchemeAsync(string blobUri, string? format = null, bool? firstRowIsHeader = null)
        {
            var engineClient = KustoClientFactory.CreateCslQueryProvider(m_engineKcsb);
            var formatStr = (format != null) ? $", '{format}'" : string.Empty;
            var firstRowStr = (format != null && firstRowIsHeader != null) ? $", dynamic({{'UseFirstRowAsHeader':{firstRowIsHeader}}})" : string.Empty;
            var cmd = $"evaluate external_data_schema('{blobUri}'{formatStr}{firstRowStr})";

            try
            {
                var res = await engineClient.ExecuteQueryAsync(m_databaseName, cmd, new ClientRequestProperties());
                var tableScheme = new TableColumns();
                var nameCol = res.GetOrdinal("ColumnName");
                var typeCol = res.GetOrdinal("ColumnType");

                while (res.Read())
                {
                    var colName = res.GetString(nameCol);
                    var typeStr = res.GetString(typeCol);

                    if (!KqlTypeHelper.TryGetTypeDedfinition(typeStr, out var typeDefintions))
                    {
                        return (false, null, $"Failed to get the type defintions for type '{typeStr}'");
                    }

                    tableScheme.Columns.Add((colName, typeDefintions));
                }

                if (tableScheme.Columns.Count > 0)
                {
                    return (true, tableScheme, null);
                }

                return (false, null, "Scheme detection returned no results");
            }
            catch (Exception ex)
            {
                return (false, null, "Failed to get engine staging account: " + ex.Message);
            }
        }

        public async Task<bool> TryCreateTableAync(string tableName, string tableSceme)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> TryCreateTempTableAync(string tableName, string tableSceme, TimeSpan tableLifetime)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> TryDirectIngestFileAync(string tableName, string fileName, object fileSourceOptions, object ingestionProperties)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> TryQueueIngestFileAync(string tableName, string fileName, object fileSourceOptions, object ingestionProperties)
        {
            throw new NotImplementedException();
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
