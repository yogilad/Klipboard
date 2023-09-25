using Klipboard.Utils;


namespace Klipboard
{
    public class DirectIngestWorkerUx : IngestWorkerUxBase
    {
        public DirectIngestWorkerUx(ISettings settings) 
            : base(settings)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Direct Ingest Data to Table";

        public override string GetToolTipText() => "Direct ingest clipboard tabular data or any number of files to a table";

        public override bool IsMenuVisible() => AppConstants.DevMode;

        public override bool IsMenuEnabled(ClipboardContent content) => false;
    }
}
