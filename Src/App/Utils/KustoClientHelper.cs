using System;
using System.Collections.Generic;
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

        public KustoClientHelper(KustoConnectionStringBuilder connectionString, string databaseName)
        {
            m_engineKcsb = new KustoConnectionStringBuilder(connectionString);
            m_dmKcsb = new KustoConnectionStringBuilder(connectionString);
            m_databaseName = databaseName;

            m_dmKcsb.DataSource = "ingest-" + m_dmKcsb.DataSource;
        }

        public bool TryUploadFileToEngineStagingArea(string filePath, string upstreamFileName, out string? blobUri, out string? error) 
        {
            var engineClient = KustoClientFactory.CreateCslAdminProvider(m_engineKcsb);

            error = null;
            blobUri = null;

            try 
            {
                var res = engineClient.ExecuteControlCommand(m_databaseName, ".create tempstorage");
                res.Read();
                
                var tempStorage = res.GetString(0);
                if (TryUploadFromFile(tempStorage, filePath, upstreamFileName, out blobUri, out error))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex) 
            { 
                error = "Failed to get engine staging account: " + ex.Message;
                return false;
            }
        }

        public bool TryUploadFileToDataManagementStagingArea(string filePath, out string blobUri)
        {
            throw new NotImplementedException();
        }

        public bool TryGetBlobScheme(string blobUri, out TableColumns? tableScheme, out string? error, string? format = null, bool? firstRowIsHeader = null)
        {
            var engineClient = KustoClientFactory.CreateCslQueryProvider(m_engineKcsb);
            var formatStr = (format != null) ? $", '{format}'" : string.Empty;
            var firstRowStr = (format != null && firstRowIsHeader != null) ? $", dynamic({{'UseFirstRowAsHeader':{firstRowIsHeader}}})" : string.Empty;
            var cmd = $"evaluate external_data_schema('{blobUri}'{formatStr}{firstRowStr})";

            try
            {
                var res = engineClient.ExecuteQuery(m_databaseName, cmd, new ClientRequestProperties());
                tableScheme = new TableColumns();
                var nameCol = res.GetOrdinal("ColumnName");
                var typeCol = res.GetOrdinal("ColumnType");

                while (res.Read())
                {
                    var colName = res.GetString(nameCol);
                    var typeStr = res.GetString(typeCol);

                    if (!KqlTypeHelper.TryGetTypeDedfinition(typeStr, out var typeDefintions))
                    {
                        error = $"Failed to get the type defintions for type '{typeStr}'";
                        return false;
                    }

                    tableScheme.Columns.Add((colName, typeDefintions));
                }

                error = null;
                return tableScheme.Columns.Count > 0;
            }
            catch (Exception ex)
            {
                tableScheme = null;
                error = "Failed to get engine staging account: " + ex.Message;
                return false;
            }
        }

        public bool TryCreateTable(string tableName, string tableSceme)
        {
            throw new NotImplementedException();
        }

        public bool TryCreateTempTable(string tableName, string tableSceme, TimeSpan tableLifetime)
        {
            throw new NotImplementedException();
        }
        public bool TryDirectIngestFile(string tableName, string fileName, object fileSourceOptions, object ingestionProperties)
        {
            throw new NotImplementedException();
        }

        public bool TryQueueIngestFile(string tableName, string fileName, object fileSourceOptions, object ingestionProperties)
        {
            throw new NotImplementedException();
        }

        private static bool TryUploadFromFile(string blobContainerUriStr, string localFilePath, string upstreamFileName, out string? blobUri, out string? error)
        {
            if (!TryCreateZipStream(localFilePath, upstreamFileName, out var memoryStream, out error))
            {
                blobUri = null;
                return false;
            }

            upstreamFileName += ".zip";
            
            using (memoryStream)
            {
                var blobContainerUri = new Uri(blobContainerUriStr);
                var containerClient = new BlobContainerClient(blobContainerUri);
                var blobClient = containerClient.GetBlobClient(upstreamFileName);
                var res = blobClient.Upload(memoryStream, false).GetRawResponse();

                if (res.IsError)
                {
                    blobUri = null;
                    error = res.ReasonPhrase;
                    return false;
                }

                blobUri = blobContainerUriStr.Replace("?", $"/{upstreamFileName}?");
                error = null;
                return true;
            }
        }

        private static bool TryCreateZipStream(string filePath, string upsteramFileName, out MemoryStream memoryStream, out string? error)
        {
            try
            {
                memoryStream = new MemoryStream();

                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    var zipFile = archive.CreateEntry(upsteramFileName);

                    using (var entryStream = zipFile.Open())
                    using (var fileStream = new FileStream(filePath, FileMode.Open))
                    {
                        fileStream.CopyTo(entryStream);
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                error = null;
                return true;
            }
            catch (Exception ex) 
            {
                error = ex.Message;
                memoryStream = null;
                return false;
            }
        }
    }
}
