using Klipboard.Utils;


namespace Klipboard
{
    public class InspectFormHandler : IWorkerUi
    {

        public Task<object> ShowDialog(object arg)
        {
            var content = (string)arg;
            var form = new InspectForm(content);
            form.ShowDialog();

            return Task.FromResult<object>(null);
        }
    }
}
