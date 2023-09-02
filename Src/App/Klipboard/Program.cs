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

            var appConfig = AppConfigFile.CreateDebugConfig().ConfigureAwait(false).GetAwaiter().GetResult();
            var clipboardHelper = new ClipboardHelper();
            var workers = CreateAppWorkers(appConfig, icons);

            using var notifIcon = new NotificationIcon(appConfig, clipboardHelper, workers);
            Application.Run();
            AppConfigFile.Write(appConfig).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static IEnumerable<WorkerBase> CreateAppWorkers(AppConfig config, Dictionary<string, object> icons)
        {
            var workers = new List<WorkerBase>();

            // Quick Actions 
            icons.TryGetValue("QuickActions", out var quickActionIcon);
            workers.Add(new QuickActionsWorker(WorkerCategory.QuickActions, config, quickActionIcon));
            workers.Add(new StructuredDataInlineQueryWorker(WorkerCategory.QuickActions, config));
            workers.Add(new FreeTextInlineQueryWorker(WorkerCategory.QuickActions, config));
            workers.Add(new ExternalDataQueryWorker(WorkerCategory.QuickActions, config));
            workers.Add(new TempTableWorker(WorkerCategory.QuickActions, config));
            workers.Add(new InspectDataWorkerUx(WorkerCategory.QuickActions, config));

            // Actions 
            workers.Add(new DirectIngestWorker(WorkerCategory.Actions, config));
            workers.Add(new QueueIngestWorker(WorkerCategory.Actions, config));

            // Management 
            workers.Add(new OptionsWorker(WorkerCategory.Management, config));
            workers.Add(new ShareWorker(WorkerCategory.Management, config));
            workers.Add(new HelpWorker(WorkerCategory.Management, config));
            return workers;
        }
    }
}