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
            workers.Add(new QuickActionsWorker(WorkerCategory.QuickActions, quickActionIcon));
            workers.Add(new StructuredDataInlineQueryWorker(WorkerCategory.QuickActions, null));
            workers.Add(new FreeTextInlineQueryWorker(WorkerCategory.QuickActions, null));
            workers.Add(new ExternalDataQueryWorker(WorkerCategory.QuickActions, null));
            workers.Add(new TempTableWorker(WorkerCategory.QuickActions, null));
            workers.Add(new InspectDataWorker(WorkerCategory.QuickActions, null));

            // Actions 
            workers.Add(new DirectIngestWorker(WorkerCategory.Actions, null));
            workers.Add(new QueueIngestWorker(WorkerCategory.Actions, null));

            // Management 
            workers.Add(new OptionsWorker(WorkerCategory.Management, null));
            workers.Add(new ShareWorker(WorkerCategory.Management, null));
            workers.Add(new HelpWorker(WorkerCategory.Management, null));
            return workers;
        }

    }
}
