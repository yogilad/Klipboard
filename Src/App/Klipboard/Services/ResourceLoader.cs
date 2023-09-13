using System.Reflection;


namespace Klipboard
{
    class ResourceLoader
    {
        static readonly Icon m_adxColorIcon;
        static readonly Icon m_adxBlueIcon;

        static ResourceLoader()
        {
            m_adxColorIcon = new Icon(LoadResourceAsStream("adx_color.ico", "Resources"));
            m_adxBlueIcon = new Icon(LoadResourceAsStream("adx_blue.ico", "Resources"));
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

        public static Icon GetIcon()
        {
            return m_adxColorIcon;
        }
    }
}
