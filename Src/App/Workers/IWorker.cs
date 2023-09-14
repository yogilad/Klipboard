using Klipboard.Utils;

namespace Klipboard.Workers
{
    public enum WorkerCategory
    {
        QuickActions,
        Actions,
        Management,
        Debug
    }

    public delegate void SendNotification(string title, string message);

    public interface IWorker
    {
        ClipboardContent SupportedContent { get; }
        bool IsMenuVisible();
        bool IsMenuEnabled(ClipboardContent content);
        string GetMenuText(ClipboardContent content);
        string GetToolTipText();
        Task HandleAsync(SendNotification sendNotification);
        Task HandleCsvAsync(string csvData, SendNotification sendNotification);
        Task HandleCsvStreamAsync(Stream csvData, SendNotification sendNotification);
        Task HandleTextAsync(string textData, SendNotification sendNotification);
        Task HandleTextStreamAsync(Stream textData, SendNotification sendNotification);
        Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification);
    }
}