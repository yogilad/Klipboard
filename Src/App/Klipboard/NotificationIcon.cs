using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using Klipboard.Utils;
using Klipboard.Workers;

namespace Klipboard
{
    public class NotificationIcon : IDisposable
    {
        private System.ComponentModel.IContainer m_components;
        private Icon m_appLogo;
        private NotifyIcon m_notifyIcon;
        private ContextMenuStrip m_contextMenuStrip;
        private IClipboardHelper m_clipboardHelper;

        private class ToolTipTag
        {
            public bool EnableFollowsClipboardContent;
            public bool NameFollowsClipboardContent;
        }

        public NotificationIcon(AppConfig config, IClipboardHelper clipboardHelper, IEnumerable<WorkerBase> workers)
        {
            m_clipboardHelper = clipboardHelper;
            m_components = new System.ComponentModel.Container();

            // Create the NotifyIcon.
            m_notifyIcon = new NotifyIcon(this.m_components);
            m_appLogo = ResourceLoader.GetIcon();
            m_notifyIcon.Icon = m_appLogo;

            // Set the context menu
            m_contextMenuStrip = new ContextMenuStrip();
            m_notifyIcon.ContextMenuStrip = m_contextMenuStrip;

            WorkerCategory? lastWorkerCategory = null;

            foreach(var worker in workers)
            {
                if (lastWorkerCategory.HasValue && lastWorkerCategory != worker.Category)
                {
                    m_contextMenuStrip.Items.Add(new ToolStripSeparator());
                }

                m_contextMenuStrip.Items.Add(worker.GetText(ClipboardContent.None), (worker.Icon as Icon)?.ToBitmap(), Worker_OnClick);
                m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = worker.GetToolTipText(ClipboardContent.None);
                m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = worker;
            
                lastWorkerCategory = worker.Category;
            }

            m_contextMenuStrip.Items.Add("Paste Data to External Data Query", null, null);
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = "Upload clipboard tabular data or one file to a blob and invoke a an external data query on it";
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = new ToolTipTag()
            {
                EnableFollowsClipboardContent = true,
                NameFollowsClipboardContent = true,
            };
            
            m_contextMenuStrip.Items.Add("Paste Data to Temporay Table", null, null);
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = "Upload clipboard tabular data or files to a temporary table and invoke a query on it";
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = new ToolTipTag()
            {
                EnableFollowsClipboardContent = true,
                NameFollowsClipboardContent = true,
            };

            m_contextMenuStrip.Items.Add(new ToolStripSeparator());
            m_contextMenuStrip.Items.Add("Paste Data to Table", null, null);
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = "Upload clipboard tabular data or up to 100 files to a table";
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = new ToolTipTag()
            {
                EnableFollowsClipboardContent = true,
                NameFollowsClipboardContent = true,
            };

            m_contextMenuStrip.Items.Add("Queue Data to Table", null, null);
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = "Queue clipboard tabular data or any number of files to a table";
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = new ToolTipTag()
            {
                EnableFollowsClipboardContent = true,
                NameFollowsClipboardContent = true,
            };

            m_contextMenuStrip.Items.Add("Exit", null, Exit_OnClick);

            // Init the menu items
            if (config.DevMode)
            {
                m_contextMenuStrip.Items.Add(new ToolStripSeparator());
                m_contextMenuStrip.Items.Add("*** Debug Items ***");
                m_contextMenuStrip.Items.Add("Paste Clipboard to Desktop", null, DebugPasteToDesktop_OnClick);
            }

            // Handle the DoubleClick event to activate the form.
            m_notifyIcon.Click += new EventHandler(NotifyIcon_OnClick);
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

        private void NotifyIcon_OnClick(object? Sender, EventArgs e)
        {
            var content = m_clipboardHelper.GetClipboardContent();

            for(int i = 0; i < m_contextMenuStrip.Items.Count; i++)
            {
                var menuItem = m_contextMenuStrip.Items[i];
                var worker = menuItem.Tag as WorkerBase;
                if (worker == null)
                {
                    continue;
                }

                menuItem.Visible = worker.IsVisible(content);
                menuItem.Enabled = worker.IsEnabled(content);
                menuItem.Text = worker.GetText(content);
            }
        }

        private void DebugPasteToDesktop_OnClick(object? Sender, EventArgs e)
        {
            if (m_clipboardHelper.TryGetDataAsMemoryStream(out var stream))
            {
                using (stream)
                {
                    var path = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Desktop\\ClipboardData.bin");
                    using var fstream = new FileStream(path, FileMode.OpenOrCreate);
                    
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fstream);
                    fstream.Close();
                }
            }

            if (m_clipboardHelper.TryGetDataAsString(out var data))
            {
                var path = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Desktop\\ClipboardData.txt");

                File.WriteAllLines(path, new string[] { data });
            }
        }

        private void Worker_OnClick(object? Sender, EventArgs e)
        {
            var menuItem = Sender as ToolStripMenuItem;
            var worker = menuItem?.Tag as WorkerBase;

            worker?.RunAsync(m_clipboardHelper, (title, message) => m_notifyIcon.ShowBalloonTip(20, title, message, ToolTipIcon.None));
        }

        private void Exit_OnClick(object? Sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}