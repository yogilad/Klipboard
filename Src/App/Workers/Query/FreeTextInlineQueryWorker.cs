using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class FreeTextInlineQueryWorker : WorkerBase
    {
        private static readonly string ToolTipText = $"Invoke a query on one small file or {AppConstants.MaxAllowedDataLengthKb}KB of clipboard containing unstructured text";
        private static readonly string NotifcationTitle = "Free Text Inline Query";

        public FreeTextInlineQueryWorker(ISettings settings)
        : base(ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files, settings) // Todo Support Text and File Data
        {
        }

        public override string GetMenuText(ClipboardContent content) => $"Paste to Free Text Inline Query";

        public override string GetToolTipText() => ToolTipText;

        public override bool IsMenuVisible() => true;

        public override async Task HandleCsvAsync(string csvData, SendNotification sendNotification, string? chosenOptions)
        {
            await Task.Run(() => HandleFreeTextData(csvData, sendNotification));
        }

        public override async Task HandleTextAsync(string textData, SendNotification sendNotification, string? chosenOptions)
        {
            // a failed detection could simply mean a single column
            await Task.Run(() => HandleFreeTextData(textData, sendNotification));
        }

        public override async Task HandleFilesAsync(List<string> files, SendNotification sendNotification, string? chosenOption)
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

            string textData = await File.ReadAllTextAsync(file);

            // a failed detection could simply mean a single column
            await Task.Run(() => HandleFreeTextData(textData, sendNotification));
        }

        private void HandleFreeTextData(string freeText, SendNotification sendNotification)
        {
            if (AppConstants.EnforceInlineQuerySizeLimits && freeText.Length > AppConstants.MaxAllowedDataLength)
            {
                sendNotification(NotifcationTitle, $"Source data size {(int) (freeText.Length / 1024)}KB is greater then inline query limited of {AppConstants.MaxAllowedDataLengthKb}KB.");
                return;
            }
            var appConfig = m_settings.GetConfig();

            var prependFreeTextQueriesWithKql = appConfig.PrependFreeTextQueriesWithKql;
            var kqlSuffix = string.IsNullOrWhiteSpace(prependFreeTextQueriesWithKql)? string.Empty : prependFreeTextQueriesWithKql.Trim();
            kqlSuffix += string.IsNullOrWhiteSpace(kqlSuffix) ? "| take  100" : "\n| take 100";

            var success = TabularDataHelper.TryConvertFreeTextToInlineQuery(
                freeText,
                kqlSuffix,
                out var query);

            if (!success || query == null)
            {
                sendNotification(NotifcationTitle, "Failed to create query text.");
                return;
            }

            if (!InlineQueryHelper.TryInvokeInlineQuery(appConfig, appConfig.ChosenCluster.ConnectionString, appConfig.ChosenCluster.DatabaseName, query, out var error))
            {
                sendNotification(NotifcationTitle, error ?? "Unknown error.");
                return;
            }
        }
    }
}
