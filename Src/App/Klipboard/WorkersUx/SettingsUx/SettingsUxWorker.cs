using Klipboard.Utils;
using Klipboard.Workers;


namespace Klipboard
{
    internal class SettingsUxWorker : SettingsWorker
    {
        public SettingsUxWorker(ISettings settings, INotificationHelper notificationHelper) : base(settings, notificationHelper)
        {
        }

        public override async Task HandleAsync(string? chosenOption)
        {
            ((Settings) m_settings).ShowDialog();
        }
    }
}
