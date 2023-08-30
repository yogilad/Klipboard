using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Klipboard.Workers
{
    public class InlineQueryWorker : WorkerBase
    {
        private string m_currentCluster = "https://kvcd8ed305830f049bbac1.northeurope.kusto.windows.net";
        private string m_currentDatabase = "MyDatabase";

        public InlineQueryWorker(WorkerCategory category, object? icon)
        : base(category, icon, ClipboardContent.CSV)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            var contentToConsider = content & SupportedContent;
            var contentStr = contentToConsider == ClipboardContent.None ? "Data" : content.ToString();
            return $"Paste {contentStr} to Inline Query";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return "Invoke a datatable query on one small file or 20KB of clipboard tabular data";
        }

        public override bool IsEnabled(ClipboardContent content)
        {
            return (content & SupportedContent) != ClipboardContent.None;
        }

        public override bool IsVisible(ClipboardContent content)
        {
            return true;
        }

        public override Task HandleCsvAsync(string csvData, SendNotification sendNotification)
        {
            return Task.Run(() => HandleCsvData(csvData, sendNotification));
        }

        private void HandleCsvData(string csvData, SendNotification sendNotification)
        {
            if (csvData.Length > 20480)
            {
                sendNotification("Inline Query", "Inline query is limited to 20KB of source data.");
                return;
            }

            var success = TabularDataHelper.TryConvertTableToInlineQueryLink(
                m_currentCluster,
                m_currentDatabase,
                csvData,
                "\t",
                out var queryLink);

            if (!success || queryLink == null || queryLink.Length > 10240)
            {
                sendNotification("Inline Query", "Failed to create query link.");
                return;
            }
            
            if (queryLink.Length > 10240)
            {
                sendNotification("Inline Query", "Resulting query link excceds 10KB.");
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
