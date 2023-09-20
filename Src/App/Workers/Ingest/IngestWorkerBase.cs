using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class IngestWorkerBase : WorkerBase
    {
        public IngestWorkerBase(ISettings settings) 
            : base(ClipboardContent.None, settings)
        {
        }
    }
}
