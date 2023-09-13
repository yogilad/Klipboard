using System.Text;

using Klipboard.Utils;
using Klipboard.Utils.Interfaces;

namespace Klipboard.Workers
{
    public class InspectDataWorker : WorkerBase
    {
        private readonly IWorkerUi m_ui;

        public InspectDataWorker(WorkerCategory category, ISettings settings, IWorkerUi ui, object? icon = null)
            : base(category, ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files , settings, icon)
        {
            m_ui = ui;
        }

        public override string GetMenuText(ClipboardContent content) => "Inspect Clipboard Content";

        public override string GetToolTipText(ClipboardContent content) => "Display a preview of Clipboard Data";

        public override bool IsMenuVisible(ClipboardContent content) => AppConstants.DevMode;

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            var contentBuilder = new StringBuilder();
            int length = csvData.Length / 1024;
            length += (csvData.Length % 1024 > 0) ? 1:0;

            contentBuilder.Append("Clipborad contains ~");
            contentBuilder.Append(length.ToString());
            contentBuilder.AppendLine("KBs of structured data:");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(csvData);

            await m_ui.ShowDialog(contentBuilder.ToString());
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            var contentBuilder = new StringBuilder();
            int length = textData.Length / 1024;
            length += (textData.Length % 1024 > 0) ? 1 : 0;

            contentBuilder.Append("Clipborad contains ~");
            contentBuilder.Append(length.ToString());
            contentBuilder.AppendLine("KBs of unstructured text data:");
            contentBuilder.AppendLine();
            contentBuilder.AppendLine(textData);

            await m_ui.ShowDialog(contentBuilder.ToString());
        }

        public override async Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification)
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

            await m_ui.ShowDialog(contentBuilder.ToString());
        }

    }
}
