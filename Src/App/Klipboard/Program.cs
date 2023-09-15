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

            Application.Run();
        }

        public static List<WorkerUiConfig> CreateWorkers(ISettings settings)
        {
            var workers = new List<WorkerUiConfig>();

            workers.Add(new WorkerUiConfig(new QuickActionsUiWorker(settings), WorkerCategory.QuickActions, Icon: ResourceLoader.KustoColorIcon));
            workers.Add(new WorkerUiConfig(new StructuredDataInlineQueryWorker(settings), WorkerCategory.QuickActions));
            workers.Add(new WorkerUiConfig(new FreeTextInlineQueryWorker(settings), WorkerCategory.QuickActions));
            workers.Add(new WorkerUiConfig(new ExternalDataQueryWorker(settings), WorkerCategory.QuickActions));
            workers.Add(new WorkerUiConfig(new TempTableWorker(settings), WorkerCategory.QuickActions));
            workers.Add(new WorkerUiConfig(new InspectDataUiWorker(settings), WorkerCategory.QuickActions));

            // Actions
            workers.Add(new WorkerUiConfig(new DirectIngestWorker(settings), WorkerCategory.Actions));
            workers.Add(new WorkerUiConfig(new QueueIngestWorker(settings), WorkerCategory.Actions));

            // Management
            workers.Add(new WorkerUiConfig(new NewVersionWorker(settings), WorkerCategory.Management, Icon: ResourceLoader.DownloadIcon));
            workers.Add(new WorkerUiConfig(new SettingsUiWorker(settings), WorkerCategory.Management));
            workers.Add(new WorkerUiConfig(new HelpWorker(settings), WorkerCategory.Management));
            return workers;
        }
    }
}
