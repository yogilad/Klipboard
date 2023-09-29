using System.Diagnostics;
using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class HelpWorker : WorkerBase
    {
        private const string Help           = "Help";
        private const string Share          = "Share Klipboard";
        private const string FreeCluster    = "Try Kusto For Free";
        private const string Report         = "Report an Issue";
        private const string Updates        = "Check For Updates";
        private const string SignOut        = "Sign Out of AAD";
        private const string About          = "About";


        public HelpWorker(ISettings settings)
            : base(ClipboardContent.None, settings, new List<string> { Help, Share, FreeCluster, Report, Updates, SignOut, About })
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Help";

        public override bool IsMenuEnabled(ClipboardContent content) => true;

        public override bool IsMenuVisible() => true;

        public override async Task HandleAsync(SendNotification sendNotification, string? chosenOption)
        {
            switch(chosenOption)
            {
                case Help:
                    OpSysHelper.InvokeLink("https://github.com/yogilad/Klipboard#readme");
                    break;

                case About:
                    var msg = $"Version '{AppConstants.ApplicationVersion}'\nDeveloped by Yochai Gilad.\nhttps://github.com/yogilad/Klipboard/";
                    sendNotification(AppConstants.ApplicationName, msg);
                    break;

                case SignOut:
                    var tokenCacheInstance = Kusto.Data.Security.UserTokenFileCache.Instance;

                    if (tokenCacheInstance != null && tokenCacheInstance.IsInitialized)
                    {
                        tokenCacheInstance.Clear();
                    }
                    break;

                case Report:
                    OpSysHelper.InvokeLink("https://github.com/yogilad/Klipboard/issues");
                    break;

                case Share:
                    ShareViaEmail();
                    break;

                case FreeCluster:
                    OpSysHelper.InvokeLink("https://dataexplorer.azure.com/freecluster");
                    break;

                case Updates:
                    if (await VersionHelper.CheckForNewVersion())
                    {
                        sendNotification(AppConstants.ApplicationName, $"Version {VersionHelper.LatestVersion} is available for download");
                    }
                    else 
                    {
                        sendNotification(AppConstants.ApplicationName, "You are running the latest version.");
                    }
                    break;
            }

            return;
        }

        public void ShareViaEmail()
        {
            var subject = "Have You Tried Klipboard for Kusto?";
            var body =
@"Hi, 

I'm using Klipboard for Kusto, and I think you'd find it useful. 
You can get it from https://github.com/yogilad/Klipboard/";

            var link = $"mailto:?subject={subject}&body={Uri.EscapeUriString(body)}";

            OpSysHelper.InvokeLink(link);
        }
    }
}
