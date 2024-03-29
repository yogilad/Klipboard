﻿using Kusto.Cloud.Platform.Utils;

using Klipboard.Utils;


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

        public QuickActionsWorker(ISettings settings, INotificationHelper notificationHelper)
            : base(ClipboardContent.None, settings, notificationHelper)
        {
        }

        public override string GetMenuText(ClipboardContent content)
        {
            var config = m_settings.GetConfig();
            string clusterName = GetClusterName(config.ChosenCluster.ConnectionString);

            string targetStr = $"{clusterName} / {config.ChosenCluster.DatabaseName}";

            if ((content & (ClipboardContent.CSV | ClipboardContent.CSV_Stream)) != ClipboardContent.None)
            {
                return $"Clipboard Table ➜ {targetStr}";
            }

            if ((content & (ClipboardContent.Text | ClipboardContent.Text_Stream)) != ClipboardContent.None)
            {
                return $"Clipboard Text ➜ {targetStr}";
            }

            if ((content & ClipboardContent.Files) != ClipboardContent.None)
            {
                return $"Clipboard Files ➜ {targetStr}";
            }

            return targetStr;
        }

        public override bool IsMenuVisible() => true;

        public override bool IsMenuEnabled(ClipboardContent content) => true;

        public override string GetToolTipText() => "Click to set the default cluster and database for Quick Actions";

        public abstract Task<QuickActionsUserSelection> PromptUser();

        public override async Task HandleAsync(string? chosenOption)
        {
            var result = await PromptUser();

            if (result.UserConfirmedSelection && !ComeAndJoinTheBigBoys(result.CurrentDatabase))
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

        private string GetClusterName(string connectionString)
        {
            string clusterName;

            if (Uri.TryCreate(connectionString, UriKind.Absolute, out var uri))
            {
                clusterName = uri.Host.ToUpper();

                if (clusterName.Contains(".KUSTO")) 
                {
                    clusterName = clusterName.SplitFirst(".");
                }
            }
            else
            {
                clusterName = connectionString.SplitTakeLast("//").SplitFirst(".").SplitFirst(":").ToUpper();
            }

            // TODO - the correct way to do this is to have some framework which qualifies KVCs by the result of .show version once on creation.
            if (clusterName.StartsWith("KVC") && clusterName.Length >= 20)
            {
                var tempName = clusterName.ToLower();
                clusterName = $"MyFreeCluster [{tempName.Substring(0, 3)} {tempName.Substring(3, 5)}]";
            }

            return clusterName;
        }

        #region
        public bool ComeAndJoinTheBigBoys(string rubbish)
        {
            var wrench = HashString(rubbish);
            const long Abracadabra = -6357484702770583501;
            const long HocusPocus = 248857055548952305;

            switch (wrench)
            {
                case Abracadabra:
                    AppConstants.DevMode = true;
                    return true;

                case HocusPocus:
                    AppConstants.DevMode = false;
                    return true;
            }

            return false;
        }

        public long HashString(string str)
        {
            str = str.Trim().ToLower();

            using var S256 = System.Security.Cryptography.SHA256.Create();
            var hash = S256.ComputeHash(System.Text.Encoding.ASCII.GetBytes(str));
            using var reader = new BinaryReader(new MemoryStream(hash));

            return reader.ReadInt64();
        }
        #endregion
    }
}
