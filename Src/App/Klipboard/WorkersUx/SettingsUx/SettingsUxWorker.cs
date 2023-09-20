using Klipboard.Utils;
using Klipboard.Workers;


namespace Klipboard
{
    internal class SettingsUxWorker : SettingsWorker
    {
        public SettingsUxWorker(ISettings settings) : base(settings)
        {
        }

        public override async Task HandleAsync(SendNotification sendNotification, string? chosenOption)
        {
            ((Settings) m_settings).ShowDialog();
        }
    }
}
