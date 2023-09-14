using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class DirectIngestWorker : WorkerBase
    {
        public DirectIngestWorker(ISettings settings) 
            : base(ClipboardContent.None, settings)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Paste Data to Table";

        public override string GetToolTipText() => "Upload clipboard tabular data or up to 100 files to a table";
    }
}
