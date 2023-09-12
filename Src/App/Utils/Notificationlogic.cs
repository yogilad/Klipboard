using Klipboard.Workers;

namespace Klipboard.Utils;

public class Notificationlogic
{
    INotificationIcon m_notificationIcon;
    private readonly ISettings m_settings;
    private readonly IClipboardHelper m_clipboardHelper;
    private readonly List<(object item, IWorker worker)> _items = new ();
    private Dictionary<string, object> m_icons;
    private readonly List<IWorker> m_workers;

    public Notificationlogic(
        INotificationIcon notificationIcon,
        ISettings settings,
        IClipboardHelper clipboardHelper,
        Dictionary<string, object> icons,
        List<IWorker> workers
        )
    {
        m_icons = icons;
        m_workers = workers;
        m_notificationIcon = notificationIcon;
        m_settings = settings;
        m_clipboardHelper = clipboardHelper;
    }

    public void Init()
    {
        m_notificationIcon.SetOnClick(async () => await UpdateWorkers());
        AddWorkers();
    }

    private void AddWorkers()
    {
        IWorker? previousWorker = null;
        foreach (var worker in m_workers)
        {
            if (previousWorker != null && previousWorker.Category != worker.Category)
            {
                m_notificationIcon.AddSeparator();
            }

            var item = m_notificationIcon.AddWorker(worker, WorkerClick);
            _items.Add((item, worker));


            previousWorker = worker;
        }

        m_notificationIcon.AddAdditionalItems(m_settings);
    }

    private async Task UpdateWorkers()
    {
        var content = await m_clipboardHelper.GetClipboardContent();

        foreach (var (item, worker) in _items)
        {
            m_notificationIcon.UpdateWorker(item, worker, content);
        }
    }

    private void SendNotification(string title, string message)
    {
        m_notificationIcon.SendNotification(title, message);
    }

    private async Task WorkerClick(IWorker? worker)
    {
        if (worker == null)
        {
            return;
        }

        var content = await m_clipboardHelper.GetClipboardContent();
        var contentToHandle = content & worker.SupportedContent;

        switch (contentToHandle)
        {
            case ClipboardContent.None:
                RunWorker(worker, async () => await worker.HandleAsync(SendNotification));
                break;

            case ClipboardContent.CSV:
                var csvData = await m_clipboardHelper.TryGetDataAsString();
                if (string.IsNullOrWhiteSpace(csvData))
                {
                    SendNotification("Error!", "Failed to get CSV Data from clipboard");
                    return;
                }

                RunWorker(worker, async () => await worker.HandleCsvAsync(csvData, SendNotification));
                break;

            case ClipboardContent.CSV_Stream:
                var csvStream = await m_clipboardHelper.TryGetDataAsMemoryStream();
                if (csvStream == null)
                {
                    SendNotification("Error!", "Failed to get CSV Data from clipboard");
                    return;
                }

                RunWorker(worker, async () => await worker.HandleCsvStreamAsync(csvStream, SendNotification));
                break;

            case ClipboardContent.Text:
                var textData = await m_clipboardHelper.TryGetDataAsString();
                if (string.IsNullOrWhiteSpace(textData))
                {
                    SendNotification("Error!", "Failed to get Text Data from clipboard");
                    return;
                }

                RunWorker(worker, async () => await worker.HandleTextAsync(textData, SendNotification));
                break;

            case ClipboardContent.Text_Stream:
                var textStream = await m_clipboardHelper.TryGetDataAsMemoryStream();
                if (textStream == null)
                {
                    SendNotification("Error!", "Failed to get Text Data from clipboard");
                    return;
                }

                RunWorker(worker, async () => await worker.HandleTextStreamAsync(textStream, SendNotification));
                break;

            case ClipboardContent.Files:
                var filesAndFolders = await m_clipboardHelper.TryGetFileDropList();
                if (filesAndFolders == null ||
                    filesAndFolders.Count == 0)
                {
                    SendNotification("Error!", "Failed to get Files Data from clipboard");
                    return;
                }

                RunWorker(worker, async () => await worker.HandleFilesAsync(filesAndFolders, SendNotification));
                break;

        }
    }

    private void RunWorker(IWorker worker, Func<Task> action)
    {
        Task.Run(async () =>
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                SendNotification(worker.GetType().ToString(), $"Worker run ended with exception!\n\n{ex}");
            }
        });
    }
}
