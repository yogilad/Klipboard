using System.Collections.Generic;

namespace CompanionCore
{
    /// <summary>
    /// Application Configuration 
    /// </summary>
    public class AppConfig
    {
        // List of known Kusto services
        public List<String> KustoConnectionStrings;

        private AppConfig()
        {
            KustoConnectionStrings = new List<String>();

            // My test cluster (until config can be read / written)
        }

        public static AppConfig TestAppConfig(string? appConfigFilePath = null)
        {
            var config = new AppConfig();

            config.KustoConnectionStrings.Add("https://kvcd8ed305830f049bbac1.northeurope.kusto.windows.net/");
            return config;
        }

        public static AppConfig ReadAppConfig(string? appConfigFilePath)
        {
            throw new NotImplementedException();
        }

        public static bool WriteAppConfig(string appConfigJsonPath)
        {
            return false;
        }
    }
}