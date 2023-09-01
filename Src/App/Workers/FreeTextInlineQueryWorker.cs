﻿using System.Diagnostics;

using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class FreeTextInlineQueryWorker : WorkerBase
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
        private static string NotifcationTitle => "Free Text Inline Query";

        public FreeTextInlineQueryWorker(WorkerCategory category, object? icon)
        : base(category, icon, ClipboardContent.CSV | ClipboardContent.Text | ClipboardContent.Files) // Todo Support Text and File Data
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
            return $"Invoke a query on one small file or {c_maxAllowedDataLength}KB of clipboard contiainig unstructured text";
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

            if (fileInfo.Length > c_maxAllowedDataLength)
            {
                sendNotification(NotifcationTitle, $"File size exceeds max limit of {c_maxAllowedDataLengthKb}KB for inline query ");
                return Task.CompletedTask;
            }

            string textData = File.ReadAllText(file);

            // a failed detection could simply mean a single column
            return Task.Run(() => HandleFreeTextData(textData, sendNotification));
        }

        private void HandleFreeTextData(string freeText, SendNotification sendNotification)
        {
            if (m_forceLimits && freeText.Length > c_maxAllowedDataLength)
            {
                sendNotification(NotifcationTitle, $"Source data size {(int) (freeText.Length / 1024)} is greater then inline query limited of {c_maxAllowedDataLengthKb}KB.");
                return;
            }

            var success = TabularDataHelper.TryConvertFreeTextToInlineQuery(
                freeText,
                out var query);

            if (!success || query == null)
            {
                sendNotification(NotifcationTitle, "Failed to create query link.");
                return;
            }

            query = TabularDataHelper.GzipBase64(query);

#if DEBUG
            sendNotification("Debug", $"Input Length={freeText.Length}, Output Legth={query.Length}");
#endif

            if (m_forceLimits && query.Length > c_max_allowedQueryLength)
            {
                sendNotification(NotifcationTitle, $"Resulting query link excceds {c_max_allowedQueryLengthKB}KB.");
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
