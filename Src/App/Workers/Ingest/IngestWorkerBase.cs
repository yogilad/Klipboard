using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class IngestWorkerBase : WorkerBase
    {
        public IngestWorkerBase(ISettings settings, INotificationHelper notificationHelper) 
            : base(ClipboardContent.None, settings, notificationHelper)
        {
        }
    }
}
