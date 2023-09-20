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

            var settings = Settings.Init().ConfigureAwait(false).GetAwaiter().GetResult();
            var clipboardHelper = new ClipboardHelper();

            var workers = CreateWorkers(settings);
            var notificationIcon = new NotificationIcon(workers, clipboardHelper);

            var x = new IngestForm("Queue Data For Ingestion", true);
            x.ShowDialog();

            Application.Run();
        }

        public static List<WorkerUxConfig> CreateWorkers(ISettings settings)
        {
            var workers = new List<WorkerUxConfig>();

            workers.Add(new WorkerUxConfig(new QuickActionsUxWorker(settings), WorkerCategory.QuickActions, Icon: ResourceLoader.KustoColorIcon));
            workers.Add(new WorkerUxConfig(new StructuredDataInlineQueryWorker(settings), WorkerCategory.QuickActions));
            workers.Add(new WorkerUxConfig(new FreeTextInlineQueryWorker(settings), WorkerCategory.QuickActions));
            workers.Add(new WorkerUxConfig(new ExternalDataQueryWorker(settings), WorkerCategory.QuickActions));
            workers.Add(new WorkerUxConfig(new TempTableWorker(settings), WorkerCategory.QuickActions));

            // Actions
            workers.Add(new WorkerUxConfig(new QueueIngestWorkerUx(settings), WorkerCategory.Actions));
            workers.Add(new WorkerUxConfig(new StreamIngestWorkerUx(settings), WorkerCategory.Actions));
            workers.Add(new WorkerUxConfig(new DirectIngestWorkerUx(settings), WorkerCategory.Actions, Icon: ResourceLoader.PrintIcon));
            workers.Add(new WorkerUxConfig(new InspectDataUxWorker(settings), WorkerCategory.Actions, Icon: ResourceLoader.PrintIcon));

            // Management
            workers.Add(new WorkerUxConfig(new NewVersionWorker(settings), WorkerCategory.Management, Icon: ResourceLoader.DownloadIcon));
            workers.Add(new WorkerUxConfig(new SettingsUxWorker(settings), WorkerCategory.Management));
            workers.Add(new WorkerUxConfig(new HelpWorker(settings), WorkerCategory.Management));
            return workers;
        }
    }
}
