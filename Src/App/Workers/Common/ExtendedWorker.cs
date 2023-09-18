using Klipboard.Utils;


namespace Klipboard.Workers
{
    public static class ExtendedWorker
    {
        public static void OnClick(this IWorker worker, IClipboardHelper clipboardHelper, SendNotification sendNotification, string? chosenOption)
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
                    worker.RunWorker(async (sendNotification) => await worker.HandleAsync(sendNotification, chosenOption), sendNotification);
                    break;

                case ClipboardContent.CSV:
                    var csvData = clipboardHelper.TryGetDataAsString();

                    if (string.IsNullOrWhiteSpace(csvData))
                    {
                        sendNotification("Error!", "Failed to get CSV Data from clipboard");
                        return;
                    }

                    worker.RunWorker(async (sendNotification) => await worker.HandleCsvAsync(csvData, sendNotification, chosenOption), sendNotification);
                    break;

                case ClipboardContent.CSV_Stream:
                    var csvStream = clipboardHelper.TryGetDataAsMemoryStream();
                    
                    if (csvStream == null)
                    {
                        sendNotification("Error!", "Failed to get CSV Data from clipboard");
                        return;
                    }

                    worker.RunWorker(async (sendNotification) => await worker.HandleCsvStreamAsync(csvStream, sendNotification), sendNotification);
                    break;

                case ClipboardContent.Text:
                    var textData = clipboardHelper.TryGetDataAsString();
                    
                    if (string.IsNullOrWhiteSpace(textData))
                    {
                        sendNotification("Error!", "Failed to get Text Data from clipboard");
                        return;
                    }

                    worker.RunWorker(async (sendNotification) => await worker.HandleTextAsync(textData, sendNotification, chosenOption), sendNotification);
                    break;

                case ClipboardContent.Text_Stream:
                    var textStream = clipboardHelper.TryGetDataAsMemoryStream();
                    
                    if (textStream == null)
                    {
                        sendNotification("Error!", "Failed to get Text Data from clipboard");
                        return;
                    }

                    worker.RunWorker(async (sendNotification) => await worker.HandleTextStreamAsync(textStream, sendNotification), sendNotification);
                    break;

                case ClipboardContent.Files:
                    var filesAndFolders = clipboardHelper.TryGetFileDropList();
                    
                    if (filesAndFolders == null || filesAndFolders.Count == 0)
                    {
                        sendNotification("Error!", "Failed to get Files Data from clipboard");
                        return;
                    }

                    worker.RunWorker(async (SendNotification) => await worker.HandleFilesAsync(filesAndFolders, sendNotification, chosenOption), sendNotification);
                    break;
            }
        }

        private static void RunWorker(this IWorker worker, Func<SendNotification, Task> action, SendNotification sendNotification)
        {
            Task.Run(async () =>
            {
                try
                {
                    await action(sendNotification);
                }
                catch (Exception ex)
                {
                    sendNotification(worker.GetType().ToString(), $"Worker run ended with exception!\n\n{ex}");
                }
            });
        }
    }
}
