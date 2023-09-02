using Klipboard.Utils;

namespace Klipboard.Workers
{
    public enum WorkerCategory
    {
        QuickActions,
        Actions,
        Management,
        Debug
    }
    public static class WorkerManager
    {
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
            workers.Add(new InspectDataWorker(WorkerCategory.QuickActions, config));

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
