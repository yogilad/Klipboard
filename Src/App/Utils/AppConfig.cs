using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Klipboard.Utils
{
    public enum QueryApp
    {
        Web,
        Desktop
    }

    public record Cluster(string ConnectionString, string DatabaseName);

    public record AppConfig(
        List<Cluster> KustoConnectionStrings,
        int DefaultClusterIndex = 0,
        QueryApp DefaultQueryApp = QueryApp.Web,
        bool StartAutomatically = false,
        string? PrependFreeTextQueriesWithKql = null
    )
    {
        [JsonIgnore]
        public Cluster ChosenCluster => KustoConnectionStrings[DefaultClusterIndex];

        public AppConfig()
            : this(new List<Cluster> { new ("https://help.kusto.windows.net", "Samples") })
        {
        }
    }

    #region App Config File

    public class AppConfigFile
    {
        public string ConfigDir { get; set; }
        public string ConfigPath { get; set; }

        private static readonly JsonSerializerOptions s_jsonOptions = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
        };

        public AppConfigFile()
        {
            ConfigDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Klipboard");
            ConfigPath = Path.Combine(ConfigDir, "config.json");
        }

        public AppConfigFile(string path)
        {
            ConfigDir = Path.GetDirectoryName(path);
            ConfigPath = path;
        }

        public static Task<AppConfig> CreateDebugConfig()
        {
            var AlgotecKQlParse =
                @"| parse-where Line with Timestamp:datetime ""-04:00 "" Level:string ""("" * "") "" ProcessName:string "" ("" ProcesId:int "","" ThreadId:int * "") "" EventText:string
| project-away Line
| extend Level = trim_end(""[ \\t]+"", Level)
| extend Level = iff(Level == ""NOTICE"", ""VERBOSE"", Level)";

            var myCluster = Environment.GetEnvironmentVariable("KUSTO_ENGINE") ?? "https://kvcd8ed305830f049bbac1.northeurope.kusto.windows.net/";
            var myDb = Environment.GetEnvironmentVariable("KUSTO_DATABASE") ?? "MyDatabase";
            var freeTextKQL = Environment.GetEnvironmentVariable("FREE_TEXT_KQL") ?? string.Empty;
            myCluster = myCluster.Trim().TrimEnd('/');

            var config = new AppConfig
            {
                KustoConnectionStrings = new List<Cluster>() { new (myCluster, myDb) },
                PrependFreeTextQueriesWithKql = freeTextKQL
            };

            return Task.FromResult(config);
        }

        public async Task<AppConfig> Read()
        {
            if (!File.Exists(ConfigPath))
            {
                return new AppConfig();
            }

            try
            {
                var jsonData = await File.ReadAllTextAsync(ConfigPath).ConfigureAwait(false);
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

        public async Task<bool> Write(AppConfig appConfig)
        {
            var options = new JsonSerializerOptions() { WriteIndented = true };
            var jsonData = JsonSerializer.Serialize<AppConfig>(appConfig, options);

            try
            {
                Directory.CreateDirectory(ConfigDir);
                await File.WriteAllTextAsync(ConfigPath, jsonData);
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
