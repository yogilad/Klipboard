using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class StreamIngestWorkerUi : IngestWorkerUiBase
    {
        public StreamIngestWorkerUi(ISettings settings) 
            : base(settings)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Stream Data to Table";

        public override string GetToolTipText() => "Stream clipboard tabular data or any number of files to a table";

        public override bool IsMenuEnabled(ClipboardContent content) => AppConstants.DevMode;
    }
}
