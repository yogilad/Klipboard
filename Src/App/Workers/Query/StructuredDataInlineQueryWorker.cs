using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class StructuredDataInlineQueryWorker : WorkerBase
    {
        private static readonly string ToolTipText = $"Invoke a datatable query on one small file or {AppConstants.MaxAllowedDataLengthKb}KB of clipboard data structured as a table";
        private static string NotifcationTitle => "Inline Query";
        private const string FirstRowIsHeader = "First Row Is Header";
        private const string NoHeaderRow = "No Header Row";
        private const string AutoDetection = "Auto Detect Header Row";

        public StructuredDataInlineQueryWorker(ISettings settings, INotificationHelper notificationHelper)
        : base  (ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files, settings, notificationHelper, new List<string> { AutoDetection, FirstRowIsHeader, NoHeaderRow })
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Paste to Inline Query";

        public override string GetToolTipText() => ToolTipText;

        public override bool IsMenuVisible() => true;

        public override async Task HandleCsvAsync(string csvData, string? chosenOption)
        {
            await Task.Run(() => HandleCsvData(csvData, '\t', chosenOption));
        }

        public override async Task HandleTextAsync(string textData, string? chosenOption)
        {
            char? separator;

            TabularDataHelper.TryDetectTabularTextFormat(textData, out separator);

            // a failed detection could simply mean a single column
            await Task.Run(() => HandleCsvData(textData, separator ?? ',', chosenOption));
        }

        public override async Task HandleFilesAsync(List<string> files, string? chosenOption)
        {
            if (files.Count > 1)
            {
                m_notificationHelper.ShowBasicNotification(NotifcationTitle, "Inline query only supports a single file.");
            }

            var file = files[0];
            var fileInfo = new FileInfo(file);

            if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                m_notificationHelper.ShowBasicNotification(NotifcationTitle, "Inline query does not support directories.");
                return;
            }

            if (!fileInfo.Exists)
            {
                m_notificationHelper.ShowBasicNotification(NotifcationTitle, $"File '{file}' does not exist.");
                return;
            }

            if (fileInfo.Length > AppConstants.MaxAllowedDataLength)
            {
                m_notificationHelper.ShowBasicNotification(NotifcationTitle, $"File size exceeds max limit of {AppConstants.MaxAllowedDataLengthKb}KB for inline query ");
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
                    m_notificationHelper.ShowBasicNotification(NotifcationTitle, $"File extension '{extension}' not supported free . Try using other options.");
                    return;
            }

            string textData = await File.ReadAllTextAsync(file);

            if (separator == null)
            {
                TabularDataHelper.TryDetectTabularTextFormat(textData, out separator);
            }

            // a failed detection could simply mean a single column
            await Task.Run(() => HandleCsvData(textData, separator ?? ',', chosenOption));
        }

        private void HandleCsvData(string csvData, char separator, string? chosenOption)
        {
            var detectionMode = m_settings.GetConfig().KqlTypeDetectionMode;
            bool? isForstRowHeader = null;

            switch(chosenOption)
            {
                case FirstRowIsHeader:
                    isForstRowHeader = true;
                    break;

                case NoHeaderRow:
                    isForstRowHeader = false;
                    break;
            }
            

            if (AppConstants.EnforceInlineQuerySizeLimits && csvData.Length > AppConstants.MaxAllowedDataLength)
            {
                m_notificationHelper.ShowBasicNotification(NotifcationTitle, $"Source data size {(int) (csvData.Length / 1024)}KB is greater then inline query limited of {AppConstants.MaxAllowedDataLengthKb}KB.");
                return;
            }

            var success = TabularDataHelper.TryConvertTableToInlineQuery(
                csvData,
                separator.ToString(),
                detectionMode,
                isForstRowHeader,
                "| take 100",
                out var query);

            if (!success || query == null)
            {
                m_notificationHelper.ShowBasicNotification(NotifcationTitle, "Failed to create query text.");
                return;
            }
            var appConfig = m_settings.GetConfig();

            if (!InlineQueryHelper.TryInvokeInlineQuery(appConfig, NotifcationTitle, appConfig.ChosenCluster.ConnectionString, appConfig.ChosenCluster.DatabaseName, query, m_notificationHelper, out var error))
            {
                m_notificationHelper.ShowExtendedNotification(NotifcationTitle, "Failed to invoke inline query", error ?? "Unknown error.");
                return;
            }
        }
    }
}
