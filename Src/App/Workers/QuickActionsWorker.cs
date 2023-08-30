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
    public class QuickActionsWorker : WorkerBase
    {
        private WorkerCategory m_category;
        private object? m_icon;

        public override WorkerCategory Category => m_category;
        public override object? Icon => m_icon;

        public QuickActionsWorker(WorkerCategory category, object? icon)
        {
            m_category = category;
            m_icon = icon;
        }

        public override string GetText(ClipboardContent content)
        {
            return "{cluster}-{database} Quick Actions";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return "Click to set the default cluster and database for Quick Actions";
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
            sendNotification("Not Implemented!", this.GetType().ToString());

            return Task.CompletedTask;
        }
    }
}
