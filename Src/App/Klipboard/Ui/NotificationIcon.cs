using Klipboard.Utils;
using Klipboard.Workers;


namespace Klipboard
{
    public record WorkerUiConfig(IWorker Worker, WorkerCategory Category, object? Icon = null, List<WorkerUiConfig>? SubItems = null);

    public class NotificationIcon : IDisposable
    {
        private System.ComponentModel.IContainer m_components;
        private NotifyIcon m_notifyIcon;
        private ContextMenuStrip m_contextMenuStrip;
        private IClipboardHelper m_clipboardHelper;

        public NotificationIcon(List<WorkerUiConfig> workerConfig, IClipboardHelper clipboardHelper)
        {
            m_clipboardHelper = clipboardHelper;
            m_components = new System.ComponentModel.Container();

            // Create the NotifyIcon.
            m_notifyIcon = new NotifyIcon(this.m_components);
            m_notifyIcon.Icon = ResourceLoader.KustoColorIcon;
            m_notifyIcon.Text = "Klipboard";

            // Set the context menu
            m_contextMenuStrip = new ContextMenuStrip();
            m_notifyIcon.ContextMenuStrip = m_contextMenuStrip;
            BuildMenu(workerConfig);

            // Handle the DoubleClick event to activate the form.
            m_notifyIcon.Click += NotifyIcon_OnClick;

            // Display the notification icon
            m_notifyIcon.Visible = true;
        }

        private void BuildMenu(List<WorkerUiConfig> workerConfig)
        {
            WorkerUiConfig? previousWorker = null;

            foreach (var config in workerConfig)
            {
                if (previousWorker != null && previousWorker.Category != config.Category)
                {
                    m_contextMenuStrip.Items.Add(new ToolStripSeparator());
                }

                var item = new ToolStripMenuItem(config.Worker.GetMenuText(ClipboardContent.None), (config.Icon as Icon)?.ToBitmap(), MenuItem_OnClick)
                {
                    ToolTipText = config.Worker.GetToolTipText(),
                    Tag = config.Worker,
                };

                m_contextMenuStrip.Items.Add(item);


                previousWorker = config;
            }

            m_contextMenuStrip.Items.Add("Exit", null, Exit_OnClick);
        }


        public void Dispose() => Dispose(true);

        public void Dispose(bool disposing)
        {
            // Clean up any components being used.
            if (disposing)
                if (m_components != null)
                    m_components.Dispose();
        }

        private void NotifyIcon_OnClick(object? Sender, EventArgs e)
        {
            var content = m_clipboardHelper.GetClipboardContent();

            for(int i = 0; i < m_contextMenuStrip.Items.Count; i++)
            {
                var menuItem = m_contextMenuStrip.Items[i];
                var worker = menuItem.Tag as IWorker;

                if (worker == null) 
                {
                    continue;
                }

                menuItem.Visible = worker.IsMenuVisible();

                if (menuItem.Visible)
                {
                    menuItem.Enabled = worker.IsMenuEnabled(content);
                    menuItem.Text = worker.GetMenuText(content);
                }
            }
        }

        private void MenuItem_OnClick(object? Sender, EventArgs e)
        {
            var menuItem = Sender as ToolStripMenuItem;
            var worker = menuItem?.Tag as IWorker;

            if (worker == null)
            {
                return;
            }

            worker.OnClick(m_clipboardHelper, SendNotification);
        }


        private void Exit_OnClick(object? Sender, EventArgs e)
        {
            Application.Exit();
        }

        public void SendNotification(string title, string message)
        {
            m_notifyIcon.ShowBalloonTip(20, title, message, ToolTipIcon.None);
        }
    }
}
