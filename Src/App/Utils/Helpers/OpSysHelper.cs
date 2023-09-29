using System.Diagnostics;


namespace Klipboard.Utils
{
    public static class OpSysHelper
    {
        public static void InvokeLink(string link)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            });

            return;
        }
    }
}
