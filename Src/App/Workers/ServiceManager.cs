using System.Diagnostics;
using Kusto.Data;

using Klipboard.Utils;

namespace Klipboard.Workers
{
    public class ServiceManager
    {
        private AppConfig m_appConfig;
        private Dictionary<string, KustoConnectionStringBuilder> m_serviceConnectionStrings;

        // Create a Service Manager 
        public ServiceManager(AppConfig appConfig)
        {
            m_appConfig = appConfig;
            m_serviceConnectionStrings = new Dictionary<string, KustoConnectionStringBuilder>();

            // Convert connections strings to KCSBs
            foreach (var connectionString in m_appConfig.KustoConnectionStrings)
            {
                TryAddService(connectionString);
            }
        }

        // Add a service from connection string
        public bool TryAddService(string serviceConnectionString)
        {
            try
            {
                var kcsb = new KustoConnectionStringBuilder(serviceConnectionString).WithAadUserPromptAuthentication();
                var hostTokens = kcsb.DataSource.Remove(0, 8).Split(".");
                var name = hostTokens[0];
                int i = 1;

                for (; i < hostTokens.Length; i++)
                {
                    if ("kusto".Equals(hostTokens[i], StringComparison.OrdinalIgnoreCase) ||
                        "kustomfa".Equals(hostTokens[i], StringComparison.OrdinalIgnoreCase))
                    {
                        break;
                    }
                }

                if (i > 1)
                {
                    name = $"{name} ({string.Join(".", hostTokens.Where((s, n) => { return n > 0 && n < i; }))})";
                }

                m_serviceConnectionStrings[name] = kcsb;
                return true;
            }
            catch
            {
                Debug.WriteLine("Failed to build kcsb from connection string {0}", serviceConnectionString);
                return false;
            }
        }

        // Remove a service from connection string
        public bool TryRemoveService(string serviceConnectionString)
        {
            return m_serviceConnectionStrings.Remove(serviceConnectionString);
        }

        // Get the kcsb for a service by service name
        public bool TryGetServiceKcsb(string serviceConnectionString, out KustoConnectionStringBuilder kcsb)
        {
            return m_serviceConnectionStrings.TryGetValue(serviceConnectionString, out kcsb);
        }

        // Get all services
        public IEnumerable<KustoConnectionStringBuilder> GetAllServices()
        {
            return m_serviceConnectionStrings.Values;
        }
    }
}