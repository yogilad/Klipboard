using Klipboard.Utils;
using Kusto.Cloud.Platform.Utils;


namespace Klipboard.Workers
{
    public abstract class QuickActionsWorker : WorkerBase
    {
        public class QuickActionsUserSelection
        {
            public bool UserConfirmedSelection;
            public int CurrentClusterIndex;
            public string CurrentDatabase;

            public QuickActionsUserSelection()
            {
                Reset();
            }

            public void Reset()
            {
                UserConfirmedSelection = false;
                CurrentClusterIndex = -1;
                CurrentDatabase = string.Empty;
            }
        }

        public QuickActionsWorker(WorkerCategory category, ISettings settings, object? icon = null)
            : base(category, ClipboardContent.None, settings, icon)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            var config = m_settings.GetConfig();
            string clusterName = config.ChosenCluster.ConnectionString.ToLower().SplitTakeLast("//").SplitFirst(".").ToUpper();

            // TODO - the correct way to do this is to have some framework which qualifies KVCs by the result of .show vesion once on creation.
            if (clusterName.StartsWith("KVC") && clusterName.Length >= 20)
            {
                clusterName = "MyFreeCluster";
            }

            string targetStr = $"{clusterName}/{config.ChosenCluster.DatabaseName}";

            if ((content & (ClipboardContent.CSV | ClipboardContent.CSV_Stream)) != ClipboardContent.None)
            {
                return $"Clipboard Table => {targetStr}";
            }

            if ((content & (ClipboardContent.Text | ClipboardContent.Text_Stream)) != ClipboardContent.None)
            {
                return $"Clipboard Text => {targetStr}";
            }

            if ((content & ClipboardContent.Files) != ClipboardContent.None)
            {
                return $"Clipboard Files => {targetStr}";
            }

            return targetStr;
        }

        public override bool IsMenuVisible(ClipboardContent content) => true;

        public override bool IsMenuEnabled(ClipboardContent content) => true;

        public override string GetToolTipText(ClipboardContent content) => "Click to set the default cluster and database for Quick Actions";

        public override async Task HandleAsync(SendNotification sendNotification)
        {
            var result = await PromptUser();

            if (result.UserConfirmedSelection)
            {
                var sourceConfig = m_settings.GetConfig();
                var targetConfig =  sourceConfig with
                {
                    DefaultClusterIndex = result.CurrentClusterIndex,
                    ChosenCluster = sourceConfig.KustoConnectionStrings[result.CurrentClusterIndex] with { DatabaseName = result.CurrentDatabase}
                };

                await m_settings.UpdateConfig(targetConfig);
            }
        }

        public abstract Task<QuickActionsUserSelection> PromptUser();
    }
}
