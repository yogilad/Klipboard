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
        List<string>? SubMenuOptions { get; }
        bool IsMenuVisible();
        bool IsMenuEnabled(ClipboardContent content);
        string GetMenuText(ClipboardContent content);
        string GetToolTipText();
        Task HandleAsync(SendNotification sendNotification, string? chosenOption);
        Task HandleCsvAsync(string csvData, SendNotification sendNotification, string? chosenOption);
        Task HandleTextAsync(string textData, SendNotification sendNotification, string? chosenOption);
        Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification, string? chosenOption);
        
        // Unused - consider removing if no need to handle streams
        Task HandleCsvStreamAsync(Stream csvData, SendNotification sendNotification);
        Task HandleTextStreamAsync(Stream textData, SendNotification sendNotification);
    }
}