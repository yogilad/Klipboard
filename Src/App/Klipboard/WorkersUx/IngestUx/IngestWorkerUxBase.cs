using Klipboard.Utils;
using Klipboard.Workers;

namespace Klipboard
{
    public class IngestWorkerUxBase : IngestWorkerBase
    {
        public IngestWorkerUxBase(ISettings settings, INotificationHelper notificationHelper) 
            : base(settings, notificationHelper)
        {
        }
    }
}
