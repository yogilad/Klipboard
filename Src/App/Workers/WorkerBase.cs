using Klipboard.Utils;
using Klipboard.Utils.Interfaces;

namespace Klipboard.Workers
{
    public abstract class WorkerBase : IWorker
    {

        #region Members And Properties

        protected readonly ClipboardContent m_supportedContent;
        protected readonly ISettings m_settings;
        protected readonly WorkerCategory m_category;
        protected readonly object? m_icon;

        public ClipboardContent SupportedContent => m_supportedContent;
        public WorkerCategory Category => m_category;
        public object? Icon => m_icon;

        #endregion

        #region Construction

        protected WorkerBase(WorkerCategory category, ClipboardContent supportedContent, ISettings mSettings, object? icon = null)
        {
            m_supportedContent = supportedContent;
            m_settings = mSettings;
            m_category = category;
            m_icon = icon;
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
        public virtual async Task HandleAsync(SendNotification sendNotification) =>
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleAsync)}");

        // Handle a click event that requires CSV string data
        public virtual async Task HandleCsvAsync(string csvData, SendNotification sendNotification) =>
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleCsvAsync)}");

        // Handle a click event that requires CSV stream data
        public virtual async Task HandleCsvStreamAsync(Stream csvData, SendNotification sendNotification) =>
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleCsvStreamAsync)}");

        // Handle a click event that requires Text string data
        public virtual async Task HandleTextAsync(string textData, SendNotification sendNotification) =>
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleTextAsync)}");

        // Handle a click event that requires Text stream data
        public virtual async Task HandleTextStreamAsync(Stream textData, SendNotification sendNotification) => sendNotification("Not Implemented!",
            $"Worker '{this.GetType().ToString()}' has no implementation for handling {nameof(HandleTextStreamAsync)}");

        // Handle a click ebent that requires File List data
        public virtual async Task HandleFilesAsync(List<string> filesAndFolders, SendNotification sendNotification) =>
            sendNotification("Not Implemented!", $"Worker '{this.GetType().ToString()}' has no implementation for {nameof(HandleFilesAsync)}");

        #endregion


        public static IEnumerable<IWorker> CreateWorkers(ISettings settings, Dictionary<string, object> icons, Dictionary<string, IWorkerUi> workerUis)
        {
            var workers = new List<IWorker>();

            // Quick Actions
            if (!icons.TryGetValue("QuickActions", out var quickActionIcon))
            {
                throw new Exception("QuickActions icon not found");
            }
            if (!workerUis.TryGetValue("InspectData", out var inspectDataUi))
            {
                throw new Exception("InspectDataUi not found");
            }

            workers.Add(new QuickActionsWorker(WorkerCategory.QuickActions, settings, quickActionIcon));
            workers.Add(new StructuredDataInlineQueryWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new FreeTextInlineQueryWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new ExternalDataQueryWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new TempTableWorker(WorkerCategory.QuickActions, settings));
            workers.Add(new InspectDataWorker(WorkerCategory.QuickActions, settings, inspectDataUi));

            // Actions
            workers.Add(new DirectIngestWorker(WorkerCategory.Actions, settings));
            workers.Add(new QueueIngestWorker(WorkerCategory.Actions, settings));

            // Management
            workers.Add(new ShareWorker(WorkerCategory.Management, settings));
            workers.Add(new HelpWorker(WorkerCategory.Management, settings));
            return workers;
        }
    }
}
