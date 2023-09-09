using Kusto.Cloud.Platform.Utils;
using Kusto.Data.Common;
using System.Text.RegularExpressions;

namespace Klipboard.Utils
{
    public static class FileHelper
    {
        private static readonly char[] s_pathSeperators = new char[] { '\\', '/'};
        private static Dictionary<string, int> s_monthStrToInt;
        
        static FileHelper()
        {
            s_monthStrToInt = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            s_monthStrToInt.Add("jan", 1);
            s_monthStrToInt.Add("feb", 2);
            s_monthStrToInt.Add("mar", 3);
            s_monthStrToInt.Add("apr", 4);
            s_monthStrToInt.Add("may", 5);
            s_monthStrToInt.Add("jun", 6);
            s_monthStrToInt.Add("jul", 7);
            s_monthStrToInt.Add("aug", 8);
            s_monthStrToInt.Add("sep", 9);
            s_monthStrToInt.Add("oct", 10);
            s_monthStrToInt.Add("nov", 11);
            s_monthStrToInt.Add("dec", 12);
        }
        
        /// <summary>
        /// Extracts the file extension. 
        /// If the file ends with .zip or .gz, extract the precceding token as the file extension. 
        /// </summary>
        /// <param name="nameOrPath">file path or name</param>
        /// <param name="fileExtension">detected extension</param>
        /// <param name="compressed">detected compression</param>
        /// <returns></returns>
        public static bool TryGetFileExtensionFromPath(string nameOrPath, out string fileExtension, out bool compressed)
        {
            fileExtension = string.Empty;
            compressed = false;

            if (string.IsNullOrWhiteSpace(nameOrPath))
            {
                return false;
            }

            var index = nameOrPath.LastIndexOfAny(s_pathSeperators);
            var filename = nameOrPath.Substring(index + 1);
            var tokens = filename.Split(".");
            var pos = tokens.Length - 1;

            if (tokens.Length < 2)
            {
                return false;
            }

            if (tokens[pos].Equals("zip") || tokens[pos].Equals("gz"))
            {
                if (tokens.Length >= 3)
                {
                    fileExtension = tokens[pos - 1];
                    compressed = true;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (tokens.Length >= 2)
            {
                fileExtension = tokens[pos];
                compressed = false;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the file creation time rounded to a whole day 
        /// </summary>
        /// <param name="path">a path to a local file </param>
        /// <param name="creationDay">The file creation time rounded down to the day</param>
        /// <returns></returns>
        public static bool TryGetFileCreationDay(string path, out DateTime? creationDay)
        {
            creationDay = null;

            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var creationTime = File.GetCreationTimeUtc(path);
                creationDay = new DateTime(creationTime.Year, creationTime.Month, creationTime.Day);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the file creation time rounded to a whole day 
        /// </summary>
        /// <param name="path">a path to a local file </param>
        /// <param name="modificationDay">The file last modification rounded down to the day</param>
        /// <returns></returns>
        public static bool TryGetFileModificationDay(string path, out DateTime? modificationDay)
        {
            modificationDay = null;

            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            try
            {
                var creationTime = File.GetLastWriteTimeUtc(path);
                modificationDay = new DateTime(creationTime.Year, creationTime.Month, creationTime.Day);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the file creation time by looking for various regex match groups in the file name 
        /// Supported Groups (case sensitive): 
        ///  EPOC - time in seconds since 1970
        ///  YYYY - year in 4 digits
        ///  YY - year in 2 Digits since 1970
        ///  MM - Month in 1 or 2 digits
        ///  MON - Month as a 3 letter acronym 
        ///  DD - day in 1 or 2 digits
        /// </summary>
        /// <param name="nameOrPath"></param>
        /// <param name="expression"></param>
        /// <param name="creationTime"></param>
        /// <returns></returns>
        public static bool TryGetFileCreationTimeFromName(string nameOrPath, Regex expression, out DateTime? creationTime)
        {
            var index = nameOrPath.LastIndexOfAny(s_pathSeperators);
            var filename = nameOrPath.Substring(index + 1);
            var groups = expression.Match(filename).Groups;

            creationTime = null;

            // Check EPOC Time Group
            if (groups.TryGetValue("EPOC_SEC", out var match))
            {
                try
                {
                    var epocTime = long.Parse(match.Value);
                    var tempTime = DateTimeOffset.FromUnixTimeSeconds(epocTime).DateTime;
                    
                    creationTime = creationTime = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, 0, 0, 0, DateTimeKind.Utc);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            if (groups.TryGetValue("EPOC_MSEC", out match))
            {
                try
                {
                    var epocTime = long.Parse(match.Value);
                    var tempTime = DateTimeOffset.FromUnixTimeMilliseconds(epocTime).DateTime;

                    creationTime = creationTime = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, 0, 0, 0, DateTimeKind.Utc);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            int year = -1;
            int month = 1;
            int day = 1;

            // A year must exist either in 4 or 2 digits
            if (groups.TryGetValue("YYYY", out match))
            {
                try
                {
                    year = int.Parse(match.Value);
                }
                catch
                {
                    return false;
                }
            }
            else if (groups.TryGetValue("YY", out match))
            {
                try
                {
                    year = int.Parse(match.Value);
                    year += (year >= 70) ? 1900 : 2000;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // No year found 
                return false;
            }

            // An optional month in 2 digits or 3 letter acronym
            if (groups.TryGetValue("MM", out match))
            {
                try
                {
                    month = int.Parse(match.Value);
                }
                catch
                {
                    return false;
                }
            }
            else if (groups.TryGetValue("MON", out match))
            {
                try
                {
                    month = s_monthStrToInt[match.Value];
                }
                catch
                {
                    return false;
                }
            }

            // An optional day in 2 digit
            if (groups.TryGetValue("DD", out match))
            {
                try
                {
                    day = int.Parse(match.Value);
                }
                catch
                {
                    return false;
                }
            }

            // Attempt to create time time itself
            try
            {
                creationTime = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
                return true;
            }
            catch
            {
            }

            return false;
        }

        public static string CreateUploadFileName(string filename)
        {
            var file = filename.SplitLast(".", out var extension);

            return CreateUploadFileName(file, extension);
        }

        public static string CreateUploadFileName(string filename, string extension)
        {
            var dt = DateTime.Now;
            var upsteramFileName = $"{AppConstants.ApplicationName}_{filename}_{dt.Year}{dt.Month}{dt.Day}_{dt.Hour}{dt.Minute}{dt.Second}_{Guid.NewGuid().ToString()}.{extension}";
            return upsteramFileName;
        }

        public static DataSourceFormat GetFormatFromExtension(string extension)
        {
            extension = extension.TrimStart('.').ToLower();

            switch(extension)
            {
                case "csv":
                    return DataSourceFormat.csv;

                case "tsv":
                    return DataSourceFormat.tsv;

                case "json":
                    return DataSourceFormat.multijson;

                case "avro":
                    return DataSourceFormat.avro;

                case "parquet":
                    return DataSourceFormat.parquet;
                
                case "orc":
                    return DataSourceFormat.orc;

                default:
                    return DataSourceFormat.txt;
            }
        }

        public static IEnumerable<string> ExpandDropFileList(List<string> dropFiles, string? extension = null)
        {
            var wildCardFileMatcher = "*";
            var enumOptions = new EnumerationOptions()
            { 
                MatchCasing = MatchCasing.CaseInsensitive,
                MatchType = MatchType.Simple,
                RecurseSubdirectories = true,
                ReturnSpecialDirectories = false,
            };

            if (extension != null)
            {
                extension = extension.StartsWith(".") ? extension : $".{extension}";
                wildCardFileMatcher = $"*{extension}";
            }

            foreach(var path in dropFiles)
            {
                var fileInfo = new FileInfo(path);

                if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    foreach (var subFile in Directory.GetFiles(path, wildCardFileMatcher, enumOptions))
                    {
                        yield return subFile;
                    }
                }

                if (!fileInfo.Exists)
                {
                    continue;
                }

                if (extension != null && !path.EndsWith(extension)) 
                {
                    continue;
                }

                yield return path;
            }

            yield break;
        }
    }
}
