using System.Windows;
using Klipboard.Utils;
using Workers;

namespace Klipboard
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var appConfig = AppConfigFile.CreateDebugConfig().ConfigureAwait(false).GetAwaiter().GetResult();
            var clipboardHelper = new ClipboardHelper();
            var workerItems = new List<IWorker>();

            using var notifIcon = new NotificationIcon(appConfig, clipboardHelper, workerItems);
            Application.Run();
            AppConfigFile.Write(appConfig).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}