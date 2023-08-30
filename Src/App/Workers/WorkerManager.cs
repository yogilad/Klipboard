using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Klipboard.Utils;

namespace Klipboard.Workers
{
    public static class WorkerManager
    {
        public static IEnumerable<IWorker> CreateAppWorkers(AppConfig config)
        {
            var workers = new List<IWorker>();

            return workers;
        }

    }
}
