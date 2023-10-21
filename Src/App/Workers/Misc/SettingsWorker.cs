using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class SettingsWorker : WorkerBase
    {
        public SettingsWorker(ISettings settings, INotificationHelper notificationHelper)
            : base(ClipboardContent.None, settings, notificationHelper)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Settings";

        public override string GetToolTipText() => string.Empty;

        public override bool IsMenuEnabled(ClipboardContent content) => true;

        public override bool IsMenuVisible() => true;
    }
}
