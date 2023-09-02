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
    public class TempTableWorker : WorkerBase
    {
        public TempTableWorker(WorkerCategory category, AppConfig config, object? icon = null)
            : base(category, ClipboardContent.None, config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            return "Paste Data to Temporay Table";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return "Upload clipboard tabular data or up to 100 files to a temporary table and invoke a query on it";
        }
    }
}
