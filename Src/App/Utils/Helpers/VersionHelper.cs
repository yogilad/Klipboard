using Azure.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Klipboard.Utils
{
    #region Version Class
    public class Version
    {
        private static readonly Regex m_parser = new Regex("\\s*[vV]?(?<major>[\\d]+)\\.(?<minor>[\\d]+)\\.(?<build>[\\d]+)\\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private string m_versionStr;

        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Build { get; private set; }

        public Version(int major, int minor, int build)
        {
            Set(major, minor, build);
        }

        public Version(string version)
        {
            int major = 0;
            int minor = 0;
            int build = 0;
            var match = m_parser.Match(version);
            
            if (match.Success)
            {
                int.TryParse(match.Groups["major"].Value, out major);
                int.TryParse(match.Groups["minor"].Value, out minor);
                int.TryParse(match.Groups["build"].Value, out build);
            }

            Set(major, minor, build);
        }

        public string ToString(bool vPrefix=true)
        {
            return m_versionStr;
        }

        public static bool operator<(Version left, Version right)
        {
            if (left.Major < right.Major) return true;
            if (left.Minor< right.Minor) return true;
            if (left.Build< right.Build) return true;
            return false;
        }

        public static bool operator >(Version left, Version right)
        {
            if (left.Major > right.Major) return true;
            if (left.Minor > right.Minor) return true;
            if (left.Build > right.Build) return true;
            return false;
        }

        private void Set(int major, int minor, int build)
        {
            Major = major >= 0 ? major : 0;
            Minor = minor >= 0 ? minor : 0;
            Build = build >= 0 ? build : 0;

            m_versionStr = $"v{Major}.{Minor}.{Build}";
        }
    }
    #endregion

    #region Version Helper Class
    public static class VersionHelper
    {
        public static bool HasNewVersion { get; private set; }
        public static Version LatestVersion { get; private set; }

        private static Timer m_timer;

        static VersionHelper()
        {
            HasNewVersion = false;
            LatestVersion = AppConstants.ApplicationVersion;
        }

        public static void StartPolling()
        {
            m_timer = new Timer(async (_) => await CheckForNewVersion(), null, TimeSpan.Zero, TimeSpan.FromDays(1));
        }

        public static void StopPolling() 
        {
            m_timer?.Dispose();
            m_timer = null;
        }

        public static async Task<bool> CheckForNewVersion()
        {
            var newVersion = await TryGetLatestVersion().ConfigureAwait(false);

            if (newVersion != null && newVersion > AppConstants.ApplicationVersion)
            {
                HasNewVersion = true;
                LatestVersion = newVersion;
            }

            return HasNewVersion;
        }

        private static async Task<Version> TryGetLatestVersion()
        {
            return new Version("0.1.1");
        }
    }
    #endregion
}
