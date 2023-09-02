﻿using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Klipboard.Workers
{
    public class QueueIngestWorker : WorkerBase
    {
        public QueueIngestWorker(WorkerCategory category, AppConfig config, object? icon = null)
            : base(category, ClipboardContent.None, config, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            return "Queue Data to Table";
        }

        public override string GetToolTipText(ClipboardContent content)
        {
            return "Queue clipboard tabular data or any number of files to a table"; ;
        }
    }
}