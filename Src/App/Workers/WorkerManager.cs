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
        public static IEnumerable<WorkerBase> CreateAppWorkers(AppConfig config, Dictionary<WorkerCategory, object> icons)
        {
            // Management 
            var workers = new List<WorkerBase>();

            workers.Add(new OptionsWorker(WorkerCategory.Management, null));
            workers.Add(new ShareWorker(WorkerCategory.Management, null));
            workers.Add(new HelpWorker(WorkerCategory.Management, null));
            return workers;
        }

    }
}
