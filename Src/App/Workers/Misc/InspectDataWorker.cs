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

        public override string GetMenuText(ClipboardContent content) => "Preview Clipboard Content";

        public override string GetToolTipText() => "Display a preview of Clipboard Data";

        public override bool IsMenuVisible() => AppConstants.DevMode;

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification, string? chosenOptions)
        {
            var contentBuilder = new StringBuilder();
            int length = csvData.Length / 1024;
            length += (csvData.Length % 1024 > 0) ? 1:0;

            contentBuilder.Append("Clipborad contains ~");
            contentBuilder.Append(length.ToString());
            contentBuilder.AppendLine("KBs of structured data:");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(csvData);

            ShowDialog(contentBuilder.ToString());
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification, string? chosenOptions)
        {
            var contentBuilder = new StringBuilder();
            int length = textData.Length / 1024;
            length += (textData.Length % 1024 > 0) ? 1 : 0;

            contentBuilder.Append("Clipborad contains ~");
            contentBuilder.Append(length.ToString());
            contentBuilder.AppendLine("KBs of unstructured text data:");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(textData);

            ShowDialog(contentBuilder.ToString());
        }

        public override async Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification, string? chosenOption)
        {
            var contentBuilder = new StringBuilder();

            contentBuilder.Append("Clipborad contains ");
            contentBuilder.Append(filesAndFolders.Count);
            contentBuilder.AppendLine(" files and folders:");
            contentBuilder.AppendLine();

            foreach (var file in filesAndFolders)
            {
                contentBuilder.AppendLine(file ?? "file item is null");
            }

            ShowDialog(contentBuilder.ToString());
        }

        public abstract void ShowDialog(string content);
    }
}
