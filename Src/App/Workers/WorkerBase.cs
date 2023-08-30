using Klipboard.Utils;

namespace Klipboard.Workers
{
    public abstract class WorkerBase
    {
        public abstract WorkerCategory Category { get; }
        public abstract object? Icon { get; }

        public abstract bool IsVisible(ClipboardContent content);
        public abstract bool IsEnabled(ClipboardContent content);
        public abstract string GetText(ClipboardContent content);
        public abstract string GetToolTipText(ClipboardContent content);
        public abstract Task RunAsync(IClipboardHelper clipboardHelper);
    }
}
