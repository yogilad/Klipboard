using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;


namespace Klipboard
{
    public static class ToastNotifcationHelper
    {
        static ToastNotifcationHelper()
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

        public static void ShowBasicToast(string title, string message)
        {
            var buttonArgs = new ToastArguments();

            buttonArgs.Add("action", "MessageBox");
            buttonArgs.Add("title", title);
            buttonArgs.Add("message", message);

            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .AddButton("See full details", ToastActivationType.Foreground, buttonArgs.ToString())
                .Show(toast =>
                    {
                        toast.ExpirationTime = DateTime.Now.AddMinutes(1);
                        toast.Priority = ToastNotificationPriority.High;
                    });
        }
    }
}
