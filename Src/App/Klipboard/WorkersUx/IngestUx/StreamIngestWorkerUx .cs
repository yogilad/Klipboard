using Klipboard.Utils;
using Kusto.Data;
using Kusto.Ingest;

namespace Klipboard.Workers
{
    public class StreamIngestWorkerUx : IngestWorkerUxBase
    {
        public StreamIngestWorkerUx(ISettings settings, INotificationHelper notificationHelper) 
            : base(settings, notificationHelper)
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Stream Data to Table";

        public override string GetToolTipText() => "Stream clipboard tabular data or any number of files to a table";

        public override IKustoIngestClient CreateIngestClient(KustoConnectionStringBuilder ClusterUri)
        {
            var policy = new ManagedStreamingIngestPolicy() 
                { 
                    ContinueWhenStreamingIngestionUnavailable = true, 
                    TimeUntilResumingStreamingIngest = TimeSpan.FromMinutes(1)
                };

            var client = KustoIngestFactory.CreateManagedStreamingIngestClient(ClusterUri, ingestPolicy: policy);

            return client;
        }
    }
}
