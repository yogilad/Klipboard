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

    public interface IWorker
    {
        ClipboardContent SupportedContent { get; }
        List<string>? SubMenuOptions { get; }
        bool IsMenuVisible();
        bool IsMenuEnabled(ClipboardContent content);
        string GetMenuText(ClipboardContent content);
        string GetToolTipText();
        Task HandleAsync(string? chosenOption);
        Task HandleCsvAsync(string csvData, string? chosenOption);
        Task HandleTextAsync(string textData, string? chosenOption);
        Task HandleFilesAsync(List<string> filesAndFolders, string? chosenOption);
        
        // Unused - consider removing if no need to handle streams
        Task HandleCsvStreamAsync(Stream csvData);
        Task HandleTextStreamAsync(Stream textData);
    }
}