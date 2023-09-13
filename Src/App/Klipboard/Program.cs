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

            var workerUis = new Dictionary<string, IWorkerUi>()
            {
                { "InspectData", new InspectFormHandler() }
            };

            var settings = Settings.Init().ConfigureAwait(false).GetAwaiter().GetResult();
            var clipboardHelper = new ClipboardHelper();

            var notifIcon = new NotificationIcon();

            var workers = CreateWorkers(settings, icons, workerUis).ToList();
            var notificationLogic = new Notificationlogic(notifIcon, settings, clipboardHelper, icons, workers);
            notificationLogic.Init();

            Application.Run();
        }

        public static IEnumerable<IWorker> CreateWorkers(ISettings settings, Dictionary<string, object> icons, Dictionary<string, IWorkerUi> workerUis)
        {
            var workers = new List<IWorker>();

            // Quick Actions
            if (!icons.TryGetValue("QuickActions", out var quickActionIcon))
            {
                throw new Exception("QuickActions icon not found");
            }
            if (!workerUis.TryGetValue("InspectData", out var inspectDataUi))
            {
                throw new Exception("InspectDataUi not found");
            }

            workers.Add(new QuickActionsUiWorker(WorkerCategory.QuickActions, settings, quickActionIcon));
            workers.Add(new StructuredDataInlineQueryWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new FreeTextInlineQueryWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new ExternalDataQueryWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new TempTableWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new InspectDataWorker(WorkerCategory.QuickActions, settings, inspectDataUi));

            // Actions
            workers.Add(new DirectIngestWorker(WorkerCategory.Actions, settings));
            workers.Add(new QueueIngestWorker(WorkerCategory.Actions, settings));

            // Management
            workers.Add(new ShareWorker(WorkerCategory.Management, settings));
            workers.Add(new HelpWorker(WorkerCategory.Management, settings));
            return workers;
        }
    }
}
