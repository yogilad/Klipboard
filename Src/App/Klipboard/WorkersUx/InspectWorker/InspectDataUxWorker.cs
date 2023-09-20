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

        public override void ShowDialog(string content)
        {
            var form = new InspectForm(content);
            form.ShowDialog();
        }
    }
}
