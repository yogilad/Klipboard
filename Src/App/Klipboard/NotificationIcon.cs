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
        private NotifyIcon m_notifyIcon;
        private ContextMenuStrip m_contextMenuStrip;
        private IClipboardHelper m_clipboardHelper;

        private class ToolTipTag
        {
            public bool EnableFollowsClipboardContent;
            public bool NameFollowsClipboardContent;
        }

        public NotificationIcon(AppConfig config, IClipboardHelper clipboardHelper, IEnumerable<IWorker> workers)
        {
            m_clipboardHelper = clipboardHelper;
            m_components = new System.ComponentModel.Container();

            // Create the NotifyIcon.
            m_notifyIcon = new NotifyIcon(this.m_components);
            m_notifyIcon.Icon = ResourceLoader.GetIcon();

            // Set the context menu
            m_contextMenuStrip = new ContextMenuStrip();
            m_notifyIcon.ContextMenuStrip = m_contextMenuStrip;

            m_contextMenuStrip.Items.Add("MyFreeCluster Quick Actions", m_notifyIcon.Icon.ToBitmap());
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = "Click to Change Default Cluster";

            m_contextMenuStrip.Items.Add("Paste Data to Inline Query", null, InlineQuery_OnClick);
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = "Invoke a datatable query on one small file or 20KB of clipboard tabular data";
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = new ToolTipTag()
            {
                EnableFollowsClipboardContent = true,
                NameFollowsClipboardContent = true,
            };

            m_contextMenuStrip.Items.Add("Paste Data to External Data Query", null, InlineQuery_OnClick);
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = "Upload clipboard tabular data or one file to a blob and invoke a an external data query on it";
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = new ToolTipTag()
            {
                EnableFollowsClipboardContent = true,
                NameFollowsClipboardContent = true,
            };
            
            m_contextMenuStrip.Items.Add("Paste Data to Temporay Table", null, InlineQuery_OnClick);
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = "Upload clipboard tabular data or files to a temporary table and invoke a query on it";
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = new ToolTipTag()
            {
                EnableFollowsClipboardContent = true,
                NameFollowsClipboardContent = true,
            };

            m_contextMenuStrip.Items.Add(new ToolStripSeparator());
            m_contextMenuStrip.Items.Add("Paste Data to Table", null, InlineQuery_OnClick);
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = "Upload clipboard tabular data or up to 100 files to a table";
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = new ToolTipTag()
            {
                EnableFollowsClipboardContent = true,
                NameFollowsClipboardContent = true,
            };

            m_contextMenuStrip.Items.Add("Queue Data to Table", null, InlineQuery_OnClick);
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].ToolTipText = "Queue clipboard tabular data or any number of files to a table";
            m_contextMenuStrip.Items[m_contextMenuStrip.Items.Count - 1].Tag = new ToolTipTag()
            {
                EnableFollowsClipboardContent = true,
                NameFollowsClipboardContent = true,
            };

            m_contextMenuStrip.Items.Add(new ToolStripSeparator());
            m_contextMenuStrip.Items.Add("Settings", null, Settings_OnClick);
            m_contextMenuStrip.Items.Add("Help", null, Help_OnClick);
            m_contextMenuStrip.Items.Add("Share", null, Share_OnClick);
            m_contextMenuStrip.Items.Add("Exit", null, Exit_OnClick);

            // Init the menu items
            if (config.DevMode)
            {
                m_contextMenuStrip.Items.Add(new ToolStripSeparator());
                m_contextMenuStrip.Items.Add("*** Debug Items ***");
                m_contextMenuStrip.Items.Add("Paste Clipboard to Desktop", null, Paste_OnClick);
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
            string textToFind = content == ClipboardContent.CSV ? "Files" : "Data";
            string textToSet = content == ClipboardContent.CSV ? "Data" : "Files";

            for(int i = 0; i < m_contextMenuStrip.Items.Count; i++)
            {
                var menuItem = m_contextMenuStrip.Items[i];
                var menuTag = menuItem.Tag as ToolTipTag;
                if (menuTag == null)
                {
                    continue;
                }

                if (menuTag.EnableFollowsClipboardContent)
                {
                    switch (content)
                    {
                        // TODO - How to handle arbitrary text clipboard content???
                        case ClipboardContent.CSV:
                        case ClipboardContent.Files:
                            menuItem.Enabled = true;
                            break;

                        default:
                            menuItem.Enabled = false;
                            break;
                    }
                }

                if (menuTag.NameFollowsClipboardContent)
                {
                    switch (content)
                    {
                        case ClipboardContent.CSV:
                        case ClipboardContent.Files:
                            menuItem.Text = menuItem.Text.Replace(textToFind, textToSet);
                            break;

                        default:
                            menuItem.Enabled = false;
                            break;
                    }
                }

            }
        }

        private void Settings_OnClick(object? Sender, EventArgs e)
        {
            var x = e;
        }

        private void Paste_OnClick(object? Sender, EventArgs e)
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

        private void InlineQuery_OnClick(object? Sender, EventArgs e)
        {
            var content = m_clipboardHelper.GetClipboardContent();
            string? queryLink;

            switch (content)
            {
                case ClipboardContent.CSV:
                    if (!m_clipboardHelper.TryGetDataAsString(out var data))
                    {
                        return;
                    }

                    if (data == null || data.Length > 20480)
                    {
                        return;
                    }

                    var success = TabularDataHelper.TryConvertTableToInlineQueryLink(
                        "https://kvcd8ed305830f049bbac1.northeurope.kusto.windows.net",
                        "MyDatabase",
                        data,
                        "\t",
                        out queryLink);

                    if (!success || queryLink == null || queryLink.Length > 10240)
                    {
                        return;
                    }

                    break;

                default:
                    return;
            }

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = queryLink,
                UseShellExecute = true
            });
        }

        private void Help_OnClick(object? Sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/yogilad/Klipboard/blob/main/README.md",
                UseShellExecute = true
            }) ;
        }

        private void Share_OnClick(object? Sender, EventArgs e)
        {
            var subject = "Have You Tried Klipboard for Kusto?";
            var body = @"Hi, I'm using Klipboard for Kusto and I think you'd find it useful. You can get it in https://github.com/yogilad/Klipboard/blob/main/README.md";

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = $"mailto:?subject={subject}&body={body}",
                UseShellExecute = true
            });
        }

        private void Exit_OnClick(object? Sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}