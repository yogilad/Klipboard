using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class QueueIngestWorker : WorkerBase
    {
        public QueueIngestWorker(WorkerCategory category, ISettings settings, object? icon = null)
            : base(category, ClipboardContent.None, settings, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Queue Data to Table";

        public override string GetToolTipText(ClipboardContent content) => "Queue clipboard tabular data or any number of files to a table";
    }
}
