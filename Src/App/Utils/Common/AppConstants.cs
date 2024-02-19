namespace Klipboard.Utils
{
    #region App Constats
    public static class AppConstants
    {
        // Debug Config
#if DEBUG
        public static bool DevMode = true;
        public const bool EnforceInlineQuerySizeLimits = true;
#else
        public static bool DevMode = false;
        public const bool EnforceInlineQuerySizeLimits = true;
#endif

        // Program Constants
        public const int MaxAllowedQueryLengthKB = 12;
        public const int MaxAllowedQueryLength = MaxAllowedQueryLengthKB * 1024;
        public const int MaxAllowedDataLengthKb = MaxAllowedQueryLengthKB * 5;
        public const int MaxAllowedDataLength = MaxAllowedDataLengthKb * 1024;

        public const string UnknownFormat = "unknown";
        public const string TextLinesSchemaStr = "(Line:string)";
        public static readonly TableColumns TextLinesSchema;

        // App Name and Version
        public const string ApplicationName = "Klipboard";
        public static readonly Version ApplicationVersion = new Version(0, 1, 4);

        // Notifcation Constants
        public const int PersistNotificationTime = 0;
        public const int DefaultNotificationTime = 30;

        static AppConstants()
        {
            TextLinesSchema = new TableColumns();
            TextLinesSchema.Columns.Add(("Line", KqlTypeHelper.GetTypeDedfinition(KqlDataType.StringType)));
        }
    }
    #endregion
}
