using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klipboard.Utils
{
    public class KustoClientHelper
    {
        private string m_connectionString = string.Empty;
        private string m_databaseName = string.Empty;

        public KustoClientHelper() { }

        public bool TryUploadFileToEngineStagingArea(string filePath, out string blobUri) 
        {
            throw new NotImplementedException();
        }

        public bool TryUploadFileToDataManagementStagingArea(string filePath, out string blobUri)
        {
            throw new NotImplementedException();
        }

        public bool TryGetBlobScheme(string blobUri, out string scheme)
        {
            throw new NotImplementedException();
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
    }
}
