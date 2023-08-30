using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class DirectIngestWorker : WorkerBase
    {
        public DirectIngestWorker(WorkerCategory category, object? icon) 
            : base(category, icon, ClipboardContent.None)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            return "Paste Data to Table";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return "Upload clipboard tabular data or up to 100 files to a table"; ;
        }
    }
}
