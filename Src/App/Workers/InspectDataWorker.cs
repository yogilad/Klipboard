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
    public class InspectDataWorker : WorkerBase
    {
        public InspectDataWorker(WorkerCategory category, AppConfig config, object? icon = null)
            : base(category, ClipboardContent.None, config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            return $"Inspect Clipboard Content ({content})";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return "Display a preview of Clipboard Data"; ;
        }
    }
}
