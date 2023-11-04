using Kusto.Data;
using Kusto.Ingest;
using Klipboard.Utils;


namespace Klipboard
{
    public class DirectIngestWorkerUx : IngestWorkerUxBase
    {
        public DirectIngestWorkerUx(ISettings settings, INotificationHelper notificationHelper) 
            : base(settings, notificationHelper)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Direct Ingest Data to Table";

        public override string GetToolTipText() => "Direct ingest clipboard tabular data or any number of files to a table";

        public override IKustoIngestClient CreateIngestClient(KustoConnectionStringBuilder ClusterUri)
        {
            var client = KustoIngestFactory.CreateDirectIngestClient(ClusterUri);

            return client;
        }
    }
}
