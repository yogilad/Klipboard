using Klipboard.Utils;


namespace Klipboard.Workers
{
    public abstract class WorkerBase : IWorker
    {

        #region Members And Properties
        public ClipboardContent SupportedContent { get; private set; }
        public List<string>? SubMenuOptions { get; private set; }

        protected readonly ISettings m_settings;
        #endregion

        #region Construction
        protected WorkerBase(ClipboardContent supportedContent, ISettings mSettings, List<string> ? subMenuOptions = null)
        {
            SupportedContent = supportedContent;
            SubMenuOptions = subMenuOptions;
            m_settings = mSettings;
        }
        #endregion

        #region Overridable Menu Item APIs

        public virtual bool IsMenuVisible() => AppConstants.DevMode;

        public virtual bool IsMenuEnabled(ClipboardContent content) => (content & SupportedContent) != ClipboardContent.None;

        public virtual string GetMenuText(ClipboardContent content) => this.GetType().ToString();

        public virtual string GetToolTipText() => string.Empty;

        #endregion

        #region Overridable Data Handling APIs

        // Handle a click event that requires no data
        public virtual async Task HandleAsync(SendNotification sendNotification, string? chosenOption) =>
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleAsync)}");

        // Handle a click event that requires CSV string data
        public virtual async Task HandleCsvAsync(string csvData, SendNotification sendNotification, string? chosenOption) =>
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleCsvAsync)}");

        // Handle a click event that requires Text string data
        public virtual async Task HandleTextAsync(string textData, SendNotification sendNotification, string? chosenOption) =>
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleTextAsync)}");

        // Handle a click ebent that requires File List data
        public virtual async Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification, string? subMenuOption) =>
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleFilesAsync)}");

        // Handle a click event that requires CSV stream data
        public virtual async Task HandleCsvStreamAsync(Stream csvData, SendNotification sendNotification) =>
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleCsvStreamAsync)}");

        // Handle a click event that requires Text stream data
        public virtual async Task HandleTextStreamAsync(Stream textData, SendNotification sendNotification) => sendNotification("Not Implemented!",
            $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleTextStreamAsync)}");

        #endregion
    }
}
