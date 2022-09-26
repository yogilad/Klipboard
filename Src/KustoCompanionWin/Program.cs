using System.Windows;

namespace KustoCompanionWin
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

            //var appConfig = CompanionCore.AppConfig.ReadAppConfig("");
            //var kustoWorker = new KustoWorker.ServiceManager(appConfig);

            using var notifIcon = new NotificationIcon();
            Application.Run(new Form1());
        }
    }
}