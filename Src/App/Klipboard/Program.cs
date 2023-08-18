using System.Windows;
using Klipboard.Utils;

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

            var appConfig = AppConfigFile.ReadStub().ConfigureAwait(false).GetAwaiter().GetResult();
            //var kustoWorker = new KustoWorker.ServiceManager(appConfig);

            using var notifIcon = new NotificationIcon();
            Application.Run(new Form1());
            AppConfigFile.Write(appConfig).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}