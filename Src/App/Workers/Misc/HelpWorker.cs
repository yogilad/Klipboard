using System.Diagnostics;

using Klipboard.Utils;


namespace Klipboard.Workers
{
    public class HelpWorker : WorkerBase
    {
        private const string Help = "Help";
        private const string About = "About";
        private const string Report = "Report in Issue";
        private const string Share = "Share Klipboard";
        private const string FreeCluster = "Try Kusto For Free";

        public HelpWorker(ISettings settings)
            : base(ClipboardContent.None, settings, new List<string> { Help, About, Report, Share, FreeCluster })
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
                    InvokeLink("https://github.com/yogilad/Klipboard#readme");
                    break;

                case About:
                    var msg = $"Version '{AppConstants.ApplicationVersion}'\nDeveloped by Yochai Gilad.\nhttps://github.com/yogilad/Klipboard/";
                    sendNotification(AppConstants.ApplicationName, msg);
                    break;

                case Report:
                    InvokeLink("https://github.com/yogilad/Klipboard/issues");
                    break;

                case Share:
                    ShareViaEmail();
                    break;

                case FreeCluster:
                    InvokeLink("https://dataexplorer.azure.com/freecluster");
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

            InvokeLink(link);
        }

        public void InvokeLink(string link)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            });

            return;
        }
    }
}
