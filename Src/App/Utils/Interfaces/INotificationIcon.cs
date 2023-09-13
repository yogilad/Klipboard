namespace Klipboard.Utils
{
    public interface INotificationIcon
    {
        void SendNotification(string title, string message);
        object AddWorker(IWorker worker, Func<IWorker?, Task> workerClick);
        void AddSeparator();
        void UpdateWorker(object item, IWorker worker, ClipboardContent content);
        void AddAdditionalItems(ISettings settings);
        void SetOnClick(Func<Task> onClick);
    }
}

