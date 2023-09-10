using Klipboard.Utils;

namespace Klipboard.Workers;

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
    WorkerCategory Category { get; }
    object? Icon { get; }
    bool IsMenuVisible(ClipboardContent content);
    bool IsMenuEnabled(ClipboardContent content);
    string GetMenuText(ClipboardContent content);
    string GetToolTipText(ClipboardContent content);
    Task HandleAsync(SendNotification sendNotification);
    Task HandleCsvAsync(string csvData, SendNotification sendNotification);
    Task HandleCsvStreamAsync(Stream csvData, SendNotification sendNotification);
    Task HandleTextAsync(string textData, SendNotification sendNotification);
    Task HandleTextStreamAsync(Stream textData, SendNotification sendNotification);
    Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification);
}
