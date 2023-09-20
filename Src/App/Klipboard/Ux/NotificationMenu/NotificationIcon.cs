using Klipboard.Utils;
using Klipboard.Workers;
using Kusto.Cloud.Platform.Utils;

namespace Klipboard
{
    public record WorkerUxConfig(IWorker Worker, WorkerCategory Category, object? Icon = null);
    public record MenuTag(IWorker Worker, string? chosenOption = null, bool hasSubOptions = false);


    public class NotificationIcon : IDisposable
    {
        private System.ComponentModel.IContainer m_components;
        private NotifyIcon m_notifyIcon;
        private ContextMenuStrip m_contextMenuStrip;
        private IClipboardHelper m_clipboardHelper;


        public NotificationIcon(List<WorkerUxConfig> workerConfig, IClipboardHelper clipboardHelper)
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

        private void BuildMenu(List<WorkerUxConfig> workerConfig)
        {
            WorkerUxConfig? previousWorker = null;

            foreach (var config in workerConfig)
            {
                if (previousWorker != null && previousWorker.Category != config.Category)
                {
                    m_contextMenuStrip.Items.Add(new ToolStripSeparator());
                }

                var hasSubOptions = config.Worker.SubMenuOptions.SafeFastAny();
                var item = new ToolStripMenuItem(config.Worker.GetMenuText(ClipboardContent.None), 
                    image: (config.Icon as Icon)?.ToBitmap(), 
                    onClick: (hasSubOptions)? null: MenuItem_OnClick)
                {
                    ToolTipText = config.Worker.GetToolTipText(),
                    Tag = new MenuTag(config.Worker, hasSubOptions: hasSubOptions),
                };

                m_contextMenuStrip.Items.Add(item);

                if (hasSubOptions)
                {
                    foreach (var subOption in config.Worker.SubMenuOptions)
                    {
                        var subItem = new ToolStripMenuItem(subOption, image: null, MenuItem_OnClick)
                        {
                            Tag = new MenuTag(config.Worker, chosenOption: subOption),
                        };

                        item.DropDownItems.Add(subItem);
                    }
                }

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
                var tag = menuItem.Tag as MenuTag;

                if (tag == null) 
                {
                    continue;
                }

                menuItem.Visible = tag.Worker.IsMenuVisible();

                if (menuItem.Visible)
                {
                    menuItem.Enabled = tag.Worker.IsMenuEnabled(content);
                    menuItem.Text = tag.Worker.GetMenuText(content);
                }
            }
        }

        private void MenuItem_OnClick(object? Sender, EventArgs e)
        {
            var menuItem = Sender as ToolStripMenuItem;
            var tag = menuItem?.Tag as MenuTag;

            if (tag == null)
            {
                return;
            }

            tag.Worker.OnClick(m_clipboardHelper, SendNotification, tag.chosenOption);
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
