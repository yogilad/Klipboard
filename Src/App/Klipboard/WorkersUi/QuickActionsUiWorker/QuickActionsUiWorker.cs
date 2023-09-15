using Klipboard.Utils;
using Klipboard.Workers;
using Klipboard.WorkersUi.QuickActionsUiWorker;


namespace Klipboard
{
    public class QuickActionsUiWorker : QuickActionsWorker
    {
        QuickActionsTargetSelector m_ui;

        public QuickActionsUiWorker(ISettings settings)
        : base(settings)
        {
            m_ui = new QuickActionsTargetSelector(settings);
        }

        public override Task<QuickActionsUserSelection> PromptUser()
        {
            m_ui.ShowDialog();

            return Task.FromResult(m_ui.UserSelection);
        }
    }
}

