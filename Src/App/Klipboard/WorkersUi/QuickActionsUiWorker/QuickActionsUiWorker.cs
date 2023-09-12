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

    public override async Task HandleAsync(SendNotification sendNotification)
    {
        m_ui.Init(m_settings);
        m_ui.ShowDialog();

        if (m_ui.UserConfirmedSelection)
        {
            // Do something with chosen cluster
            // Do something with chosen DB
        }
    }
}
