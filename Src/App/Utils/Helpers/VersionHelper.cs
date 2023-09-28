using System.Net;
using System.Text.RegularExpressions;
using Kusto.Cloud.Platform.Utils;


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

        public static bool TryParse(string versionStr, out Version? version)
        {
            var match = m_parser.Match(versionStr);

            if (!match.Success)
            {
                version = null;
                return false;
            }

            int.TryParse(match.Groups["major"].Value, out var major);
            int.TryParse(match.Groups["minor"].Value, out var minor);
            int.TryParse(match.Groups["build"].Value, out var build);

            version = new Version(major, minor, build);
            return true;
        }

        public string ToString(bool vPrefix=true)
        {
            return m_versionStr;
        }

        public static bool operator<(Version left, Version right)
        {
            if (left.Major < right.Major) return true;
            if (left.Minor < right.Minor) return true;
            if (left.Build < right.Build) return true;
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

        private static Timer? m_timer;

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

        private static async Task<Version?> TryGetLatestVersion()
        {
            var redirectUri = await TryGetRedirectedUri("https://github.com/yogilad/Klipboard/releases/latest");
            
            if (redirectUri == null)
            {
                return null;
            }

            redirectUri.SplitLast("/", out var versionStr);
            
            if (Version.TryParse(versionStr, out var onlineVersion))
            {
                return onlineVersion;
            }

            return null;
        }

        public static async Task<string?> TryGetRedirectedUri(string url)
        {
            var handler = new HttpClientHandler() 
            {
                AllowAutoRedirect = false
            };

            try
            {
                using (var client = new HttpClient(handler))
                using (var response = await client.GetAsync(url))
                {
                    if (response.StatusCode == HttpStatusCode.Found)
                    {
                        return response.Headers?.Location?.AbsoluteUri;
                    }
                }
            }
            catch (Exception) 
            { 
            }

            return null;
        }
    }
    #endregion
}
