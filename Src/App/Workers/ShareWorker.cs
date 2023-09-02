using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klipboard.Workers
{
    public class ShareWorker : WorkerBase
    {
        public ShareWorker(WorkerCategory category, AppConfig config, object? icon = null)
            : base(category, ClipboardContent.None, config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            return "Share Klipboard";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return string.Empty;
        }

        public override bool IsEnabled(ClipboardContent content)
        {
            return true;
        }

        public override Task HandleAsync(SendNotification sendNotification)
        {
            var subject = "Have You Tried Klipboard for Kusto?";
            var body = @"Hi, I'm using Klipboard for Kusto and I think you'd find it useful. You can get it in https://github.com/yogilad/Klipboard/blob/main/README.md";

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = $"mailto:?subject={subject}&body={body}",
                UseShellExecute = true
            });

            return Task.CompletedTask;
        }
    }
}
