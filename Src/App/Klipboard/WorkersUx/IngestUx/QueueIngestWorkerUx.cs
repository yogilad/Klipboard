using Kusto.Data;
using Kusto.Ingest;
using Klipboard.Utils;



namespace Klipboard.Workers
{
    public class QueueIngestWorkerUx : IngestWorkerUxBase
    {
        public QueueIngestWorkerUx(ISettings settings, INotificationHelper notificationHelper)
            : base(settings, notificationHelper)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Queue Data to Table";

        public override string GetToolTipText() => "Queue clipboard tabular data or any number of files to a table";

        public override IKustoIngestClient CreateIngestClient(KustoConnectionStringBuilder ClusterUri)
        {
            var client = KustoIngestFactory.CreateQueuedIngestClient(ClusterUri);

            return client;
        }
    }
}
