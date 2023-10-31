using Microsoft.Extensions.Caching.Memory;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;

using Klipboard.Utils;


namespace Klipboard
{
    #region NotifcationHelper
    public class NotificationHelper : INotificationHelper
    {
        static readonly MemoryCache m_ObjectCache = new MemoryCache(new MemoryCacheOptions());

        // Actions
        private const string ShowMessageBoxAction = "ShowMessageBox";
        private const string SetClipboardAction = "SetClipboard";
        private const string RunOnClickAction = "RunOnClickAction";

        // Parameters
        private const string TitleArg = "Title";
        private const string ActionArg = "Action";
        private const string MessageArg = "Message";
        private const string CacheObjectIdArg = "CacheObjectId";


        public NotificationHelper(ClipboardHelper clipboardHelper)
        {
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Obtain the arguments from the notification
                ToastArguments args = ToastArguments.Parse(toastArgs.Argument);

                // Obtain any user input (text boxes, menu selections) from the notification
                ValueSet userInput = toastArgs.UserInput;

                if (!args.TryGetValue(ActionArg, out var action))
                {
                    return;
                }

                switch (action)
                {
                    case ShowMessageBoxAction:
                        args.TryGetValue(TitleArg, out var title);
                        args.TryGetValue(MessageArg, out var message);
                        args.TryGetValue(CacheObjectIdArg, out var detailsId);
                        var details = m_ObjectCache.Get(detailsId) as string;

                        if (details != null)
                        {
                            m_ObjectCache.Remove(detailsId);
                            clipboardHelper.SetText(details);
                        }
                        
                        new TextViewForm(title, message, details, wordWrap: true).ShowDialog();
                        break;

                    case SetClipboardAction:
                        args.TryGetValue(CacheObjectIdArg, out var textId);
                        var text = m_ObjectCache.Get(textId) as string;

                        if (text != null)
                        {
                            m_ObjectCache.Remove(textId);
                            clipboardHelper.SetText(text);
                        }
                        break;

                    case RunOnClickAction:
                        args.TryGetValue(CacheObjectIdArg, out var actionId);
                        var onClick = m_ObjectCache.Get(actionId) as Action;

                        if (onClick != null)
                        {
                            m_ObjectCache.Remove(actionId);
                            Task.Run(() =>
                            {
                                try
                                {
                                    onClick.Invoke();
                                }
                                catch(Exception ex)
                                {
                                    Logger.Log.Error(ex, "notifcation Action ended with exception");
                                }
                                
                            });
                        }
                        break;

                }
            };
        }

        public void ShowBasicNotification(string title, string message, int timeoutSeconds = AppConstants.DefaultNotificationTime, Action? onClick = null, string? onClickButton = null)
        {
            var builder = new ToastContentBuilder();
                
            builder
                .AddText(title)
                .AddText(message);

            if (onClick != null)
            {
                var actionId = Guid.NewGuid().ToString();
                var buttonArgs = new ToastArguments()
                .Add(ActionArg, RunOnClickAction)
                .Add(CacheObjectIdArg, actionId);

                m_ObjectCache.Set(actionId, onClick);

                if (string.IsNullOrWhiteSpace(onClickButton))
                {
                    builder.AddToastActivationInfo(buttonArgs.ToString(), ToastActivationType.Foreground);
                }
                else
                {
                    builder.AddButton(onClickButton, ToastActivationType.Foreground, buttonArgs.ToString());
                }
            }

            builder.Show(toast =>
                {
                    if (timeoutSeconds > 0)
                        toast.ExpirationTime = DateTime.Now.AddSeconds(timeoutSeconds);

                    toast.Priority = ToastNotificationPriority.High;
                });
        }

        public void ShowExtendedNotification(string title, string shortMessage, string extraDetails, int timeoutSeconds = AppConstants.DefaultNotificationTime)
        {
            var cahceId = Guid.NewGuid().ToString();
            var buttonArgs = new ToastArguments()
                .Add(ActionArg, ShowMessageBoxAction)
                .Add(TitleArg, title)
                .Add(MessageArg, shortMessage)
                .Add(CacheObjectIdArg, cahceId);

            m_ObjectCache.Set(cahceId, extraDetails, TimeSpan.FromHours(1));

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

        public void CopyResultNotification(string title, string shortMessage, string text, int timeoutSeconds = AppConstants.DefaultNotificationTime)
        {
            var cahceId = Guid.NewGuid().ToString();
            var buttonArgs = new ToastArguments()
                .Add(ActionArg, SetClipboardAction)
                .Add(CacheObjectIdArg, cahceId);

            m_ObjectCache.Set(cahceId, text, TimeSpan.FromHours(1));

            new ToastContentBuilder()
                .AddText(title)
                .AddText(shortMessage)
                .AddButton("Copy Result", ToastActivationType.Foreground, buttonArgs.ToString())
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
