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
        void ShowBasicNotification(string title, string message, int timeoutSeconds = AppConstants.DefaultNotificationTime);

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
        /// <param name="title"></param>
        /// <param name="shortMessage"></param>
        /// <returns>ProgressNotificationUpdater class instance linked to the new created notification</returns>
        IProgressNotificationUpdater ShowProgressNotification(string title, string shortMessage);
    }

    public interface IProgressNotificationUpdater
    {
        /// <summary>
        /// Update the progress of the notification
        /// </summary>
        /// <param name="status">the current operation stage</param>
        /// <param name="progress">A numeric progress between 0 and 1</param>
        /// <param name="progressString">A string progress</param>
        void UpdateProgress(string status, double progress, string progressString);
    }
}
