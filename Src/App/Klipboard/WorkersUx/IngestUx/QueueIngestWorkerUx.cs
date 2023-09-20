using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class QueueIngestWorkerUx : IngestWorkerUxBase
    {
        public QueueIngestWorkerUx(ISettings settings)
            : base(settings)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Queue Data to Table";

        public override string GetToolTipText() => "Queue clipboard tabular data or any number of files to a table";

        public override bool IsMenuVisible() => true;
        
        public override bool IsMenuEnabled(ClipboardContent content) => content != ClipboardContent.None;
    }
}
