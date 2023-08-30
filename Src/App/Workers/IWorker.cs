using Klipboard.Utils;

namespace Klipboard.Workers
{
    public interface IWorker
    {
        string Category { get; }
        object Icon { get; }

        bool IsVisible(ClipboardContent content);
        bool IsEnabled(ClipboardContent content);
        string GetText(ClipboardContent content);
        string GetToolTipText(ClipboardContent content);
        Task RunAsync(IClipboardHelper clipboardHelper);
    }
}
