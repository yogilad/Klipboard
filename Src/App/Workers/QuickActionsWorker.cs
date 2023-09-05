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

            if ((content & (ClipboardContent.CSV | ClipboardContent.CSV_Stream)) != ClipboardContent.None)
            {
                return $"Table Text => {targetStr}";
            }
            
            if ((content & (ClipboardContent.CSV | ClipboardContent.CSV_Stream)) != ClipboardContent.None)
            {
                return $"Free Text => {targetStr}";
            }
            
            if ((content & ClipboardContent.Files) != ClipboardContent.None)
            {
                return $"Files => {targetStr}";
            }

            return targetStr;
        }

        public override string GetToolTipText(ClipboardContent content) => "Click to set the default cluster and database for Quick Actions";
    }
}
