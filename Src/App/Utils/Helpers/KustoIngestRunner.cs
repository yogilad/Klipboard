using System.Threading.Tasks.Dataflow;

using Kusto.Ingest;


namespace Klipboard.Utils
{
    public record IngestFileWorkItem(string filePath, KustoIngestionProperties ingestProperties, StorageSourceOptions sourceOptions);
    public delegate void Trace(string level, string message);
    public delegate void ProgressReport(IngestFileWorkItem item, bool success);

    public class KustoIngestRunner
    {
        private IKustoIngestClient m_ingestClient;
        private ActionBlock<IngestFileWorkItem> m_ingestBlock;

        public event Trace TraceEvent;
        public event ProgressReport ReportProgress;

        public KustoIngestRunner(IKustoIngestClient ingestClient, int degreeOfParallelism)
        {
            var options = new ExecutionDataflowBlockOptions() { MaxDegreeOfParallelism = degreeOfParallelism };

            m_ingestClient = ingestClient;
            m_ingestBlock = new ActionBlock<IngestFileWorkItem>(TryIngestFile, options);
        }

        public async Task QueueWorkItemAsync(IngestFileWorkItem workItem)
        {
            await m_ingestBlock.SendAsync(workItem);
        }

        public async Task CloseAndWaitForCompletionAsync()
        {
            m_ingestBlock.Complete();
            await m_ingestBlock.Completion.ConfigureAwait(false);
        }

        private async Task TryIngestFile(IngestFileWorkItem workItem)
        {
            try
            {
                await IngestFileImpl(workItem);
            }
            catch (Exception ex) 
            {
                TraceEvent?.Invoke("Error", $"ingestion of file '{workItem.filePath}' failed with exception:\n{ex}");
                ReportProgress?.Invoke(workItem, success: false);
            }
        }

        private async Task IngestFileImpl(IngestFileWorkItem workItem)
        {
            var ingestionResult = await m_ingestClient.IngestFromStorageAsync(workItem.filePath, workItem.ingestProperties, workItem.sourceOptions).ConfigureAwait(false);
            var ingestionStatus = ingestionResult.GetIngestionStatusBySourceId(workItem.sourceOptions.SourceId);

            switch (ingestionStatus.Status)
            {
                case Status.Queued:
                case Status.Succeeded:
                    TraceEvent?.Invoke("Info", $"ingestion of file '{workItem.filePath}' succeeded");
                    ReportProgress?.Invoke(workItem, success: true);
                    break;

                case Status.Pending:
                    TraceEvent?.Invoke("Warning", $"ingestion of file '{workItem.filePath}' in pending state and {nameof(KustoIngestRunner)} does not support waiting for status");
                    ReportProgress?.Invoke(workItem, success: true);
                    break;

                case Status.PartiallySucceeded:
                    TraceEvent?.Invoke("Warning", $"ingestion of file '{workItem.filePath}' ended with partial success");
                    ReportProgress?.Invoke(workItem, success: false);
                    break;

                case Status.Failed:
                    TraceEvent?.Invoke("Error", $"ingestion of file '{workItem.filePath}' failed with error ({ingestionStatus.ErrorCode}): {ingestionStatus.FailureStatus}");
                    ReportProgress?.Invoke(workItem, success: false);
                    break;

                case Status.Skipped:
                    TraceEvent?.Invoke("Error", $"ingestion of file '{workItem.filePath}' skipped with error ({ingestionStatus.ErrorCode}): {ingestionStatus.FailureStatus}");
                    ReportProgress?.Invoke(workItem, success: false);
                    break;
            }
        }
    }
}
