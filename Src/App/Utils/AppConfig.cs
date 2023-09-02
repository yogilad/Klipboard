using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Klipboard.Utils
{
    #region App Constats
    public static class AppConstants
    {
        // Debug Config
#if DEBUG
        public static readonly bool DevMode = true;
        public static readonly bool EnforceInlineQuerySizeLimits = true;
#else
        public bool DevMode = false;
        public static bool EnforceInlineQuerySizeLimits = true;
#endif

        // Program Constants
        public static readonly int MaxAllowedQueryLengthKB = 12;
        public static readonly int MaxAllowedQueryLength = MaxAllowedQueryLengthKB * 1024;
        public static readonly int MaxAllowedDataLengthKb = MaxAllowedQueryLengthKB * 10;
        public static readonly int MaxAllowedDataLength = MaxAllowedDataLengthKb * 1024;
    }
    #endregion

    #region App Config
    public class AppConfig
    {
        #region Connection Configuration
        // Kusto Configuration
        public HashSet<String> KustoConnectionStrings = new HashSet<String>();
        public String DefaultClusterConnectionString = string.Empty;
        public String DefaultClusterDatabaseName = string.Empty;
        #endregion

        #region Behavior Configuration
        // if set to true invokes queries in Kusto Explorer, otherwise invokes queries in Kusto Web Explorer
        public bool InvokeQueryInDesktopApp = false;

        // Auto start application when windows starts 
        public bool StartAutomatically = false;

        // Create free text queries with a default parse command
        public string PrepandFreeTextQueriesWithKQL = string.Empty;
        #endregion

        #region Construction
        internal AppConfig()
        {
        }
        #endregion 
    }
    #endregion

    #region App Config File
    public class AppConfigFile
    {
        public static readonly string s_configDir;
        public static readonly string s_configPath;

        static AppConfigFile()
        {
            s_configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Klipboard");
            s_configPath = Path.Combine(s_configDir, "config.json");
        }

        public static Task<AppConfig> CreateDebugConfig()
        {
            var config = new AppConfig();
            var myCluster = Environment.GetEnvironmentVariable("KUSTO_ENGINE") ?? "https://kvcd8ed305830f049bbac1.northeurope.kusto.windows.net/";
            var myDb = Environment.GetEnvironmentVariable("KUSTO_DATABASE") ?? "MyDatabase";
            var freeTextKQL = Environment.GetEnvironmentVariable("FREE_TEXT_KQL") ?? "| parse-where Line with Timestamp:datetime \"-04:00 \" Level:string \"(\" SomeNumber:int \") \" ProcessName:string \" (\" ProcesId:int \",\" ThreadId:int \"|\" Task:string \") ClientIP(\" IP:string \") SessionId(\" SessionId:int \") \" File:string \"(\" LineNumber:int \") \" EventText:string\r\n| project-away Line\r\n| extend Level = trim_end(\"[ \\\\t]+\", Level)\r\n| extend Level = iff(Level == \"NOTICE\", \"VERBOSE\", Level)\r\n";

            myCluster = myCluster.Trim().TrimEnd('/');
            config.DefaultClusterConnectionString = myCluster;
            config.DefaultClusterDatabaseName = myDb;
            config.KustoConnectionStrings.Add(myCluster);
            config.PrepandFreeTextQueriesWithKQL = freeTextKQL;

            return Task.FromResult(config);
        }

        public static async Task<AppConfig> Read()
        {
            if (!File.Exists(s_configPath))
            {
                return new AppConfig();
            }

            try
            {
                var jsonData = await File.ReadAllTextAsync(s_configPath);
                var appConfig = JsonSerializer.Deserialize<AppConfig>(jsonData);

                if (appConfig == null)
                {
                    Debug.WriteLine("File does not contain proper config:");
                    Debug.WriteLine(jsonData); 
                    appConfig = new AppConfig();
                }

                return appConfig;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to read config data: {ex.Message}");
                return new AppConfig();
            }
        }

        public static async Task<bool> Write(AppConfig appConfig)
        {
            var options = new JsonSerializerOptions() { WriteIndented = true };
            var jsonData = JsonSerializer.Serialize<AppConfig>(appConfig, options);

            try
            {
                Directory.CreateDirectory(s_configDir);
                await File.WriteAllTextAsync(s_configPath, jsonData);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to write config data: {ex.Message}");
                return false;
            }
        }
    }
    #endregion
}