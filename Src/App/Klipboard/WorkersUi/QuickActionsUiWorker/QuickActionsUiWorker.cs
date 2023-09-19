using Klipboard.Utils;
using Klipboard.Workers;
using Klipboard.WorkersUi.QuickActionsUiWorker;


namespace Klipboard
{
    public class QuickActionsUiWorker : QuickActionsWorker
    {
        QuickActionsTargetSelectorForm m_ui;

        public QuickActionsUiWorker(ISettings settings)
        : base(settings)
        {
            m_ui = new QuickActionsTargetSelectorForm(settings);
        }

        public override Task<QuickActionsUserSelection> PromptUser()
        {
            m_ui.ShowDialog();

            return Task.FromResult(m_ui.UserSelection);
        }
    }
}

