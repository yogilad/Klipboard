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
        #region Members And Properties
        public delegate void SendNotification(string title, string message);

        protected readonly ClipboardContent m_supportedContent;
        protected readonly WorkerCategory m_category;
        protected readonly object? m_icon;
        protected readonly AppConfig m_appConfig;

        public ClipboardContent SupportedContent => m_supportedContent;
        public WorkerCategory Category => m_category;
        public object? Icon => m_icon;
        #endregion

        #region Construction
        protected WorkerBase(WorkerCategory category, ClipboardContent supportedContent, AppConfig config, object? icon = null)
        {
            m_supportedContent = supportedContent;
            m_category = category;
            m_icon = icon;
            m_appConfig = config;
        }
        #endregion

        #region Overridable Menu Item APIs
        public virtual bool IsMenuVisible(ClipboardContent content) => AppConstants.DevMode;

        public virtual bool IsMenuEnabled(ClipboardContent content) => (content & SupportedContent) != ClipboardContent.None;

        public virtual string GetMenuText(ClipboardContent content) => this.GetType().ToString();

        public virtual string GetToolTipText(ClipboardContent content) => string.Empty;
        #endregion

        #region Overridable Data Handling APIs
        // Handle a click event that requires no data
        public virtual async Task HandleAsync(SendNotification sendNotification) => sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleAsync)}");

        // Handle a click event that requires CSV string data
        public virtual async Task HandleCsvAsync(string csvData, SendNotification sendNotification) => sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleCsvAsync)}");

        // Handle a click event that requires CSV stream data
        public virtual async Task HandleCsvStreamAsync(Stream csvData, SendNotification sendNotification) => sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleCsvStreamAsync)}");
        
        // Handle a click event that requires Text string data
        public virtual async Task HandleTextAsync(string textData, SendNotification sendNotification) =>sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleTextAsync)}");

        // Handle a click event that requires Text stream data
        public virtual async Task HandleTextStreamAsync(Stream textData, SendNotification sendNotification) => sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleTextStreamAsync)}");

        // Handle a click ebent that requires File List data
        public virtual async Task HandleFilesAsync(List<string> filesAndFolders , SendNotification sendNotification) => sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleFilesAsync)}");
        #endregion
    }
}
