using Klipboard.Utils;
using Klipboard.Workers;


namespace Klipboard
{
    public abstract class IngestWorkerUxBase : IngestWorkerBase
    {
        public IngestWorkerUxBase(ISettings settings, INotificationHelper notificationHelper) 
            : base(settings, notificationHelper)
        {
        }

        public override async Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification, string? chosenOption)
        {
            using var ux = new IngestForm(GetMenuText(ClipboardContent.Files), devMode: true);

            ux.ShowDialog();

            if (ux.UserSelection.UserConfirmedIngestion)
            {
                await base.HandleFilesAsync(filesAndFolders, ux.UserSelection, traceEvent: null, reportProgress: null);
            }
        }
    }
}
