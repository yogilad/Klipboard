using Klipboard.Utils;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;


namespace Klipboard
{
    #region NotifcationHelper
    public class NotificationHelper : INotificationHelper
    {
        static NotificationHelper()
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
                        args.TryGetValue("details", out var details);
                        new TextViewForm(title, message, details, wordWrap: true).ShowDialog();
                        break;
                }
            };
        }

        public void ShowBasicNotification(string title, string message, int timeoutSeconds = AppConstants.DefaultNotificationTime)
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

        public void ShowExtendedNotification(string title, string shortMessage, string extraDetails, int timeoutSeconds = AppConstants.DefaultNotificationTime)
        {
            var buttonArgs = new ToastArguments()
                .Add("action", "MessageBox")
                .Add("title", title)
                .Add("message", shortMessage)
                .Add("details", extraDetails);


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

        public IProgressNotificationUpdater ShowProgressNotification(string title, string shortMessage, string step = "", string progressString = "")
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
                    toast.Data.Values["progressValueString"] = progressString;
                    toast.Data.Values["progressStatus"] = step;
                    toast.Data.SequenceNumber = 1;
                });

            return updater;
        }
    }
    #endregion

    #region ProgressNotificationUpdater
    public class ProgressNotificationUpdater : IProgressNotificationUpdater
    {
        private readonly string m_tag;
        private long m_sequence;

        internal ProgressNotificationUpdater(string tag)
        {
            m_tag = tag;
            m_sequence = 1;
        }

        public void UpdateProgress(string step, double progress, string progressString)
        {
            var data = new NotificationData
            {
                SequenceNumber = (uint)Interlocked.Increment(ref m_sequence)
            };

            // normalize
            progress = Math.Max(progress, 0);
            progress = Math.Min(progress, 1);

            // Assign new values
            // Note that you only need to assign values that changed. In this example
            // we don't assign progressStatus since we don't need to change it
            data.Values["progressStatus"] = step;
            data.Values["progressValue"] = progress.ToString();
            data.Values["progressValueString"] = progressString;

            // Update the existing notification's data by using tag/group
            ToastNotificationManagerCompat.CreateToastNotifier().Update(data, m_tag);
        }

        public void CloseNotification(int withinSeconds = 0)
        {
            if (withinSeconds == 0)
            {
                ToastNotificationManagerCompat.History.Remove(m_tag);
            }
            else
            {
                Task.Run(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(withinSeconds));
                    ToastNotificationManagerCompat.History.Remove(m_tag);
                });
            }
        }
    }
    #endregion

}
