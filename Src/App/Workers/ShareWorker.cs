using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using Kusto.Ingest.Exceptions;
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
        public ShareWorker(WorkerCategory category, ISettings settings, object? icon = null)
            : base(category, ClipboardContent.None, settings, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Share Klipboard";

        public override string GetToolTipText(ClipboardContent content) => string.Empty;

        public override bool IsMenuEnabled(ClipboardContent content) => true;

        public override bool IsMenuVisible(ClipboardContent content) => true;

        public override async Task HandleAsync(SendNotification sendNotification)
        {
            var subject = "Have You Tried Klipboard for Kusto?";
            var body = 
@"Hi, 

I'm using Klipboard for Kusto and I think you'd find it useful. 
You can get it from https://github.com/yogilad/Klipboard/blob/main/README.md";

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = $"mailto:?subject={subject}&body={Uri.EscapeUriString(body)}",
                UseShellExecute = true
            });
        }
    }
}
