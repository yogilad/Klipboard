using Klipboard.Utils;
using Klipboard.Workers;


namespace Klipboard
{
    internal class SettingsUiWorker : SettingsWorker
    {
        public SettingsUiWorker(ISettings settings) : base(settings)
        {
        }

        public override async Task HandleAsync(SendNotification sendNotification)
        {
            ((Settings) m_settings).ShowDialog();
        }
    }
}
