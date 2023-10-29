using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class FreeTextInlineQueryWorker : WorkerBase
    {
        private static readonly string ToolTipText = $"Invoke a query on one small file or {AppConstants.MaxAllowedDataLengthKb}KB of clipboard containing unstructured text";
        private static readonly string NotifcationTitle = "Free Text Inline Query";

        public FreeTextInlineQueryWorker(ISettings settings, INotificationHelper notificationHelper)
        : base(ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files, settings, notificationHelper)
        {
        }

        public override string GetMenuText(ClipboardContent content) => $"Paste to Free Text Inline Query";

        public override string GetToolTipText() => ToolTipText;

        public override bool IsMenuVisible() => true;

        public override async Task HandleCsvAsync(string csvData, string? chosenOptions)
        {
            await Task.Run(() => HandleFreeTextData(csvData));
        }

        public override async Task HandleTextAsync(string textData, string? chosenOptions)
        {
            // a failed detection could simply mean a single column
            await Task.Run(() => HandleFreeTextData(textData));
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

            string textData = await File.ReadAllTextAsync(file);

            // a failed detection could simply mean a single column
            await Task.Run(() => HandleFreeTextData(textData));
        }

        private void HandleFreeTextData(string freeText)
        {
            if (AppConstants.EnforceInlineQuerySizeLimits && freeText.Length > AppConstants.MaxAllowedDataLength)
            {
                m_notificationHelper.ShowBasicNotification(NotifcationTitle, $"Source data size {(int) (freeText.Length / 1024)}KB is greater then inline query limited of {AppConstants.MaxAllowedDataLengthKb}KB.");
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
                m_notificationHelper.ShowBasicNotification(NotifcationTitle, "Failed to create query text.");
                return;
            }

            if (!InlineQueryHelper.TryInvokeInlineQuery(appConfig, NotifcationTitle, appConfig.ChosenCluster.ConnectionString, appConfig.ChosenCluster.DatabaseName, query, m_notificationHelper, out var error))
            {
                m_notificationHelper.ShowExtendedNotification(NotifcationTitle, "Failed to invoke inline query", error ?? "Unknown error.");
                return;
            }
        }
    }
}
