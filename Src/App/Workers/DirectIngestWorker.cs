using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class DirectIngestWorker : WorkerBase
    {
        public DirectIngestWorker(WorkerCategory category, AppConfig config, object? icon = null) 
            : base(category, ClipboardContent.None, config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Paste Data to Table";

        public override string GetToolTipText(ClipboardContent content) => "Upload clipboard tabular data or up to 100 files to a table";
    }
}
