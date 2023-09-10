using System.Windows;
using Klipboard.InteractiveWorkers.InpsectClipboardWorkerUx;
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

            //
            var icons = new Dictionary<string, object>()
            {
                { "QuickActions", ResourceLoader.GetIcon() }
            };

            var settings = Settings.Init().ConfigureAwait(false).GetAwaiter().GetResult();

            var clipboardHelper = new ClipboardHelper();
            var workers = CreateAppWorkers(settings, icons);

            using var notifIcon = new NotificationIcon(settings, clipboardHelper, workers);
            Application.Run();
        }

        public static IEnumerable<WorkerBase> CreateAppWorkers(Settings settings, Dictionary<string, object> icons)
        {
            var workers = new List<WorkerBase>();

            // Quick Actions
            if (!icons.TryGetValue("QuickActions", out var quickActionIcon))
            {
                throw new Exception("QuickActions icon not found");
            }

            workers.Add(new QuickActionsWorker(WorkerCategory.QuickActions, settings, quickActionIcon));
            workers.Add(new StructuredDataInlineQueryWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new FreeTextInlineQueryWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new ExternalDataQueryWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new TempTableWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new InspectDataWorkerUx(WorkerCategory.QuickActions, settings));

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
