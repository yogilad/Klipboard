using System.Diagnostics;

using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class StructuredDataInlineQueryWorker : WorkerBase
    {
        // Static members
        private static string NotifcationTitle => "Inline Query";

        public StructuredDataInlineQueryWorker(WorkerCategory category, AppConfig config, object? icon = null)
        : base(category, ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files, config, icon) // Todo Support Text and File Data
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
            return $"Invoke a datatable query on one small file or {AppConstants.MaxAllowedDataLength}KB of clipboard data structured as a table";
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
            return Task.Run(() => HandleCsvData(csvData, '\t', sendNotification));
        }

        public override Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            char? separator;

            TabularDataHelper.TryDetectTabularTextFormat(textData, out separator);
            
            // a failed detection could simply mean a single column
            return Task.Run(() => HandleCsvData(textData, separator ?? ',', sendNotification));
        }

        public override Task HandleFilesAsync(List<string> files, SendNotification sendNotification)
        {
            if (files.Count > 1) 
            {
                sendNotification(NotifcationTitle, "Inline query only supports a single file.");
            }

            var file = files[0];
            var fileInfo = new FileInfo(file);

            if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                sendNotification(NotifcationTitle, "Inline query does not support directories.");
                return Task.CompletedTask;
            }

            if (!fileInfo.Exists) 
            {
                sendNotification(NotifcationTitle, $"File '{file}' does not exist.");
                return Task.CompletedTask;
            }

            if (fileInfo.Length > AppConstants.MaxAllowedDataLength)
            {
                sendNotification(NotifcationTitle, $"File size exceeds max limit of {AppConstants.MaxAllowedDataLengthKb}KB for inline query ");
                return Task.CompletedTask;
            }

            string textData = File.ReadAllText(file);
            char? separator;

            if (fileInfo.Extension.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                separator = ',';
            }
            else if (fileInfo.Extension.Equals("tsv", StringComparison.OrdinalIgnoreCase))
            {
                separator = '\t';
            }
            else
            {
                TabularDataHelper.TryDetectTabularTextFormat(textData, out separator);
            }

            // a failed detection could simply mean a single column
            return Task.Run(() => HandleCsvData(textData, separator ?? ',', sendNotification));
        }

        private void HandleCsvData(string csvData, char separator, SendNotification sendNotification)
        {
            if (AppConstants.EnforceInlineQuerySizeLimits && csvData.Length > AppConstants.MaxAllowedDataLength)
            {
                sendNotification(NotifcationTitle, $"Source data size {(int) (csvData.Length / 1024)}KB is greater then inline query limited of {AppConstants.MaxAllowedDataLengthKb}KB.");
                return;
            }

            var success = TabularDataHelper.TryConvertTableToInlineQuery(
                csvData,
                separator.ToString(),
                "| take 100",
                out var query);

            if (!success || query == null)
            {
                sendNotification(NotifcationTitle, "Failed to create query text.");
                return;
            }

            if (!InlineQueryHelper.TryInvokeInlineQuery(m_appConfig, m_appConfig.DefaultClusterConnectionString, m_appConfig.DefaultClusterDatabaseName, query, out var error))
            {
                sendNotification(NotifcationTitle, error);
                return;
            }
        }
    }
}
