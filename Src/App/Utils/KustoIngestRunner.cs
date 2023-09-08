using Kusto.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klipboard.Utils
{
    public class KustoIngestRunner
    {
        public KustoIngestRunner(string connectionString, string databaseName)
            : this(new KustoConnectionStringBuilder(connectionString), databaseName)
        { 
        }

        public KustoIngestRunner(KustoConnectionStringBuilder connectionString, string databaseName)
        { 
        }
    }
}
