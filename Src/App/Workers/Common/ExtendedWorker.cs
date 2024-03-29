using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;

namespace Klipboard.Workers
{
    public static class ExtendedWorker
    {
        public static void OnClick(this IWorker worker, IClipboardHelper clipboardHelper, INotificationHelper notificationHelper, string? chosenOption)
        {
            if (worker == null)
            {
                return;
            }

            var content = clipboardHelper.GetClipboardContent();
            var contentToHandle = content & worker.SupportedContent;

            switch (contentToHandle)
            {
                case ClipboardContent.None:
                    worker.RunWorker(nameof(worker.HandleAsync), async () => await worker.HandleAsync(chosenOption), notificationHelper);
                    break;

                case ClipboardContent.CSV:
                    var csvData = clipboardHelper.TryGetDataAsString();

                    if (string.IsNullOrWhiteSpace(csvData))
                    {
                        notificationHelper.ShowBasicNotification("Error!", "Failed to get CSV Data from clipboard");
                        return;
                    }

                    worker.RunWorker(nameof(worker.HandleCsvAsync), async () => await worker.HandleCsvAsync(csvData, chosenOption), notificationHelper);
                    break;

                case ClipboardContent.CSV_Stream:
                    var csvStream = clipboardHelper.TryGetDataAsMemoryStream();
                    
                    if (csvStream == null)
                    {
                        notificationHelper.ShowBasicNotification("Error!", "Failed to get CSV Data from clipboard");
                        return;
                    }

                    worker.RunWorker(nameof(worker.HandleCsvStreamAsync), async () => await worker.HandleCsvStreamAsync(csvStream), notificationHelper);
                    break;

                case ClipboardContent.Text:
                    var textData = clipboardHelper.TryGetDataAsString();
                    
                    if (string.IsNullOrWhiteSpace(textData))
                    {
                        notificationHelper.ShowBasicNotification("Error!", "Failed to get Text Data from clipboard");
                        return;
                    }

                    worker.RunWorker(nameof(worker.HandleTextAsync), async () => await worker.HandleTextAsync(textData, chosenOption), notificationHelper);
                    break;

                case ClipboardContent.Text_Stream:
                    var textStream = clipboardHelper.TryGetDataAsMemoryStream();
                    
                    if (textStream == null)
                    {
                        notificationHelper.ShowBasicNotification("Error!", "Failed to get Text Data from clipboard");
                        return;
                    }

                    worker.RunWorker(nameof(worker.HandleTextStreamAsync), async () => await worker.HandleTextStreamAsync(textStream), notificationHelper);
                    break;

                case ClipboardContent.Files:
                    var filesAndFolders = clipboardHelper.TryGetFileDropList();
                    
                    if (filesAndFolders == null || filesAndFolders.Count == 0)
                    {
                        notificationHelper.ShowBasicNotification("Error!", "Failed to get Files Data from clipboard");
                        return;
                    }

                    worker.RunWorker(nameof(worker.HandleFilesAsync), async () => await worker.HandleFilesAsync(filesAndFolders, chosenOption), notificationHelper);
                    break;
            }
        }

        private static void RunWorker(this IWorker worker, string operation, Func<Task> action, INotificationHelper notificationHelper)
        {
            Task.Run(async () =>
            {
                var className = worker.GetType().ToString().SplitTakeLast(".");
                using var scope = Logger.OperationScope(className, operation);

                try
                {
                    Logger.Log.Information("Operation Started");

                    await action();

                    Logger.Log.Information("Operation Ended");

                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, "Operation ended with an exception");
                    notificationHelper.ShowExtendedNotification(worker.GetType().ToString(), $"{ex.Message}", ex.ToString());
                }
            });
        }
    }
}
