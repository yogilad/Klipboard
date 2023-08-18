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

        public NotificationIcon()
        {
            m_components = new System.ComponentModel.Container();

            // Create the NotifyIcon.
            m_notifyIcon = new NotifyIcon(this.m_components);
            m_notifyIcon.Icon = ResourceLoader.GetIcon();

            // Set the context menu
            m_contextMenuStrip = new ContextMenuStrip();
            m_notifyIcon.ContextMenuStrip = m_contextMenuStrip;

            // Init the menu items
            m_contextMenuStrip.Items.Add("Settings", null, Settings_OnClick);
            m_contextMenuStrip.Items.Add("Paste not initialized", null, Paste_OnClick);
            m_contextMenuStrip.Items.Add("Help", null, Help_OnClick);
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

        private void NotifyIcon_OnClick(object? Sender, EventArgs e)
        {
            var content = ClipboardHelper.GetClipboardContent();
            var menuItem = m_contextMenuStrip.Items[1];

            switch (content)
            {
                case ClipboardHelper.Content.CSV:
                    menuItem.Enabled = true;
                    menuItem.Text= "Paste Table to Kusto";
                    break;

                case ClipboardHelper.Content.Text:
                    menuItem.Enabled = true;
                    menuItem.Text = "Paste Text to Kusto";
                    break;

                case ClipboardHelper.Content.DropFiles:
                    menuItem.Enabled = true;
                    menuItem.Text = "Paste Files to Kusto";
                    break;

                default:
                    menuItem.Enabled = false ;
                    menuItem.Text = "No Content to Paste";
                    break;
            }
        }

        private void Settings_OnClick(object? Sender, EventArgs e)
        {
            var x = e;
        }

        private void Paste_OnClick(object? Sender, EventArgs e)
        {
            var y = ClipboardHelper.GetDataAsIstream();
            var x = ClipboardHelper.GetDataAsString();
            var z = e;
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