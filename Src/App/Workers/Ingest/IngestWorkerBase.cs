using Kusto.Data;
using Kusto.Ingest;

using Klipboard.Utils;
using Microsoft.Identity.Client;
using Kusto.Data.Security;
using Azure.Identity;

namespace Klipboard.Workers
{
    public abstract class IngestWorkerBase : WorkerBase
    {
        public enum MappingType
        {
            None,
            Inline,
            Reference
        }

        public class IngestUserSelection
        {
            public bool UserConfirmedIngestion;

            public string ClusterUri = string.Empty;
            public string Database = string.Empty;

            public bool CreateTable;
            public string Table = string.Empty;
            public string TableSchema = string.Empty;

            public string Format = string.Empty;
            public bool FirstRowIsHeader;

            public MappingType MappingTypeSelection;
            public string Mapping = string.Empty;

            public int DegreeOfParallelism;
        }

        public IngestWorkerBase(ISettings settings) 
            : base(ClipboardContent.Files, settings)
        {
        }

        public abstract IKustoIngestClient CreateIngestClient(KustoConnectionStringBuilder ClusterUri);

        public async Task HandleFilesAsync(List<string> filesAndFolders, 
            IngestUserSelection userSelection, 
            KustoIngestRunner.Trace traceEvent, 
            KustoIngestRunner.ProgressReport reportProgress)
        {
            using var databaseHelper = new KustoDatabaseHelper(userSelection.ClusterUri, userSelection.Database);

            if (userSelection.CreateTable) 
            { 
                if (string.IsNullOrWhiteSpace(userSelection.TableSchema))
                {
                    // TODO Report error
                    return;
                }

                using var helper = new KustoDatabaseHelper(userSelection.ClusterUri, userSelection.Database);
                var result = await helper.TryCreateTableAync(userSelection.Table, userSelection.TableSchema, ingestionBatchingTimeSeconds: 60);

                if (result.Success == false)
                {
                    // TODO Report error
                    return;
                }
            }

            var m_ingestionRunner = new KustoIngestRunner(databaseHelper.GetDirectIngestClient(), degreeOfParallelism: userSelection.DegreeOfParallelism);

            m_ingestionRunner.TraceEvent += traceEvent;
            m_ingestionRunner.ReportProgress += reportProgress;

            foreach (var path in FileHelper.ExpandDropFileList(filesAndFolders))
            {
                var format = userSelection.Format;
                
                if (format == AppConstants.UnknownFormat)
                {
                    var fileInfo = new FileInfo(path);

                    if (string.IsNullOrWhiteSpace(fileInfo.Extension))
                    {
                        // TODO Report back
                        continue;
                    }

                    format = fileInfo.Extension.TrimStart('.');
                }

                var storageOptions = new StorageSourceOptions();
                var ingestionProperties = new KustoIngestionProperties()
                {
                    DatabaseName = userSelection.Database,
                    TableName = userSelection.Table,
                    Format = FileHelper.GetFormatFromExtension(format),
                    IgnoreFirstRecord = userSelection.FirstRowIsHeader,
                };

                switch(userSelection.MappingTypeSelection)
                {
                    case MappingType.Inline:
                        // TODO Support inline mapping
                        break;

                    case MappingType.Reference:
                        // TODO Support inline mapping
                        break;
                }

                await m_ingestionRunner.QueueWorkItemAsync(new KustoIngestRunner.IngestFileWorkItem(path, ingestionProperties, storageOptions));
            }

            await m_ingestionRunner.CloseAndWaitForCompletionAsync();

            // TODO send done notice
        }
    }
}
