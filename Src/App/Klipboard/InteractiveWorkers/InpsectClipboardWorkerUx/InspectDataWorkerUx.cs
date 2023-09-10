using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Klipboard.Utils;
using Klipboard.Workers;

namespace Klipboard.InteractiveWorkers.InpsectClipboardWorkerUx
{
    internal class InspectDataWorkerUx : InspectDataWorker
    {
        public InspectDataWorkerUx(WorkerCategory category, ISettings settings, object? icon = null)
            : base(category, settings, icon)
        {
        }

        public override Task ShowContent(string clipboardContent)
        {
            var ux = new ClipboardContentForm(clipboardContent);
            ux.ShowDialog();
            return Task.CompletedTask;
        }
    }
}
