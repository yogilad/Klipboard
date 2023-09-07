using Klipboard.Utils;
using System.Diagnostics;

namespace Klipboard.Workers
{
    public class HelpWorker : WorkerBase
    {
        public HelpWorker(WorkerCategory category, AppConfig config, object? icon = null)
            : base(category, ClipboardContent.None, config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Help";

        public override bool IsMenuEnabled(ClipboardContent content) => true;

        public override bool IsMenuVisible(ClipboardContent content) => true;

        public override async Task HandleAsync(SendNotification sendNotification)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/yogilad/Klipboard/blob/main/README.md",
                UseShellExecute = true
            });

            return;
        }
    }
}
