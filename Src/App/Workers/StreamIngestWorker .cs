using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class StreamIngestWorker : WorkerBase
    {
        public StreamIngestWorker(ISettings settings) 
            : base(ClipboardContent.None, settings)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Stream Data to Table";

        public override string GetToolTipText() => "Stream clipboard tabular data or any number of files to a table";

        public override bool IsMenuEnabled(ClipboardContent content) => true;
    }
}
