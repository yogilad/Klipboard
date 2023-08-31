using System.Diagnostics;

using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class InlineQueryWorker : WorkerBase
    {
        // TODO Get Defaults from AppConfig at runtime
        private string m_currentCluster = "kvcd8ed305830f049bbac1.northeurope.kusto.windows.net";
        private string m_currentDatabase = "MyDatabase";
        private bool m_invokeDesktopQuery = false;
        private bool m_forceLimits = true;

        private const int c_max_allowedQueryLengthKB = 12;
        private const int c_max_allowedQueryLength = c_max_allowedQueryLengthKB * 1024;
        private const int c_maxAllowedDataLengthKb = c_max_allowedQueryLengthKB * 10;
        private const int c_maxAllowedDataLength = c_maxAllowedDataLengthKb * 1024;

        public InlineQueryWorker(WorkerCategory category, object? icon)
        : base(category, icon, ClipboardContent.CSV) // Todo Support Text and File Data
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            var contentToConsider = content & SupportedContent;
            var contentStr = contentToConsider == ClipboardContent.None ? "Data" : content.ToString();
            return $"Paste {contentStr} to Inline Query";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return "Invoke a datatable query on one small file or 20KB of clipboard tabular data";
        }

        public override bool IsEnabled(ClipboardContent content)
        {
            return (content & SupportedContent) != ClipboardContent.None;
        }

        public override bool IsVisible(ClipboardContent content)
        {
            return true;
        }

        public override Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            return Task.Run(() => HandleCsvData(csvData, sendNotification));
        }

        private void HandleCsvData(string csvData, SendNotification sendNotification)
        {
            if (m_forceLimits && csvData.Length > c_maxAllowedDataLength)
            {
                sendNotification("Inline Query", $"Source data size {(int) (csvData.Length / 1024)} is greater then inline query limited of {c_maxAllowedDataLengthKb}KB.");
                return;
            }

            var success = TabularDataHelper.TryConvertTableToInlineQueryGzipBase64(
                csvData,
                "\t",
                out var query);

            if (!success || query == null)
            {
                sendNotification("Inline Query", "Failed to create query link.");
                return;
            }

#if DEBUG
            sendNotification("Debug", $"Input Length={csvData.Length}, Output Legth={query.Length}");
#endif

            if (m_forceLimits && query.Length > c_max_allowedQueryLength)
            {
                sendNotification("Inline Query", $"Resulting query link excceds {c_max_allowedQueryLengthKB}KB.");
                return;
            }

            string queryLink;
            if (m_invokeDesktopQuery)
            {
                queryLink = $"https://{m_currentCluster}/{m_currentDatabase}?query={query}&web=0";
            }
            else
            {
                queryLink = $"https://dataexplorer.azure.com/clusters/{m_currentCluster}/databases/{m_currentDatabase}?query={query}";
            }

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = queryLink,
                UseShellExecute = true
            });
        }
    }
}
