using System.Text;

using Klipboard.Utils;


namespace Klipboard.Workers
{
    public abstract class InspectDataWorker : WorkerBase
    {
        public InspectDataWorker(ISettings settings)
            : base(ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files, settings)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "View Clipboard Content";

        public override string GetToolTipText() => "Display a preview of Clipboard Data";

        public override bool IsMenuVisible() => true;

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification, string? chosenOptions)
        {
            ShowDialog("Table Data", ToSizeString(csvData.Length), csvData);
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification, string? chosenOptions)
        {
            ShowDialog("Free Text", ToSizeString(textData.Length), textData);
        }

        public override async Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification, string? chosenOption)
        {
            var contentBuilder = new StringBuilder();

            foreach (var file in filesAndFolders)
            {
                contentBuilder.AppendLine(file ?? "file item is null");
            }

            ShowDialog("Files", filesAndFolders.Count.ToString(), contentBuilder.ToString());
        }

        public abstract void ShowDialog(string contentType, string size, string content);

        public static string ToSizeString(double size)
        {
            const int KB = 1024;
            const int MB = KB* 1024;
            const int GB = MB * 1024;

            if (size < KB)
            {
                return $"{size:F0} bytes";
            }
            
            if (size < MB)
            {
                return $"{size/KB:F2} KB";
            }

            if (size < GB)
            {
                return $"{size/MB:F2} MBs";
            }

            return $"~{size/MB:F2} GBs";
        }
    }
}
