using Klipboard.Utils;
using System.Diagnostics;

namespace Klipboard.Workers
{
    public class HelpWorker : WorkerBase
    {
        public HelpWorker(WorkerCategory category, object? icon)
            : base(category, icon, ClipboardContent.None)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            return "Help";
        }

        public override bool IsEnabled(ClipboardContent content)
        {
            return true;
        }

        public override Task HandleAsync(SendNotification sendNotification)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/yogilad/Klipboard/blob/main/README.md",
                UseShellExecute = true
            });

            return Task.CompletedTask;
        }
    }
}
