using System.Diagnostics;

using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class ShareWorker : WorkerBase
    {
        public ShareWorker(ISettings settings)
            : base(ClipboardContent.None, settings)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Share Klipboard";

        public override string GetToolTipText() => string.Empty;

        public override bool IsMenuEnabled(ClipboardContent content) => true;

        public override bool IsMenuVisible() => true;

        public override async Task HandleAsync(SendNotification sendNotification, string? chosenOption)
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
