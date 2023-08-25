using Klipboard.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Klipboard
{
    public class NotificationIcon : IDisposable
    {
        private System.ComponentModel.IContainer m_components;
        private NotifyIcon m_notifyIcon;
        private ContextMenuStrip m_contextMenuStrip;

        private readonly int m_inlineQueryButtonId;
        private readonly int m_pasteToFileButtonId;
        private readonly int m_settingsButtonId;
        private readonly int m_helpButtonId;
        private readonly int m_exitButtonId;

        public NotificationIcon(AppConfig config)
        {
            m_components = new System.ComponentModel.Container();

            // Create the NotifyIcon.
            m_notifyIcon = new NotifyIcon(this.m_components);
            m_notifyIcon.Icon = ResourceLoader.GetIcon();

            // Set the context menu
            m_contextMenuStrip = new ContextMenuStrip();
            m_notifyIcon.ContextMenuStrip = m_contextMenuStrip;

            m_inlineQueryButtonId = m_contextMenuStrip.Items.Count;
            m_contextMenuStrip.Items.Add("Paste Table to Inline Query", null, InlineQuery_OnClick);

            m_helpButtonId = m_contextMenuStrip.Items.Count;
            m_contextMenuStrip.Items.Add("Help", null, Help_OnClick);

            m_exitButtonId = m_contextMenuStrip.Items.Count;
            m_contextMenuStrip.Items.Add("Exit", null, Exit_OnClick);

            // Init the menu items
            if (config.DevMode)
            {
                m_contextMenuStrip.Items.Add("Debug Items");
                m_settingsButtonId = m_contextMenuStrip.Items.Count;
                m_contextMenuStrip.Items.Add("Settings", null, Settings_OnClick);

                m_pasteToFileButtonId = m_contextMenuStrip.Items.Count;
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
            var content = ClipboardHelper.GetClipboardContent();
            var menuItem = m_contextMenuStrip.Items[m_inlineQueryButtonId];

            switch (content)
            {
                case ClipboardHelper.Content.CSV:
                    menuItem.Enabled = true;
                    menuItem.Text= "Paste Table to Inline Query";
                    break;

                case ClipboardHelper.Content.DropFiles:
                    menuItem.Enabled = false;
                    menuItem.Text = "Paste Files to Inline Query";
                    break;

                default:
                    menuItem.Enabled = false ;
                    break;
            }
        }

        private void Settings_OnClick(object? Sender, EventArgs e)
        {
            var x = e;
        }

        private void Paste_OnClick(object? Sender, EventArgs e)
        {
            if (ClipboardHelper.TryGetDataAsMemoryStream(out var stream))
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

            if (ClipboardHelper.TryGetDataAsString(out var data))
            {
                var path = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Desktop\\ClipboardData.txt");

                File.WriteAllLines(path, new string[] { data });
            }
        }

        private void InlineQuery_OnClick(object? Sender, EventArgs e)
        {
            var content = ClipboardHelper.GetClipboardContent();
            string? queryLink;

            switch (content)
            {
                case ClipboardHelper.Content.CSV:
                    if (!ClipboardHelper.TryGetDataAsString(out var data))
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
                FileName = "https://github.com/yogilad/KustoCompanion/blob/main/README.md",
                UseShellExecute = true
            });
        }

        private void Exit_OnClick(object? Sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}