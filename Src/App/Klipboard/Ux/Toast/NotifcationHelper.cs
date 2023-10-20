using Kusto.Cloud.Platform.Utils;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;


namespace Klipboard
{
    public class ProgressNotificationUpdater
    {
        private readonly string m_tag;
        private long m_sequence;
        
        public ProgressNotificationUpdater(string tag) 
        { 
            m_tag = tag;
            m_sequence = 1;
        }

        public void UpdateProgress(string status, double progress, string progressString)
        {
            var data = new NotificationData
            {
                SequenceNumber = (uint) Interlocked.Increment(ref m_sequence)
            };

            // normalize
            progress = Math.Max(progress, 0);
            progress = Math.Min(progress, 1);

            // Assign new values
            // Note that you only need to assign values that changed. In this example
            // we don't assign progressStatus since we don't need to change it
            data.Values["progressStatus"] = status;
            data.Values["progressValue"] = progress.ToString();
            data.Values["progressValueString"] = progressString;
            

            // Update the existing notification's data by using tag/group
            ToastNotificationManagerCompat.CreateToastNotifier().Update(data, m_tag);
        }
    }

    public static class NotifcationHelper
    {
        public const int PersistNotificationTime = 0;
        public const int DefaultNotificationTime = 30;

        static NotifcationHelper()
        {
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Obtain any user input (text boxes, menu selections) from the notification
                ValueSet userInput = toastArgs.UserInput;

                if (!args.TryGetValue("action", out var action))
                {
                    return;
                }

                switch (action)
                {
                    case "MessageBox":
                        args.TryGetValue("title", out var title);
                        args.TryGetValue("message", out var message);
                        MessageBox.Show(message, title, MessageBoxButtons.OK);
                        break;
                }
            };
        }

        /// <summary>
        /// Show a toast notification with a short text
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="timeoutSeconds">Number of seconds before hiding the notification. 0 to persist indefinitely.</param>
        public static void ShowBasicNotification(string title, string message, int timeoutSeconds = DefaultNotificationTime)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show(toast =>
                {
                    if (timeoutSeconds > 0)
                        toast.ExpirationTime = DateTime.Now.AddSeconds(timeoutSeconds);

                    toast.Priority = ToastNotificationPriority.High;
                });
        }

        /// <summary>
        /// Show a text notification with an extended message details button 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="shortMessage">A short message to display in the notification</param>
        /// <param name="extraDetails">Extra details to show when button is pressed</param>
        /// <param name="timeoutSeconds">Number of seconds before hiding the notification. 0 to persist indefinitely.</param>
        public static void ShowExtendedNotification(string title, string shortMessage, string extraDetails, int timeoutSeconds = DefaultNotificationTime)
        {
            var buttonArgs = new ToastArguments()
                .Add("action", "MessageBox")
                .Add("title", title)
                .Add("message", extraDetails);

            new ToastContentBuilder()
                .AddText(title)
                .AddText(shortMessage)
                .AddButton("See full details", ToastActivationType.Foreground, buttonArgs.ToString())
                .Show(toast =>
                {
                    if (timeoutSeconds > 0)
                        toast.ExpirationTime = DateTime.Now.AddSeconds(timeoutSeconds);

                    toast.Priority = ToastNotificationPriority.High;
                });
        }

        public static ProgressNotificationUpdater ShowProgressNotification(string title, string shortMessage)
        {
            // Define a tag (and optionally a group) to uniquely identify the notification, in order update the notification data later;
            string tag = Guid.NewGuid().ToString();
            var updater = new ProgressNotificationUpdater(tag);

            // Construct the toast content with data bound fields
            new ToastContentBuilder()
                .AddText(title)
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Title = shortMessage,
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressValueString"),
                    Status = new BindableString("progressStatus")
                })
                .Show(toast =>
                {
                    toast.Tag = tag;
                    toast.Data = new NotificationData();
                    toast.Data.Values["progressValue"] = "0.0";
                    toast.Data.Values["progressValueString"] = "";
                    toast.Data.Values["progressStatus"] = "";
                    toast.Data.SequenceNumber = 1;
                });

            return updater;
        }
    }
}
