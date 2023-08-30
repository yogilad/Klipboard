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
    public class InlineQueryWorker : WorkerBase
    {
        private WorkerCategory m_category;
        private object? m_icon;

        public override WorkerCategory Category => m_category;
        public override object? Icon => m_icon;

        public InlineQueryWorker(WorkerCategory category, object? icon)
        {
            m_category = category;
            m_icon = icon;
        }

        public override string GetText(ClipboardContent content)
        {
            var contentStr = content == ClipboardContent.None ? "Data" : content.ToString();
            return $"Paste {contentStr} to Inline Query";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return "Invoke a datatable query on one small file or 20KB of clipboard tabular data";
        }

        public override bool IsEnabled(ClipboardContent content)
        {
            return content != ClipboardContent.None;
        }

        public override bool IsVisible(ClipboardContent content)
        {
            return true;
        }

        public override Task RunAsync(IClipboardHelper clipboardHelper, SendNotification sendNotification)
        {
            return Task.Run(() => RunInlineQuery(clipboardHelper, sendNotification));
        }

        private void RunInlineQuery(IClipboardHelper clipboardHelper, SendNotification sendNotification)
        {
            var content = clipboardHelper.GetClipboardContent();
            string? queryLink;

            switch (content)
            {
                case ClipboardContent.CSV:
                    if (!clipboardHelper.TryGetDataAsString(out var data))
                    {
                        sendNotification("Inline Query", "Failed to get data from Clipboard!");
                        return; 
                    }

                    if (data == null || data.Length > 20480)
                    {
                        sendNotification("Inline Query", "Inline query is limited to 20KB of source data.");
                    }

                    var success = TabularDataHelper.TryConvertTableToInlineQueryLink(
                        "https://kvcd8ed305830f049bbac1.northeurope.kusto.windows.net",
                        "MyDatabase",
                        data,
                        "\t",
                        out queryLink);

                    if (!success || queryLink == null || queryLink.Length > 10240)
                    {
                        sendNotification("Inline Query", "Resulting query link excceds 10KB.");
                        return;
                    }

                    break;

                default:
                    sendNotification("Inline Query", $"Data Type '{content}' is not supported.");
                    return;
            }

            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = queryLink,
                UseShellExecute = true
            });
        }

    }
}
