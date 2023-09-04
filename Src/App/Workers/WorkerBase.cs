using Klipboard.Utils;

namespace Klipboard.Workers
{
    public enum WorkerCategory
    {
        QuickActions,
        Actions,
        Management,
        Debug
    }

    public abstract class WorkerBase
    {
        public delegate void SendNotification(string title, string message);

        protected readonly ClipboardContent m_supportedContent;
        protected readonly WorkerCategory m_category;
        protected readonly object? m_icon;
        protected readonly AppConfig m_appConfig;

        public ClipboardContent SupportedContent => m_supportedContent;
        public WorkerCategory Category => m_category;
        public object? Icon => m_icon;

        protected WorkerBase(WorkerCategory category, ClipboardContent supportedContent, AppConfig config, object? icon = null)
        {
            m_supportedContent = supportedContent;
            m_category = category;
            m_icon = icon;
            m_appConfig = config;
        }

        public virtual bool IsVisible(ClipboardContent content)
        {
            return AppConstants.DevMode;
        }

        public virtual bool IsEnabled(ClipboardContent content)
        {
            return (content & SupportedContent) != ClipboardContent.None;
        }

        public virtual string GetMenuText(ClipboardContent content)
        {
            return this.GetType().ToString();
        }

        public virtual string GetToolTipText(ClipboardContent content)
        {
            return string.Empty;
        }

        public virtual async Task HandleAsync(SendNotification sendNotification)
        {
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleAsync)}");

            return;
        }

        public virtual async Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleCsvAsync)}");

            return;
        }

        public virtual async Task HandleTextAsync(string textData, SendNotification sendNotification)
        {
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleTextAsync)}");

            return;
        }

        public virtual async Task HandleFilesAsync(List<string> filesAndFolders , SendNotification sendNotification)
        {
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleFilesAsync)}");

            return;
        }
    }
}
