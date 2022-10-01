using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CompanionCore
{
    #region App Config
    public class AppConfig
    {
        public HashSet<String> KustoConnectionStrings { set; get; }

        internal AppConfig()
        {
            KustoConnectionStrings = new HashSet<String>();
        }
    }
    #endregion

    #region App Config File
    public class AppConfigFile
    {
        [JsonIgnore]
        public static readonly string s_configDir;
        
        [JsonIgnore]
        public static readonly string s_configPath;
        // List of known Kusto services

        static AppConfigFile()
        {
            s_configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Klipboard");
            s_configPath = Path.Combine(s_configDir, "config.json");
        }

        public static async Task<AppConfig> ReadTest()
        {
            var config = await Read();

            config.KustoConnectionStrings.Add("https://kvcd8ed305830f049bbac1.northeurope.kusto.windows.net/");
            return config;
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