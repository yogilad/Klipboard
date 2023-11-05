using System.Diagnostics;
using System.Reflection;


namespace Klipboard
{
    class ResourceLoader
    {
        public static readonly Icon KustoColorIcon;
        public static readonly Icon DownloadIcon;
        public static readonly Icon DevModeIcon;

        static ResourceLoader()
        {
#if DEBUG
            KustoColorIcon = new Icon(LoadResourceAsStream("adx_grey.ico", "Resources"));
#else
            if (Debugger.IsAttached)
            {
                KustoColorIcon = new Icon(LoadResourceAsStream("adx_grey.ico", "Resources"));
            }
            else
            {
                KustoColorIcon = new Icon(LoadResourceAsStream("adx_color.ico", "Resources"));
            }
#endif
            DownloadIcon = new Icon(LoadResourceAsStream("download.ico", "Resources"));
            DevModeIcon = new Icon(LoadResourceAsStream("wand.ico", "Resources"));
        }

        private static Stream LoadResourceAsStream(string resourceName, string folder, string name_space = nameof(Klipboard))
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourcePath = $"{name_space}.{folder}.{resourceName}";
            Stream? stream = assembly.GetManifestResourceStream(resourcePath);

            if (stream == null) 
            {
                throw new Exception("Resouce missing");
            }

            return stream;
        }
    }
}
