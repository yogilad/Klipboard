using Klipboard.Utils;
using System.Diagnostics;

namespace Klipboard.Workers
{
    public class NewVersionWorker : WorkerBase
    {
        public NewVersionWorker(ISettings settings, INotificationHelper notificationHelper)
            : base(ClipboardContent.None, settings, notificationHelper)
        {
        }

        public override string GetMenuText(ClipboardContent content) => $"Version {VersionHelper.LatestVersion} Available For Download!";

        public override string GetToolTipText() => string.Empty;

        public override bool IsMenuEnabled(ClipboardContent content) => VersionHelper.HasNewVersion;

        public override bool IsMenuVisible() => VersionHelper.HasNewVersion;

        public override async Task HandleAsync(string? chosenOption)
        {
            OpSysHelper.InvokeLink("https://github.com/yogilad/Klipboard/releases");
        }
    }
}
