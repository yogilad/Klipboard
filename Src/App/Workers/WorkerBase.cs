using Klipboard.Utils;

namespace Klipboard.Workers
{
    public abstract class WorkerBase
    {
        public delegate void SendNotification(string title, string message);

        protected readonly ClipboardContent m_supportedContent;
        protected readonly WorkerCategory m_category;
        protected readonly object? m_icon;

        public ClipboardContent SupportedContent => m_supportedContent;
        public WorkerCategory Category => m_category;
        public object? Icon => m_icon;

        protected WorkerBase(WorkerCategory category, object? icon, ClipboardContent supportedContent)
        {
            m_supportedContent = supportedContent;
            m_category = category;
            m_icon = icon;
        }

        public virtual bool IsVisible(ClipboardContent content)
        {
            return true;
        }

        public virtual bool IsEnabled(ClipboardContent content)
        {
            return false;
        }

        public virtual string GetMenuText(ClipboardContent content)
        {
            return this.GetType().ToString();
        }

        public virtual string GetToolTipText(ClipboardContent content)
        {
            return string.Empty;
        }

        public virtual Task HandleAsync(SendNotification sendNotification)
        {
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleAsync)}");

            return Task.CompletedTask;
        }

        public virtual Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleCsvAsync)}");

            return Task.CompletedTask;
        }

        public virtual Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleTextAsync)}");

            return Task.CompletedTask;
        }

        public virtual Task HandleFilesAsync(List<string> filesAndFolders , SendNotification sendNotification)
        {
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleFilesAsync)}");

            return Task.CompletedTask;
        }
    }
}
