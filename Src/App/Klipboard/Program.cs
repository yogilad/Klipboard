using Klipboard.Utils;
using Klipboard.Workers;

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

            var icons = new Dictionary<string, object>()
            {
                { "QuickActions", ResourceLoader.GetIcon() }
            };
            var settings = Settings.Init().ConfigureAwait(false).GetAwaiter().GetResult();
            var clipboardHelper = new ClipboardHelper();

            var notifIcon = new NotificationIcon();

            var notificationLogic = new Notificationlogic(notifIcon, settings, clipboardHelper, icons, WorkerBase.CreateWorkers(settings, icons).ToList());
            notificationLogic.Init();

            Application.Run();
        }

    }
}
