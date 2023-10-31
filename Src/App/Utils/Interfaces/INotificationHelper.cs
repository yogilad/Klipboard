namespace Klipboard.Utils
{
    public interface INotificationHelper
    {
        /// <summary>
        /// Show a toast notification with a short text
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="timeoutSeconds">Number of seconds before hiding the notification. 0 to persist indefinitely.</param>
        /// <param name="onClick">Optional action to invoke if clicked</param>
        /// <param name="onClickButton">Optional button for action. If not set the action will be called when the notification itself is clicked</param>
        void ShowBasicNotification(string title, string message, int timeoutSeconds = AppConstants.DefaultNotificationTime, Action? onClick = null, string? onClickButton = null);

        /// <summary>
        /// Show a text notification with an extended message details button 
        /// </summary>
        /// <param name="title"></param>
        /// <param name="shortMessage">A short message to display in the notification</param>
        /// <param name="extraDetails">Extra details to show when button is pressed</param>
        /// <param name="timeoutSeconds">Number of seconds before hiding the notification. 0 to persist indefinitely.</param>
        void ShowExtendedNotification(string title, string shortMessage, string extraDetails, int timeoutSeconds = AppConstants.DefaultNotificationTime);

        /// <summary>
        /// Show a progress notification
        /// Returns an object allowing to update progress of the new notification 
        /// </summary>
        /// <param name="title">Notification Title</param>
        /// <param name="shortMessage">Notification Message</param>
        /// <param name="step">The current running step</param>
        /// <param name="progressString">A textual progress string</param>
        /// <returns>ProgressNotificationUpdater class instance linked to the new created notification</returns>
        IProgressNotificationUpdater ShowProgressNotification(string title, string shortMessage, string step = "", string progressString = "");

        /// <summary>
        /// Set a result back to the Cliboard
        /// </summary>
        /// <param name="title">Notification title</param>
        /// <param name="shortMessage">Notification message</param>
        /// <param name="text">Text to copy to the clipboard</param>
        /// <param name="timeoutSeconds">Time to show notification</param>
        void CopyResultNotification(string title, string shortMessage, string text, int timeoutSeconds = AppConstants.DefaultNotificationTime);
    }

    public interface IProgressNotificationUpdater
    {
        /// <summary>
        /// Update the progress of the notification
        /// </summary>
        /// <param name="step">the current running step</param>
        /// <param name="progress">A numeric progress between 0 and 1</param>
        /// <param name="progressString">A textual progress string</param>
        void UpdateProgress(string step, double progress, string progressString);

        /// <summary>
        /// Close a notification within the allotted time
        /// </summary>
        /// <param name="withinSeconds">Seconds to wait before closing the notification</param>
        void CloseNotification(int withinSeconds = 0);
    }
}
