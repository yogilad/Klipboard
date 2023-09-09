namespace Klipboard.Utils
{
    #region App Constats
    public static class AppConstants
    {
        // Debug Config
#if DEBUG
        public const bool DevMode = true;
        public const bool EnforceInlineQuerySizeLimits = true;
#else
        public const DevMode = false;
        public const EnforceInlineQuerySizeLimits = true;
#endif

        // Program Constants
        public const int MaxAllowedQueryLengthKB = 12;
        public const int MaxAllowedQueryLength = MaxAllowedQueryLengthKB * 1024;
        public const int MaxAllowedDataLengthKb = MaxAllowedQueryLengthKB * 5;
        public const int MaxAllowedDataLength = MaxAllowedDataLengthKb * 1024;

        public const string UnknownFormat = "unknown";
        public const string TextLinesScheme = "(Line:string)";


        // App Name and Version
        public const string ApplicationName = "Klipboard";
        public const string ApplicationVersion = "v0.0.0";
    }
    #endregion
}
