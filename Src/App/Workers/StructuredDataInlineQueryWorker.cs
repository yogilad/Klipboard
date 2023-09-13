using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class StructuredDataInlineQueryWorker : WorkerBase
    {
        private static readonly string ToolTipText = $"Invoke a datatable query on one small file or {AppConstants.MaxAllowedDataLengthKb}KB of clipboard data structured as a table";
        private static string NotifcationTitle => "Inline Query";

        public StructuredDataInlineQueryWorker(WorkerCategory category, ISettings settings, object? icon = null)
        : base(category, ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files, settings, icon) // Todo Support Text and File Data
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Paste to Inline Query";

        public override string GetToolTipText(ClipboardContent content) => ToolTipText;

        public override bool IsMenuVisible(ClipboardContent content) => true;

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            await Task.Run(() => HandleCsvData(csvData, '\t', sendNotification));
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            char? separator;

            TabularDataHelper.TryDetectTabularTextFormat(textData, out separator);

            // a failed detection could simply mean a single column
            await Task.Run(() => HandleCsvData(textData, separator ?? ',', sendNotification));
        }

        public override async Task HandleFilesAsync(List<string> files, SendNotification sendNotification)
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
                return;
            }

            if (!fileInfo.Exists)
            {
                sendNotification(NotifcationTitle, $"File '{file}' does not exist.");
                return;
            }

            if (fileInfo.Length > AppConstants.MaxAllowedDataLength)
            {
                sendNotification(NotifcationTitle, $"File size exceeds max limit of {AppConstants.MaxAllowedDataLengthKb}KB for inline query ");
                return;
            }

            var extension = fileInfo.Extension.TrimStart('.').ToLower();
            char? separator = null;

            switch(extension)
            {
                case "csv":
                    separator = ',';
                    break;

                case "tsv":
                    separator = '\t';
                    break;

                case "txt":
                case "log":
                    break;

                default:
                    sendNotification(NotifcationTitle, $"File extension '{extension}' not supported free . Try using other options.");
                    return;
            }

            string textData = await File.ReadAllTextAsync(file);

            if (separator == null)
            {
                TabularDataHelper.TryDetectTabularTextFormat(textData, out separator);
            }

            // a failed detection could simply mean a single column
            await Task.Run(() => HandleCsvData(textData, separator ?? ',', sendNotification));
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
            var appConfig = m_settings.GetConfig();

            if (!InlineQueryHelper.TryInvokeInlineQuery(appConfig, appConfig.ChosenCluster.ConnectionString, appConfig.ChosenCluster.DatabaseName, query, out var error))
            {
                sendNotification(NotifcationTitle, error ?? "Unknown error.");
                return;
            }
        }
    }
}
