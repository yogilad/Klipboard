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
            if (!OpSysHelper.TryAcquireSingleProcessLock(out var processLock)) 
            {
                return;
            }

            using (processLock)
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();

                var settings = Settings.Init().ConfigureAwait(false).GetAwaiter().GetResult();
                var clipboardHelper = new ClipboardHelper();
                var notificationHelper = new NotificationHelper();

                var workers = CreateWorkers(settings, notificationHelper);
                using var notificationIcon = new NotificationIcon(workers, clipboardHelper, notificationHelper);

                VersionHelper.StartPolling();

                Application.Run();

                VersionHelper.StopPolling();
            }
        }

        private static List<WorkerUxConfig> CreateWorkers(ISettings settings, INotificationHelper notificationHelper)
        {
            var workers = new List<WorkerUxConfig>();

            workers.Add(new WorkerUxConfig(new QuickActionsUxWorker(settings, notificationHelper), WorkerCategory.QuickActions, Icon: ResourceLoader.KustoColorIcon));
            workers.Add(new WorkerUxConfig(new StructuredDataInlineQueryWorker(settings, notificationHelper), WorkerCategory.QuickActions));
            workers.Add(new WorkerUxConfig(new FreeTextInlineQueryWorker(settings, notificationHelper), WorkerCategory.QuickActions));
            workers.Add(new WorkerUxConfig(new ExternalDataQueryWorker(settings, notificationHelper), WorkerCategory.QuickActions));
            workers.Add(new WorkerUxConfig(new TempTableWorker(settings, notificationHelper), WorkerCategory.QuickActions));

            // Actions
            workers.Add(new WorkerUxConfig(new QueueIngestWorkerUx(settings, notificationHelper), WorkerCategory.Actions));
            workers.Add(new WorkerUxConfig(new StreamIngestWorkerUx(settings, notificationHelper), WorkerCategory.Actions));
            workers.Add(new WorkerUxConfig(new DirectIngestWorkerUx(settings, notificationHelper), WorkerCategory.Actions, Icon: ResourceLoader.DevModeIcon));
            workers.Add(new WorkerUxConfig(new InspectDataUxWorker(settings, notificationHelper), WorkerCategory.Actions));

            // Management
            workers.Add(new WorkerUxConfig(new NewVersionWorker(settings, notificationHelper), WorkerCategory.Management, Icon: ResourceLoader.DownloadIcon));
            workers.Add(new WorkerUxConfig(new SettingsUxWorker(settings, notificationHelper), WorkerCategory.Management));
            workers.Add(new WorkerUxConfig(new HelpWorker(settings, notificationHelper), WorkerCategory.Management));
            return workers;
        }
    }
}
