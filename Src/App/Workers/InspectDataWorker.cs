using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using System.Text;

namespace Klipboard.Workers
{
    public abstract class InspectDataWorker : WorkerBase
    {
        public InspectDataWorker(WorkerCategory category, AppConfig config, object? icon = null)
            : base(category, ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files , config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            return $"Inspect Clipboard Content ({content})";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return "Display a preview of Clipboard Data"; ;
        }

        public override bool IsEnabled(ClipboardContent content)
        {
            return content != ClipboardContent.None;
        }


        public abstract Task ShowContent(string clipboardContent);

        public override Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            var contentBuilder = new StringBuilder();
            int length = csvData.Length / 1024;
            length += (csvData.Length % 1024 > 0) ? 1:0;

            contentBuilder.Append("Clipborad contains ~");
            contentBuilder.Append(length.ToString());
            contentBuilder.AppendLine("KBs of structured data:");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(csvData);

            ShowContent(contentBuilder.ToString()).ConfigureAwait(false).ResultEx();

            return Task.CompletedTask;
        }

        public override Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            var contentBuilder = new StringBuilder();
            int length = textData.Length / 1024;
            length += (textData.Length % 1024 > 0) ? 1 : 0;

            contentBuilder.Append("Clipborad contains ~");
            contentBuilder.Append(length.ToString());
            contentBuilder.AppendLine("KBs of unstructured text data:");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(textData);

            ShowContent(contentBuilder.ToString()).ConfigureAwait(false).ResultEx();

            return Task.CompletedTask;
        }

        public override Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification)
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

            ShowContent(contentBuilder.ToString()).ConfigureAwait(false).ResultEx();

            return Task.CompletedTask;
        }

    }
}
