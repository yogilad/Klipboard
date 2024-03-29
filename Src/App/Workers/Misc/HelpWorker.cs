﻿using System.Diagnostics;
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
        private const string WhatsNew       = "What's New";


        public HelpWorker(ISettings settings, INotificationHelper notificationHelper)
            : base(ClipboardContent.None, settings, notificationHelper, new List<string> { Help, About, WhatsNew, Share, Report, Updates, SignOut, FreeCluster })
        {
        }

        public override string GetMenuText(ClipboardContent content) => "Help";

        public override bool IsMenuEnabled(ClipboardContent content) => true;

        public override bool IsMenuVisible() => true;

        public override async Task HandleAsync(string? chosenOption)
        {
            switch(chosenOption)
            {
                case Help:
                    OpSysHelper.InvokeLink("https://github.com/yogilad/Klipboard/wiki");
                    break;

                case About:
                    var msg = $"Version '{AppConstants.ApplicationVersion}'\nDeveloped by Yochai Gilad.\nhttps://github.com/yogilad/Klipboard";

                    m_notificationHelper.ShowBasicNotification(
                        AppConstants.ApplicationName, 
                        msg, 
                        onClick: () => OpSysHelper.InvokeLink("https://github.com/yogilad/Klipboard"));

                    break;

                case SignOut:
                    var tokenCacheInstance = Kusto.Data.Security.UserTokenFileCache.Instance;

                    if (tokenCacheInstance != null && tokenCacheInstance.IsInitialized)
                    {
                        tokenCacheInstance.Clear();
                    }
                    break;

                case Report:
                    OpSysHelper.InvokeLink("https://github.com/yogilad/Klipboard/wiki/Reporting-Issues");
                    break;

                case Share:
                    ShareViaEmail();
                    break;

                case FreeCluster:
                    OpSysHelper.InvokeLink("https://dataexplorer.azure.com/freecluster");
                    break;

                case WhatsNew:
                    OpSysHelper.InvokeLink("https://github.com/yogilad/Klipboard/blob/main/ReleaseNotes.md#release-notes");
                    break;

                case Updates:
                    if (await VersionHelper.CheckForNewVersion())
                    {
                        m_notificationHelper.ShowBasicNotification(
                            AppConstants.ApplicationName, 
                            $"Version {VersionHelper.LatestVersion} is available for download",
                            onClick: () => OpSysHelper.InvokeLink("https://github.com/yogilad/Klipboard/releases/latest"),
                            onClickButton: "Go To Download Page");
                    }
                    else 
                    {
                        m_notificationHelper.ShowBasicNotification(AppConstants.ApplicationName, "You are running the latest version.");
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
https://github.com/yogilad/Klipboard/Wiki";

            var link = $"mailto:?subject={subject}&body={Uri.EscapeUriString(body)}";

            OpSysHelper.InvokeLink(link);
        }
    }
}
