using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class DirectIngestWorker : WorkerBase
    {
        public DirectIngestWorker(ISettings settings) 
            : base(ClipboardContent.None, settings)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Direct Ingest Data to Table";

        public override string GetToolTipText() => "Direct ingest clipboard tabular data or any number of files to a table";

        public override bool IsMenuEnabled(ClipboardContent content) => AppConstants.DevMode;
    }
}
