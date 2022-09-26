using System.Drawing;
using System.IO;
using System.Reflection;

namespace KustoCompanionWin
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

        private static Stream LoadResourceAsStream(string resourceName, string folder = null, string name_space = nameof(KustoCompanionWin))
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourcePath = $"{name_space}.{folder}.{resourceName}";
            Stream stream = assembly.GetManifestResourceStream(resourcePath);

            return stream;
        }

        public static Icon GetIcon()
        {
            return m_adxColorIcon;
        }
    }
}
