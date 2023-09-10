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

        public NotificationIcon(Settings settings, IClipboardHelper clipboardHelper, IEnumerable<WorkerBase> workers)
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

                m_contextMenuStrip.Items.Add(worker.GetMenuText(ClipboardContent.None), (worker.Icon as Icon)?.ToBitmap(), Worker_OnClick);
                m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = worker.GetToolTipText(ClipboardContent.None);
                m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = worker;

                lastWorkerCategory = worker.Category;
            }

            m_contextMenuStrip.Items.Add(new ToolStripSeparator());
            m_contextMenuStrip.Items.Add("Settings", null, (s, e) => settings.ShowDialog());

            m_contextMenuStrip.Items.Add("Exit", null, Exit_OnClick);

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

        private async void NotifyIcon_OnClick(object? Sender, EventArgs e)
        {
            var content = await m_clipboardHelper.GetClipboardContent().ConfigureAwait(false);

            for(int i = 0; i < m_contextMenuStrip.Items.Count; i++)
            {
                var menuItem = m_contextMenuStrip.Items[i];
                var worker = menuItem.Tag as WorkerBase;
                if (worker == null)
                {
                    continue;
                }

                menuItem.Visible = worker.IsMenuVisible(content);
                menuItem.Enabled = worker.IsMenuEnabled(content);
                menuItem.Text = worker.GetMenuText(content);
                menuItem.ToolTipText = worker.GetToolTipText(content);
            }
        }
        private async void Worker_OnClick(object? Sender, EventArgs e)
        {
            var menuItem = Sender as ToolStripMenuItem;
            var worker = menuItem?.Tag as WorkerBase;

            if (worker == null)
            {
                return;
            }

            var content = await m_clipboardHelper.GetClipboardContent();
            var contentToHandle = content & worker.SupportedContent;

            switch(contentToHandle)
            {
                case ClipboardContent.None:
                    RunWorker(worker, async () => await worker.HandleAsync(SendNotification));
                    break;

                case ClipboardContent.CSV:
                    var csvData = await m_clipboardHelper.TryGetDataAsString();
                    if (string.IsNullOrWhiteSpace(csvData))
                    {
                        SendNotification("Error!", "Failed to get CSV Data from clipboard");
                        return;
                    }

                    RunWorker(worker, async () => await worker.HandleCsvAsync(csvData, SendNotification));
                    break;

                case ClipboardContent.CSV_Stream:
                    var csvStream = await m_clipboardHelper.TryGetDataAsMemoryStream();
                    if (csvStream == null)
                    {
                        SendNotification("Error!", "Failed to get CSV Data from clipboard");
                        return;
                    }

                    RunWorker(worker, async () => await worker.HandleCsvStreamAsync(csvStream, SendNotification));
                    break;

                case ClipboardContent.Text:
                    var textData = await m_clipboardHelper.TryGetDataAsString();
                    if (string.IsNullOrWhiteSpace(textData))
                    {
                        SendNotification("Error!", "Failed to get Text Data from clipboard");
                        return;
                    }

                    RunWorker(worker, async () => await worker.HandleTextAsync(textData, SendNotification));
                    break;

                case ClipboardContent.Text_Stream:
                    var textStream = await m_clipboardHelper.TryGetDataAsMemoryStream();
                    if (textStream == null)
                    {
                        SendNotification("Error!", "Failed to get Text Data from clipboard");
                        return;
                    }

                    RunWorker(worker, async () => await worker.HandleTextStreamAsync(textStream, SendNotification));
                    break;

                case ClipboardContent.Files:
                    var filesAndFolders = await m_clipboardHelper.TryGetFileDropList();
                    if (filesAndFolders == null ||
                            filesAndFolders.Count == 0)
                    {
                        SendNotification("Error!", "Failed to get Files Data from clipboard");
                        return;
                    }

                    RunWorker(worker, async () => await worker.HandleFilesAsync(filesAndFolders, SendNotification));
                    break;

                default:
                    break;
            }
        }

        private void RunWorker(WorkerBase worker, Func<Task> action)
        {
            Task.Run(async () =>
            {
                try
                {
                    await action();
                }
                catch (Exception ex)
                {
                    SendNotification(worker.GetType().ToString(), $"Worker run ended with exception!\n\n{ex}");
                }
            });
        }

        private void Exit_OnClick(object? Sender, EventArgs e)
        {
            Application.Exit();
        }

        private void SendNotification(string title, string message) => m_notifyIcon.ShowBalloonTip(20, title, message, ToolTipIcon.None);
    }
}
