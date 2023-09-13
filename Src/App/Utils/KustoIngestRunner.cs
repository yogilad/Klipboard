using Kusto.Data;


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
