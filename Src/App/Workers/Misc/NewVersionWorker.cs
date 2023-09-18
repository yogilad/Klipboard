using Klipboard.Utils;

namespace Klipboard.Workers
{
    public class NewVersionWorker : WorkerBase
    {
        public NewVersionWorker(ISettings settings)
            : base(ClipboardContent.None, settings)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "New Version Available!";

        public override string GetToolTipText() => string.Empty;

        public override bool IsMenuEnabled(ClipboardContent content) => AppConstants.DevMode;

        public override bool IsMenuVisible() => AppConstants.DevMode;

        public override async Task HandleAsync(SendNotification sendNotification, string? chosenOption)
        {
        }
    }
}
