using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klipboard.Workers
{
    public class QuickActionsWorker : WorkerBase
    {
        public QuickActionsWorker(WorkerCategory category, AppConfig config, object? icon = null)
            : base(category, ClipboardContent.None, config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            string clusterName = m_appConfig.DefaultClusterConnectionString.ToLower().SplitTakeLast("//").SplitFirst(".kusto").ToUpper();
            string targetStr = $"{clusterName}-{m_appConfig.DefaultClusterDatabaseName}";

            var x = content & (ClipboardContent.CSV | ClipboardContent.CSV_Stream);
            if ((content & (ClipboardContent.CSV | ClipboardContent.CSV_Stream)) != ClipboardContent.None)
            {
                return $"Clipboard Table => {targetStr}";
            }
            
            if ((content & (ClipboardContent.Text | ClipboardContent.Text_Stream)) != ClipboardContent.None)
            {
                return $"Clipboard Text => {targetStr}";
            }
            
            if ((content & ClipboardContent.Files) != ClipboardContent.None)
            {
                return $"Clipboard Files => {targetStr}";
            }

            return targetStr;
        }

        public override bool IsMenuEnabled(ClipboardContent content) => true;

        public override string GetToolTipText(ClipboardContent content) => "Click to set the default cluster and database for Quick Actions";
    }
}
