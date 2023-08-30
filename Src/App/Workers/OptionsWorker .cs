using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class OptionsWorker : WorkerBase
    {
        public OptionsWorker(WorkerCategory category, object? icon)
            : base(category, icon, ClipboardContent.None)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            return "Options";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return string.Empty;
        }
    }
}
