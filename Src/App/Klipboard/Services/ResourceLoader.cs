using System.Reflection;


namespace Klipboard
{
    class ResourceLoader
    {
        public static readonly Icon KustoColorIcon;
        public static readonly Icon DownloadIcon;
        public static readonly Icon PrintIcon;

        static ResourceLoader()
        {
            KustoColorIcon = new Icon(LoadResourceAsStream("adx_color.ico", "Resources"));
            DownloadIcon = new Icon(LoadResourceAsStream("download-arrow.ico", "Resources"));
            PrintIcon = new Icon(LoadResourceAsStream("footprint.ico", "Resources"));
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
