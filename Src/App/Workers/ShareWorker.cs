using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klipboard.Workers
{
    public class ShareWorker : WorkerBase
    {
        private WorkerCategory m_category;
        private object? m_icon;

        public override WorkerCategory Category => m_category;
        public override object? Icon => m_icon;

        public ShareWorker(WorkerCategory category, object? icon)
        {
            m_category = category;
            m_icon = icon;
        }

        public override string GetText(ClipboardContent content)
        {
            return "Share Klipboard";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return string.Empty;
        }

        public override bool IsEnabled(ClipboardContent content)
        {
            return true;
        }

        public override bool IsVisible(ClipboardContent content)
        {
            return true;
        }

        public override Task RunAsync(IClipboardHelper clipboardHelper, SendNotification sendNotification)
        {
            var subject = "Have You Tried Klipboard for Kusto?";
            var body = @"Hi, I'm using Klipboard for Kusto and I think you'd find it useful. You can get it in https://github.com/yogilad/Klipboard/blob/main/README.md";

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = $"mailto:?subject={subject}&body={body}",
                UseShellExecute = true
            });

            return Task.CompletedTask;
        }
    }
}
