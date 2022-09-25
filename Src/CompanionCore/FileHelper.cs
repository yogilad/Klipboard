using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanionCore
{
    public static class FileHelper
    {
        private static readonly char[] c_pathSeperators = new char[] { '\\', '/'};
        
        public static bool TryGetFileTypeFromPath(string path, out string fileType, out bool compressed)
        {
            fileType = string.Empty;
            compressed = false;

            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var index = path.LastIndexOfAny(c_pathSeperators);
            var filename = path.Substring(index + 1);
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
                    fileType = tokens[pos - 1];
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
                fileType = tokens[pos];
                compressed = false;
                return true;
            }

            return false;
        }
    }
}
