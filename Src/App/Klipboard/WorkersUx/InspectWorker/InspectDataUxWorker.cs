using Klipboard.Utils;
using Klipboard.Workers;

namespace Klipboard
{
    public class InspectDataUxWorker : InspectDataWorker
    {
        public InspectDataUxWorker(ISettings settings) 
            : base(settings)
        {
        }

        public override void ShowDialog(string contentType, string size, string content)
        {
            var form = new InspectForm(contentType, size, content);
            form.ShowDialog();
        }
    }
}
