using Klipboard.Utils;


namespace Klipboard
{
    public class NotificationIcon : IDisposable, INotificationIcon
    {
        private System.ComponentModel.IContainer m_components;
        private Icon m_appLogo;
        private NotifyIcon m_notifyIcon;
        private ContextMenuStrip m_contextMenuStrip;

        public NotificationIcon()
        {
            m_components = new System.ComponentModel.Container();

            // Create the NotifyIcon.
            m_notifyIcon = new NotifyIcon(this.m_components);
            m_appLogo = ResourceLoader.GetIcon();
            m_notifyIcon.Icon = m_appLogo;

            // Set the context menu
            m_contextMenuStrip = new ContextMenuStrip();
            m_notifyIcon.ContextMenuStrip = m_contextMenuStrip;

            // Handle the DoubleClick event to activate the form.
            m_notifyIcon.Text = "Klipboard";

            // Dispaly the notification icon
            m_notifyIcon.Visible = true;

        }

        public void Dispose() => Dispose(true);

        public void Dispose(bool disposing)
        {
            // Clean up any components being used.
            if (disposing)
                if (m_components != null)
                    m_components.Dispose();
        }

        private void Exit_OnClick(object? Sender, EventArgs e)
        {
            Application.Exit();
        }

        public void SendNotification(string title, string message)
        {
            m_notifyIcon.ShowBalloonTip(20, title, message, ToolTipIcon.None);
        }

        public object AddWorker(IWorker worker, Func<IWorker?, Task> workerClick)
        {
            var item = new ToolStripMenuItem(worker.GetMenuText(ClipboardContent.None), (worker.Icon as Icon)?.ToBitmap(),
                async (s, e) => await ExceptionUtils.Protect(() => workerClick(worker)))
            {
                ToolTipText = worker.GetToolTipText(ClipboardContent.None)
            };

            m_contextMenuStrip.Items.Add(item);

            return item;
        }

        public void AddSeparator()
        {
            m_contextMenuStrip.Items.Add(new ToolStripSeparator());
        }

        public void UpdateWorker(object item, IWorker worker, ClipboardContent content)
        {
            var menuItem = item as ToolStripMenuItem;
            menuItem.Visible = worker.IsMenuVisible(content);
            menuItem.Enabled = worker.IsMenuEnabled(content);
            menuItem.Text = worker.GetMenuText(content);
            menuItem.ToolTipText = worker.GetToolTipText(content);
        }

        public void AddAdditionalItems(ISettings settings)
        {
            m_contextMenuStrip.Items.Add("Settings", null, (s, e) => ((Settings)settings).ShowDialog());
            m_contextMenuStrip.Items.Add("Exit", null, Exit_OnClick);
        }

        public void SetOnClick(Func<Task> onClick)
        {
            m_notifyIcon.Click += async (_, _) => await ExceptionUtils.Protect(onClick);
        }
    }
}
