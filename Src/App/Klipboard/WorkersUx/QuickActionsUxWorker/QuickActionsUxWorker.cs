using Klipboard.Utils;
using Klipboard.Workers;


namespace Klipboard
{
    public class QuickActionsUxWorker : QuickActionsWorker
    {
        QuickActionsTargetSelectorForm m_ux;

        public QuickActionsUxWorker(ISettings settings)
        : base(settings)
        {
            m_ux = new QuickActionsTargetSelectorForm(settings);
        }

        public override Task<QuickActionsUserSelection> PromptUser()
        {
            m_ux.ShowDialog();

            return Task.FromResult(m_ux.UserSelection);
        }
    }
}

