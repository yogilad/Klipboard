using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class QueueIngestWorker : WorkerBase
    {
        public QueueIngestWorker(ISettings settings)
            : base(ClipboardContent.None, settings)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Queue Data to Table";

        public override string GetToolTipText() => "Queue clipboard tabular data or any number of files to a table";

        public override bool IsMenuVisible() => true;
        
        public override bool IsMenuEnabled(ClipboardContent content) => content != ClipboardContent.None;
    }
}
