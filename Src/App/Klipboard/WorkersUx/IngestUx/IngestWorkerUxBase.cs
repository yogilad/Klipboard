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

        public override bool IsMenuVisible() => AppConstants.DevMode;

        public override bool IsMenuEnabled(ClipboardContent content) => AppConstants.DevMode && content == ClipboardContent.Files;

        public override async Task HandleFilesAsync(List<string> filesAndFolders, string? chosenOption)
        {
            using var ux = new IngestForm(GetMenuText(ClipboardContent.Files), devMode: AppConstants.DevMode);

            ux.ShowDialog();

            if (ux.UserSelection.UserConfirmedIngestion)
            {
                await base.HandleFilesAsync(filesAndFolders, ux.UserSelection, traceEvent: null, reportProgress: null);
            }
        }
    }
}
