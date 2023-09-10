using Klipboard.Workers;

namespace Klipboard.Utils;

public interface INotificationIcon
{
    void SendNotification(string title, string message);
    object AddWorker(WorkerBase worker, Func<WorkerBase?, Task> workerClick);

    void AddSeparator();
    void UpdateWorker(object item, WorkerBase worker, ClipboardContent content);

    void AddAdditionalItems(ISettings settings);

    void SetOnClick(Func<Task> onClick);
}
