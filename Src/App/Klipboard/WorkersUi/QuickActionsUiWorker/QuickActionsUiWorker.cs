using Klipboard.Utils;
using Klipboard.Workers;
using Klipboard.WorkersUi.QuickActionsUiWorker;

namespace Klipboard;

public class QuickActionsUiWorker : QuickActionsWorker
{
    QuickActionsTargetSelector m_ui;

    public QuickActionsUiWorker(WorkerCategory category, ISettings settings, object? icon = null)
    : base(category, settings, icon)
    {
        m_ui = new QuickActionsTargetSelector();
    }

    public override Task<QuickActionsUserSelection> PromptUser()
    {
        m_ui.Init(m_settings);
        m_ui.ShowDialog();

        return Task.FromResult(m_ui.UserSelection);
    }
}
