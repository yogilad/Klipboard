using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class ExternalDataQueryWorker : WorkerBase
    {
        public ExternalDataQueryWorker(WorkerCategory category, object? icon)
            : base(category, icon, ClipboardContent.None)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            var contentToConsider = content & SupportedContent;
            var contentStr = contentToConsider == ClipboardContent.None ? "Data" : content.ToString();
            return $"Paste {contentStr} to External Data Query";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return "Upload clipboard tabular data or a single file to a blob and invoke a an external data query on it";
        }
    }
}
