﻿using System.Diagnostics;

using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class FreeTextInlineQueryWorker : WorkerBase
    {
        private static string NotifcationTitle => "Free Text Inline Query";

        public FreeTextInlineQueryWorker(WorkerCategory category, AppConfig config, object? icon = null)
        : base(category, ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files, config, icon) // Todo Support Text and File Data
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            var contentToConsider = content & SupportedContent;
            var contentStr = contentToConsider == ClipboardContent.None ? "Data" : content.ToString();
            return $"Paste {contentStr} to Free Text Inline Query";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return $"Invoke a query on one small file or {AppConstants.MaxAllowedDataLength}KB of clipboard contiainig unstructured text";
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
            return Task.Run(() => HandleFreeTextData(csvData, sendNotification));
        }

        public override Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            // a failed detection could simply mean a single column
            return Task.Run(() => HandleFreeTextData(textData, sendNotification));
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

            // a failed detection could simply mean a single column
            return Task.Run(() => HandleFreeTextData(textData, sendNotification));
        }

        private void HandleFreeTextData(string freeText, SendNotification sendNotification)
        {
            if (AppConstants.EnforceInlineQuerySizeLimits && freeText.Length > AppConstants.MaxAllowedDataLength)
            {
                sendNotification(NotifcationTitle, $"Source data size {(int) (freeText.Length / 1024)}KB is greater then inline query limited of {AppConstants.MaxAllowedDataLengthKb}KB.");
                return;
            }

            var kqlSuffix = string.IsNullOrWhiteSpace(m_appConfig.PrepandFreeTextQueriesWithKQL)? string.Empty : m_appConfig.PrepandFreeTextQueriesWithKQL.Trim();
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

            if (!InlineQueryHelper.TryInvokeInlineQuery(m_appConfig, m_appConfig.DefaultClusterConnectionString, m_appConfig.DefaultClusterDatabaseName, query, out var error))
            {
                sendNotification(NotifcationTitle, error);
                return;
            }
        }
    }
}