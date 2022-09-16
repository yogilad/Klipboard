using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kusto.Cloud.Platform.Data;
using Kusto.Data;
using Kusto.Data.Common;
using Kusto.Data.Net.Client;

namespace KustoWorker
{
    public class QueryWorker
    {
        KustoConnectionStringBuilder m_kcsb;
        ClientRequestProperties m_crp;
        ICslQueryProvider m_kustoClient;

        public QueryWorker(KustoConnectionStringBuilder kcsb)
        {
            m_kcsb = kcsb;
            m_crp = new ClientRequestProperties();
            m_kustoClient = KustoClientFactory.CreateCslQueryProvider(kcsb);
        }

        // List all databases in the service 
        public async Task<List<string>> ListServiceDatabasesAsync()
        {
            var dbList = new List<string>();

            try
            {
                var result = await m_kustoClient.ExecuteQueryAsync("", ".show databases", m_crp).ConfigureAwait(false);
                
                dbList.AddRange(result.ToStringColumn("DatabaseName"));
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error while reading service {0} databases:\n{1}", m_kcsb.DataSource, ex.Message);
            }

            return dbList;
        }
    }
}
