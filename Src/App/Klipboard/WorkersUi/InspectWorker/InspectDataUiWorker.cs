using Klipboard.Utils;
using Klipboard.Workers;

namespace Klipboard
{
    public class InspectDataUiWorker : InspectDataWorker
    {
        public InspectDataUiWorker(ISettings settings) 
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
